﻿// This file is subject of copyright notice which described in SgiLicense.txt file.
// Initial contributors of this source code (SgiManager.cs): Morrolan e'Drien (Castle Black) and Mesenion (ArahnaBytes). Other contributors should be mentioned in comments.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Text;
using Microsoft.Win32;

namespace SteamGamesInstaller
{
    public delegate void CheckDepotsMethod(SteamApplication app, DirectoryInfo[] directories);
    public delegate Int64 GetFilesSizeMethod(SteamApplication app, InstallOptions installOptions);
    public delegate Int64 InstallApplicationMethod(SteamApplication app, InstallOptions installOptions, Int64 appSize, BackgroundWorker worker);

    public interface ISgiManager
    {
        String[] GetInstallableApplications();
        String[] GetInstallableLanguages(String appName);
        Int64 GetFilesSize(Object installOptions);
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        void InstallApplication(Object installOptions, BackgroundWorker worker);
    }

    public class SgiManager : ISgiManager
    {
        private List<SteamApplication> apps;
        private DirectoryInfo depotsDirectory;

        # region Code for front end.

        /// <summary>
        /// Creates instance of manager, create applications list and check installable applications in current directory.
        /// </summary>
        public SgiManager()
        {
            apps = new List<SteamApplication>();
            depotsDirectory = new DirectoryInfo(Environment.CurrentDirectory);

            PopulateApplicationsList();
        }

        /// <summary>
        /// Returns array of strings with names of installable applications which depots are present in current directory.
        /// </summary>
        /// <returns>Array of strings with names of installable applications which depots are present in current directory.</returns>
        public String[] GetInstallableApplications()
        {
            List<String> appNames = new List<String>();

            foreach (SteamApplication app in this.apps)
            {
                if (app.CheckState == CheckState.Installable)
                    appNames.Add(app.Name);
            }

            if (appNames.Count > 0)
                return appNames.ToArray();
            else
                return null;
        }

        /// <summary>
        /// Returns array of strings with installable languages for specified application which depots are present in current directory.
        /// </summary>
        /// <param name="appName">Name of the game.</param>
        /// <returns>Array of strings with languages for specified application which depots are present in current directory.</returns>
        public String[] GetInstallableLanguages(String appName)
        {
            Dictionary<String, String> languages;
            SteamApplication app = GetApplication(appName);

            languages = new Dictionary<String, String>();

            if (app != null)
            {
                for (Int32 i = 0; i < app.DepotsCount; i++)
                {
                    SteamDepot depot = app.GetDepot(i);

                    if (depot.CheckState == CheckState.Installable)
                    {
                        for (Int32 j = 0; j < depot.CulturesCount; j++)
                        {
                            CultureInfo depotCulture = depot.GetCulture(j);

                            if (!depot.IsInvariant && !languages.ContainsKey(depotCulture.EnglishName))
                                languages.Add(depotCulture.EnglishName, depotCulture.EnglishName + " (" + depotCulture.NativeName + ")");
                        }
                    }
                }

                if (languages.Count > 0)
                    return languages.Values.ToArray<String>();
            }

            return null;
        }

        /// <summary>
        /// Returns size of application files in bytes for specified installation options.
        /// </summary>
        /// <param name="installOptions">Install options.</param>
        /// <returns>Size of application files in bytes for specified installation options.</returns>
        public Int64 GetFilesSize(Object installOptions)
        {
            if (installOptions == null)
                throw new ArgumentNullException("installOptions");

            InstallOptions installOpts = (InstallOptions)installOptions;
            SteamApplication app = GetApplication(installOpts.ApplicationName);

            if (app != null)
                return app.GetFilesSize(app, installOpts);
            else
                return -1L;
        }

        /// <summary>
        /// Installs application with specified installation options in background thread.
        /// </summary>
        /// <param name="installOptions">Installation options.</param>
        /// <param name="worker">BackgroundWorker object.</param>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public void InstallApplication(Object installOptions, BackgroundWorker worker)
        {
            if (installOptions == null)
                throw new ArgumentNullException("installOptions");

            InstallOptions installOpts = (InstallOptions)installOptions;
            SteamApplication app = GetApplication(installOpts.ApplicationName);

            if (app != null && app.CheckState == CheckState.Installable)
            {
                app.InstallApplication(app, installOpts, -1L, worker);

                if (installOpts.IsUseInstallScript && !String.IsNullOrEmpty(app.InstallScriptName))
                {
                    String installScriptName = Path.Combine(Path.Combine(installOpts.InstallPath, app.InstallDirectoryName), app.InstallScriptName);
                    ValveDataFile vdf = new ValveDataFile(app, installScriptName,
                        new DirectoryInfo(Path.Combine(installOpts.InstallPath, app.InstallDirectoryName)),
                        installOpts.ApplicationCulture.EnglishName, worker);

                    vdf.ExecuteScript();
                }
            }
        }

        # endregion Code for front end.

        public DirectoryInfo DepotsDirectory
        {
            get { return depotsDirectory; }
            set { depotsDirectory = value; }
        }

        /// <summary>
        /// Returns name of directory with fixes.
        /// </summary>
        public static String FixesDirectoryName
        {
            get { return "_Fixes"; }
        }

        /// <summary>
        /// Returns name of directory with updates.
        /// </summary>
        public static String UpdatesDirectoryName
        {
            get { return "_Updates"; }
        }

        # region Games list.

        /// <summary>
        /// Populates applications list with information about Steam applications and their depots.
        /// Also finds depots for added applications.
        /// </summary>
        private void PopulateApplicationsList()
        {
            #region DEFCON
            apps.Add(new SteamApplication(1520, @"DEFCON", @"Defcon", null,
                CheckDepots, GetFilesSize, InstallApplication));
            apps[apps.Count - 1].AddDepot(new SteamDepot(1521, @"Defcon Content", @"Defcon Content", false,
                new CultureInfo[] { CultureInfo.GetCultureInfo("en"), CultureInfo.GetCultureInfo("fr"),
                CultureInfo.GetCultureInfo("it"), CultureInfo.GetCultureInfo("de"), CultureInfo.GetCultureInfo("es") }));
            #endregion DEFCON

            #region X3: Terran Conflict
            apps.Add(new SteamApplication(2820, @"X3: Terran Conflict", @"X3 Terran Conflict", "installscript.vdf",
                CheckDepots, GetFilesSize, InstallApplication));
            apps[apps.Count - 1].AddDepot(new SteamDepot(2821, @"X3: Terran Conflict content", @"X3 Terran Conflict content", false,
                new CultureInfo[] { CultureInfo.InvariantCulture }));
            apps[apps.Count - 1].AddDepot(new SteamDepot(2822, @"X3: Terran Conflict German", @"X3 Terran Conflict German", true,
                new CultureInfo[] { CultureInfo.GetCultureInfo("de") }));
            apps[apps.Count - 1].AddDepot(new SteamDepot(2823, @"X3: Terran Conflict French", @"X3 Terran Conflict French", true,
                new CultureInfo[] { CultureInfo.GetCultureInfo("fr") }));
            apps[apps.Count - 1].AddDepot(new SteamDepot(2824, @"X3: Terran Conflict Italian", @"X3 Terran Conflict Italian", true,
                new CultureInfo[] { CultureInfo.GetCultureInfo("it") }));
            apps[apps.Count - 1].AddDepot(new SteamDepot(2825, @"X3: Terran Conflict English", @"X3 Terran Conflict English", true,
                new CultureInfo[] { CultureInfo.GetCultureInfo("en") }));
            apps[apps.Count - 1].AddDepot(new SteamDepot(2826, @"X3 Soundtrack", @"X3 Soundtrack", true,
                new CultureInfo[] { CultureInfo.InvariantCulture }));
            apps[apps.Count - 1].AddDepot(new SteamDepot(2827, @"X3: A Sunny Place", @"x3tc DLC", true,
                new CultureInfo[] { CultureInfo.InvariantCulture }));
            apps[apps.Count - 1].AddDepot(new SteamDepot(2828, @"X3: Terran Conflict Russian", @"X3TC Russian", true,
                new CultureInfo[] { CultureInfo.GetCultureInfo("ru") }));
            #endregion X3: Terran Conflict

            #region The Elder Scrolls V: Skyrim
            apps.Add(new SteamApplication(72850, @"The Elder Scrolls V: Skyrim", @"Skyrim", "installscript.vdf",
                CheckDepots_72850, GetFilesSize_72850, InstallApplication_72850));
            apps[apps.Count - 1].AddDepot(new SteamDepot(72851, @"Skyrim Content", @"Skyrim Content", false,
                new CultureInfo[] { CultureInfo.InvariantCulture }));
            apps[apps.Count - 1].AddDepot(new SteamDepot(72852, @"Skyrim exe", @"Skyrim exe", true,
                new CultureInfo[] { CultureInfo.InvariantCulture }));
            apps[apps.Count - 1].AddDepot(new SteamDepot(72853, @"The Elder Scrolls V: Skyrim english", @"Skyrim english", true,
                new CultureInfo[] { CultureInfo.GetCultureInfo("en") }));
            apps[apps.Count - 1].AddDepot(new SteamDepot(72854, @"The Elder Scrolls V: Skyrim french", @"Skyrim french", true,
                new CultureInfo[] { CultureInfo.GetCultureInfo("fr") }));
            apps[apps.Count - 1].AddDepot(new SteamDepot(72855, @"The Elder Scrolls V: Skyrim italian", @"Skyrim italian", true,
                new CultureInfo[] { CultureInfo.GetCultureInfo("it") }));
            apps[apps.Count - 1].AddDepot(new SteamDepot(72856, @"The Elder Scrolls V: Skyrim german", @"Skyrim german", true,
                new CultureInfo[] { CultureInfo.GetCultureInfo("de") }));
            apps[apps.Count - 1].AddDepot(new SteamDepot(72857, @"The Elder Scrolls V: Skyrim spanish", @"Skyrim spanish", true,
                new CultureInfo[] { CultureInfo.GetCultureInfo("es") }));
            apps[apps.Count - 1].AddDepot(new SteamDepot(72858, @"The Elder Scrolls V: Skyrim Polish", @"Skyrim Polish", true,
                new CultureInfo[] { CultureInfo.GetCultureInfo("pl") }));
            apps[apps.Count - 1].AddDepot(new SteamDepot(72859, @"The Elder Scrolls V: Skyrim Czech", @"Skyrim Czech", true,
                new CultureInfo[] { CultureInfo.GetCultureInfo("cs") }));
            apps[apps.Count - 1].AddDepot(new SteamDepot(72860, @"The Elder Scrolls V: Skyrim Russian", @"Skyrim Russian", true,
                new CultureInfo[] { CultureInfo.GetCultureInfo("ru") }));
            apps[apps.Count - 1].AddDepot(new SteamDepot(72861, @"The Elder Scrolls V: Skyrim Japanese", @"The Elder Scrolls V Skyrim Japanese", true,
                new CultureInfo[] { CultureInfo.GetCultureInfo("ja") }));
            // "Skyrim Czech" and "Skyrim Polish" depots use "common\Skyrim\Data\Skyrim - Voices.bsa" and "common\Skyrim\Data\Skyrim - VoicesExtra.bsa"
            // files from "Skyrim english" depot.
            apps[apps.Count - 1].CustomObject = new String[] { @"common\Skyrim\Data\Skyrim - Voices.bsa", @"common\Skyrim\Data\Skyrim - VoicesExtra.bsa" };
            #endregion The Elder Scrolls V: Skyrim

            #region Terraria
            apps.Add(new SteamApplication(105600, @"Terraria", @"Terraria", "installscript.vdf",
                CheckDepots, GetFilesSize, InstallApplication));
            apps[apps.Count - 1].AddDepot(new SteamDepot(105601, @"TerrariaRelease", @"TerrariaRelease", false,
                new CultureInfo[] { CultureInfo.GetCultureInfo("en") }));
            #endregion Terraria

            #region X3: Albion Prelude
            apps.Add(new SteamApplication(201310, @"X3: Albion Prelude", @"x3 terran conflict", "installscript-x3ap.vdf",
                CheckDepots, GetFilesSize, InstallApplication));
            apps[apps.Count - 1].AddDepot(new SteamDepot(201311, @"X3: Albion Prelude content", @"x3ap content", false,
                new CultureInfo[] { CultureInfo.InvariantCulture }));
            apps[apps.Count - 1].AddDepot(new SteamDepot(201312, @"X3: Albion Prelude english", @"X3AP english", true,
                new CultureInfo[] { CultureInfo.GetCultureInfo("en") }));
            apps[apps.Count - 1].AddDepot(new SteamDepot(201313, @"X3: Albion Prelude german", @"X3AP german", true,
                new CultureInfo[] { CultureInfo.GetCultureInfo("de") }));
            apps[apps.Count - 1].AddDepot(new SteamDepot(201314, @"X3: Albion Prelude french", @"X3AP french", true,
                new CultureInfo[] { CultureInfo.GetCultureInfo("fr") }));
            apps[apps.Count - 1].AddDepot(new SteamDepot(201315, @"X3: Albion Prelude italian", @"X3AP italian", true,
                new CultureInfo[] { CultureInfo.GetCultureInfo("it") }));
            apps[apps.Count - 1].AddDepot(new SteamDepot(201316, @"X3: Albion Prelude Russian", @"X3AP Russian", true,
                new CultureInfo[] { CultureInfo.GetCultureInfo("ru") }));
            #endregion X3: Albion Prelude

            // Find depots for all applications
            foreach (SteamApplication app in this.apps)
                app.FindDepots(depotsDirectory.GetDirectories());
        }

        # endregion Games list.

        #region Check depots methods.

        /// <summary>
        /// Checks depots.
        /// </summary>
        /// <param name="application">SteamApplication object.</param>
        /// <param name="directories">Directories in current directory.</param>
        private static void CheckDepots(SteamApplication application, DirectoryInfo[] directories)
        {
            if (application.CheckState == CheckState.NotChecked)
            {
                Boolean isNonInvariantDepotExists = false;
                Boolean isRequiredDepotNotExists = false;

                for (Int32 i = 0; i < application.DepotsCount; i++)
                {
                    SteamDepot depot = application.GetDepot(i);

                    if (depot.GetDirectoriesCount(SteamDepotFileTypes.Common) > 0)
                    {
                        depot.CheckState = CheckState.Installable;
                        if (!depot.IsInvariant)
                            isNonInvariantDepotExists = true;
                    }

                    if (depot.CheckState != CheckState.Installable)
                    {
                        application.CheckState = CheckState.NotInstallable;
                        if (!depot.IsOptional)
                            isRequiredDepotNotExists = true;
                    }
                }

                if (isRequiredDepotNotExists || !isNonInvariantDepotExists)
                    application.CheckState = CheckState.NotInstallable;
                else
                    application.CheckState = CheckState.Installable;
            }
        }

        /// <summary>
        /// Checks depots for "The Elder Scrolls V: Skyrim".
        /// </summary>
        /// <param name="application">SteamApplication object.</param>
        /// <param name="directories">Directories in current directory.</param>
        private void CheckDepots_72850(SteamApplication application, DirectoryInfo[] directories)
        {
            if (application.CheckState == CheckState.NotChecked)
            {
                CheckDepots(application, directories);

                // Application specific scenario
                SteamDepot polish = application.GetDepotById(72858);
                SteamDepot czech = application.GetDepotById(72859);

                if (polish != null || czech != null)
                {
                    Boolean isFile1Exist = false;
                    Boolean isFile2Exist = false;
                    String file1RelativeName = ((String[])application.CustomObject)[0];
                    String file2RelativeName = ((String[])application.CustomObject)[1];
                    SteamDepotFile[] files = application.FilesMap.GetFiles(SteamDepotFileTypes.Any, CultureInfo.GetCultureInfo("en"));

                    if (files != null)
                    {
                        foreach (SteamDepotFile file in files)
                        {
                            if (!isFile1Exist && String.Compare(file.RelativeName, file1RelativeName, StringComparison.OrdinalIgnoreCase) == 0)
                                isFile1Exist = true;
                            if (!isFile2Exist && String.Compare(file.RelativeName, file2RelativeName, StringComparison.OrdinalIgnoreCase) == 0)
                                isFile2Exist = true;
                            if (isFile1Exist && isFile2Exist)
                                break;
                        }

                        if (!isFile1Exist || !isFile2Exist)
                        {
                            polish.CheckState = CheckState.NotInstallable;
                            czech.CheckState = CheckState.NotInstallable;
                        }
                    }
                }
            }
        }

        #endregion Check depots methods.

        #region Get files size methods.

        /// <summary>
        /// Returns size of application files in bytes for specified installation options.
        /// </summary>
        /// <param name="application">SteamApplication object.</param>
        /// <param name="installOptions">Installation options.</param>
        /// <returns>Size of application files in bytes for specified installation options.</returns>
        private Int64 GetFilesSize(SteamApplication application, InstallOptions installOptions)
        {
            Int64 size = 0L;
            if (installOptions.FilesType != SteamDepotFileTypes.None)
            {
                SteamDepotFile[] files = application.GetFiles(installOptions);

                if (files != null)
                {
                    foreach (SteamDepotFile file in files)
                        size += new FileInfo(file.FullName).Length;
                }
            }
            return size;
        }

        /// <summary>
        /// Returns size of "The Elder Scrolls V: Skyrim" files in bytes for specified installation options.
        /// </summary>
        /// <param name="application">SteamApplication object.</param>
        /// <param name="installOptions">Installation options.</param>
        /// <param name="filesMap">FilesMap object.</param>
        /// <returns>Size of application files in bytes for specified installation options.</returns>
        private Int64 GetFilesSize_72850(SteamApplication application, InstallOptions installOptions)
        {
            Int64 size = 0L;

            if (installOptions.FilesType != SteamDepotFileTypes.None)
            {
                size = GetFilesSize(application, installOptions);

                // Application specific scenario
                if (CultureInfo.Equals(installOptions.ApplicationCulture, CultureInfo.GetCultureInfo("pl")) ||
                    CultureInfo.Equals(installOptions.ApplicationCulture, CultureInfo.GetCultureInfo("cs")))
                {
                    SteamDepotFile file1 = null;
                    SteamDepotFile file2 = null;
                    String file1RelativeName = ((String[])application.CustomObject)[0];
                    String file2RelativeName = ((String[])application.CustomObject)[1];
                    SteamDepotFile[] files = application.FilesMap.GetFiles(installOptions.FilesType, CultureInfo.GetCultureInfo("en"));

                    if (files != null)
                    {
                        foreach (SteamDepotFile file in files)
                        {
                            if (file1 == null && String.Compare(file.RelativeName, file1RelativeName, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                file1 = file;
                                size += new FileInfo(file1.FullName).Length;
                            }
                            if (file2 == null && String.Compare(file.RelativeName, file2RelativeName, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                file2 = file;
                                size += new FileInfo(file2.FullName).Length;
                            }
                            if (file1 != null && file2 != null)
                                break;
                        }
                    }
                }
            }

            return size;
        }

        #endregion Get files size methods.

        #region Install application methods.

        /// <summary>
        /// Installs application using specified installation options.
        /// </summary>
        /// <param name="application">SteamApplication object.</param>
        /// <param name="installOptions">InstallOptions object.</param>
        /// <param name="filesSize">Size of files for installation. Method calculate this size if parameter equals to -1.</param>
        /// <param name="worker">BackgroundWorker object.</param>
        /// <returns>Size of installed files.</returns>
        private Int64 InstallApplication(SteamApplication application, InstallOptions installOptions, Int64 filesSize, BackgroundWorker worker)
        {
            Int64 copiedFilesSize = 0L;

            if (installOptions.FilesType != SteamDepotFileTypes.None)
            {
                SteamDepotFile[] files = application.GetFiles(installOptions);

                if (files != null)
                {
                    DirectoryInfo destDirectory = new DirectoryInfo(installOptions.InstallPath);

                    if (filesSize == -1L)
                        filesSize = application.GetFilesSize(application, installOptions);

                    foreach (SteamDepotFile file in files)
                    {
                        if (worker.CancellationPending)
                            break;

                        copiedFilesSize += SgiUtils.CopyFile(file, Path.Combine(destDirectory.FullName, file.RelativeName),
                            filesSize, copiedFilesSize, worker);
                    }
                }
            }

            if (filesSize == 0L)
                worker.ReportProgress(100);

            return copiedFilesSize;
        }

        /// <summary>
        /// Installs "The Elder Scrolls V: Skyrim" using specified installation options.
        /// </summary>
        /// <param name="application">SteamApplication object.</param>
        /// <param name="installOptions">InstallOptions object.</param>
        /// <param name="filesSize">Size of files for installation. Method calculate this size if parameter equals to -1.</param>
        /// <param name="worker">BackgroundWorker object.</param>
        /// <returns>Size of installed files.</returns>
        private Int64 InstallApplication_72850(SteamApplication application, InstallOptions installOptions, Int64 filesSize, BackgroundWorker worker)
        {
            Int64 installedFilesSize = 0L;

            if (installOptions.FilesType != SteamDepotFileTypes.None)
            {
                if (filesSize == -1L)
                    filesSize = application.GetFilesSize(application, installOptions);

                installedFilesSize = InstallApplication(application, installOptions, filesSize, worker);

                // Application specific scenario
                if (!worker.CancellationPending)
                {
                    if (CultureInfo.Equals(installOptions.ApplicationCulture, CultureInfo.GetCultureInfo("pl")) ||
                        CultureInfo.Equals(installOptions.ApplicationCulture, CultureInfo.GetCultureInfo("cs")))
                    {
                        SteamDepotFile file1 = null;
                        SteamDepotFile file2 = null;
                        String file1RelativeName = ((String[])application.CustomObject)[0];
                        String file2RelativeName = ((String[])application.CustomObject)[1];
                        SteamDepotFile[] files = application.FilesMap.GetFiles(installOptions.FilesType, CultureInfo.GetCultureInfo("en"));

                        if (files != null)
                        {
                            foreach (SteamDepotFile file in files)
                            {
                                if (file1 == null && String.Compare(file.RelativeName, file1RelativeName, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    file1 = file;
                                    installedFilesSize += SgiUtils.CopyFile(file1, Path.Combine(installOptions.InstallPath, file1RelativeName),
                                        filesSize, installedFilesSize, worker);
                                }
                                if (file2 == null && String.Compare(file.RelativeName, file2RelativeName, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    file2 = file;
                                    installedFilesSize += SgiUtils.CopyFile(file2, Path.Combine(installOptions.InstallPath, file2RelativeName),
                                        filesSize, installedFilesSize, worker);
                                }
                                if (file1 != null && file2 != null)
                                    break;
                            }
                        }
                    }
                }
            }

            return installedFilesSize;
        }

        #endregion Install application methods.

        private SteamApplication GetApplication(String applicationName)
        {
            foreach (SteamApplication app in this.apps)
            {
                if (String.Compare(app.Name, applicationName, StringComparison.OrdinalIgnoreCase) == 0)
                    return app;
            }

            return null;
        }
    }

    public enum CheckState
    {
        NotChecked,
        Installable,
        NotInstallable
    }

    public class SteamApplication
    {
        private Int32 id;
        private String name;
        private String installDirectory;
        private CheckDepotsMethod checkDepots;
        private GetFilesSizeMethod getFilesSize;
        private InstallApplicationMethod installApplication;
        private CheckState checkState;
        private List<SteamDepot> depots;
        private String installScriptName;
        private Object customObject;
        /// <summary>
        /// Files map for this application.
        /// Callers must not use this property directly! Use SteamApplication.FilesMap field instead.
        /// </summary>
        private static FilesMap filesMap; // use staticly for memory economy

        public SteamApplication(Int32 applicationId, String appName, String installDirectoryName, String installScriptFileName,
            CheckDepotsMethod checkDepotsMethod, GetFilesSizeMethod getFilesSizeMethod, InstallApplicationMethod installApplicationMethod)
        {
            id = applicationId;
            name = appName;
            installDirectory = Path.Combine("common", installDirectoryName);
            installScriptName = installScriptFileName;
            checkDepots = checkDepotsMethod;
            getFilesSize = getFilesSizeMethod;
            installApplication = installApplicationMethod;
            checkState = CheckState.NotChecked;
            depots = new List<SteamDepot>();
            customObject = null;
            filesMap = null;
        }

        public Int32 Id
        {
            get { return id; }
            set { id = value; }
        }

        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        public String InstallDirectoryName
        {
            get { return installDirectory; }
            set { installDirectory = value; }
        }

        public CheckDepotsMethod CheckDepots
        {
            get { return checkDepots; }
            set { checkDepots = value; }
        }

        public GetFilesSizeMethod GetFilesSize
        {
            get { return getFilesSize; }
            set { getFilesSize = value; }
        }

        public InstallApplicationMethod InstallApplication
        {
            get { return installApplication; }
            set { installApplication = value; }
        }

        public CheckState CheckState
        {
            get { return checkState; }
            set { checkState = value; }
        }

        public String InstallScriptName
        {
            get { return installScriptName; }
            set { installScriptName = value; }
        }

        public Object CustomObject
        {
            get { return customObject; }
            set { customObject = value; }
        }

        public FilesMap FilesMap
        {
            get
            {
                // Create new files map if not exist or created not for this application
                if (filesMap == null || filesMap.Application != this)
                    filesMap = new FilesMap(this);

                return filesMap;
            }
        }

        public void AddDepot(SteamDepot depot)
        {
            depots.Add(depot);
        }

        public SteamDepot GetDepot(Int32 index)
        {
            return depots[index];
        }

        public Int32 DepotsCount
        {
            get { return depots.Count; }
        }

        public SteamDepot GetDepotByFileName(String depotFileName)
        {
            depotFileName = SteamDepot.TrimDirectoryVersion(depotFileName);

            foreach (SteamDepot depot in depots)
            {
                if (String.Compare(depot.FileName, depotFileName, StringComparison.OrdinalIgnoreCase) == 0)
                    return depot;
            }

            return null;
        }

        public SteamDepot GetDepotById(Int32 depotId)
        {
            foreach (SteamDepot depot in depots)
            {
                if (depot.Id == depotId)
                    return depot;
            }

            return null;
        }

        public void FindDepots(DirectoryInfo[] directories)
        {
            foreach (SteamDepot depot in depots)
                depot.FindDepot(directories);

            CheckDepots(this, directories);
        }

        public SteamDepotFile[] GetFiles(InstallOptions installOptions)
        {
            if (installOptions == null)
                throw new ArgumentNullException("installOptions");

            SteamDepotFileTypes filesType = installOptions.FilesType;

            if (filesType != SteamDepotFileTypes.None)
            {
                return FilesMap.GetFiles(filesType, installOptions.ApplicationCulture);
            }
            else
                return null;
        }
    }

    public class SteamDepot
    {
        private Int32 id;
        private Boolean isOptional;
        private String name;
        private String fileName;
        private List<CultureInfo> cultures;
        private CheckState checkState;
        private Dictionary<Int32, DirectoryInfo> commonDirectories;
        private Dictionary<Int32, DirectoryInfo> fixesDirectories;

        public SteamDepot(Int32 depotId, String depotName, String depotFileName, Boolean isDepotOptional, CultureInfo[] depotCultures)
        {
            if (String.IsNullOrEmpty(depotFileName))
                throw new ArgumentNullException("depotFileName");
            if (depotCultures == null)
                throw new ArgumentNullException("depotCultures");
            if (depotCultures.Length < 1)
                throw new ArgumentException("Depot must have at least one culture.", "depotCultures");

            id = depotId;
            name = depotName;
            fileName = depotFileName;
            isOptional = isDepotOptional;
            cultures = new List<CultureInfo>(1);

            foreach (CultureInfo culture in depotCultures)
                cultures.Add(culture);

            checkState = CheckState.NotChecked;
            commonDirectories = new Dictionary<Int32, DirectoryInfo>(0);
            fixesDirectories = new Dictionary<Int32, DirectoryInfo>(0);
        }

        public Int32 Id
        {
            get { return id; }
            set { id = value; }
        }

        public Boolean IsOptional
        {
            get { return isOptional; }
            set { isOptional = value; }
        }

        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        public String FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        public CultureInfo GetCulture(Int32 index)
        {
            return cultures[index];
        }

        public Int32 CulturesCount
        {
            get { return cultures.Count; }
        }

        public CheckState CheckState
        {
            get { return checkState; }
            set { checkState = value; }
        }

        public void FindDepot(DirectoryInfo[] directories)
        {
            if (directories == null)
                throw new ArgumentNullException("directories");

            // Add common directories
            AddDirectories(directories, SteamDepotFileTypes.Common);

            // Add fixes directories if finded directory with fixes
            foreach (DirectoryInfo directory in directories)
            {
                if (String.Compare(directory.Name, SgiManager.FixesDirectoryName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    AddDirectories(directory.GetDirectories(), SteamDepotFileTypes.Fix);
                    break;
                }
            }
        }

        public void AddDirectories(DirectoryInfo[] directories, SteamDepotFileTypes filesType)
        {
            if (directories == null)
                throw new ArgumentNullException("directories");

            Dictionary<Int32, DirectoryInfo> depotDirectories;

            if (filesType == SteamDepotFileTypes.Common)
                depotDirectories = commonDirectories;
            else if (filesType == SteamDepotFileTypes.Fix)
                depotDirectories = fixesDirectories;
            else // TODO: updates system
                throw new ArgumentException("Invalid files type.", "filesType");

            foreach (DirectoryInfo directory in directories)
            {
                if (String.Compare(SteamDepot.TrimDirectoryVersion(directory.Name), fileName, StringComparison.OrdinalIgnoreCase) == 0)
                    depotDirectories.Add(GetDirectoryVersion(directory.Name), directory);
            }
        }

        public DirectoryInfo GetDirectory(Int32 filesVersion, SteamDepotFileTypes filesType)
        {
            if (filesType == SteamDepotFileTypes.Common && commonDirectories.ContainsKey(filesVersion))
                return commonDirectories[filesVersion];
            else if (filesType == SteamDepotFileTypes.Fix && fixesDirectories.ContainsKey(filesVersion))
                return fixesDirectories[filesVersion];
            else // TODO: updates system
                return null;
        }

        public Int32 GetDirectoriesCount(SteamDepotFileTypes filesType)
        {
            if (filesType == SteamDepotFileTypes.Common)
                return commonDirectories.Count;
            else if (filesType == SteamDepotFileTypes.Fix)
                return fixesDirectories.Count;
            else // TODO: updates system
                throw new ArgumentException("Invalid files type.", "filesType");
        }

        public Int32[] GetDepotVersions(SteamDepotFileTypes filesType)
        {
            if (filesType == SteamDepotFileTypes.Common)
                return commonDirectories.Keys.ToArray();
            else if (filesType == SteamDepotFileTypes.Fix)
                return fixesDirectories.Keys.ToArray();
            else // TODO: updates system
                throw new ArgumentException("Invalid files type.", "filesType");
        }

        public Boolean IsInvariant
        {
            get
            {
                foreach (CultureInfo componentCulture in this.cultures)
                {
                    if (CultureInfo.Equals(componentCulture, CultureInfo.InvariantCulture))
                        return true;
                }

                return false;
            }
        }

        public Boolean IsHaveCulture(CultureInfo culture)
        {
            foreach (CultureInfo componentCulture in this.cultures)
            {
                if (CultureInfo.Equals(componentCulture, culture))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns latest version of files in depot with specified type.
        /// </summary>
        public Int32 GetLatestVersion(SteamDepotFileTypes filesType)
        {
            Int32 latestVersion = -1;
            Dictionary<Int32, DirectoryInfo> directories;

            if (filesType == SteamDepotFileTypes.Common)
                directories = commonDirectories;
            else if (filesType == SteamDepotFileTypes.Fix)
                directories = fixesDirectories;
            else // TODO: updates system
                throw new ArgumentException("Invalid files type.", "filesType");

            foreach (Int32 directoryVersion in directories.Keys)
            {
                if (directoryVersion >= latestVersion)
                    latestVersion = directoryVersion;
            }

            return latestVersion;
        }

        public DirectoryInfo GetLatestVersionDirectory(SteamDepotFileTypes filesType)
        {
            return GetDirectory(GetLatestVersion(filesType), filesType);
        }

        /// <summary>
        /// Returns array with files in depot of specified version and type.
        /// </summary>
        /// <param name="filesVersion">Version of files.</param>
        /// <param name="filesType">Type of files.</param>
        /// <returns>Array with files of specified version and type.</returns>
        public FileInfo[] GetFiles(Int32 filesVersion, SteamDepotFileTypes filesType)
        {
            List<FileInfo> files = new List<FileInfo>();
            Queue<DirectoryInfo> directoriesQueue = new Queue<DirectoryInfo>();
            DirectoryInfo directory = GetDirectory(filesVersion, filesType);
            DirectoryInfo[] subDirectories;
            FileInfo[] depotFiles;

            do
            {
                if (directoriesQueue.Count > 0)
                    directory = directoriesQueue.Dequeue();

                depotFiles = directory.GetFiles();

                foreach (FileInfo file in depotFiles)
                    files.Add(file);

                subDirectories = directory.GetDirectories();

                foreach (DirectoryInfo subDirectory in subDirectories)
                    directoriesQueue.Enqueue(subDirectory);
            } while (directoriesQueue.Count > 0);

            return files.ToArray();
        }

        /// <summary>
        /// Returns version of specified directory name or -1 if directory have not valid version.
        /// Examples: for "directory.17" version is 17, for "directory.v17" - 17, for "directory.V17" - 17,
        /// for "directory.-17" - -1, for "directory.extension" - -1, for "directory" - -1.
        /// </summary>
        /// <param name="directoryName">Name of directory.</param>
        /// <returns>Version of specified directory name or -1 if directory have not valid version.</returns>
        public static Int32 GetDirectoryVersion(String directoryName)
        {
            if (String.IsNullOrEmpty(directoryName))
                throw new ArgumentNullException("directoryName");

            Int32 version = -1;
            String extension = Path.GetExtension(directoryName);

            if (!String.IsNullOrEmpty(extension) && extension[0] == '.')
            {
                extension = extension.Remove(0, 1);

                if (Char.ToLowerInvariant(extension[0]) == 'v')
                    extension = extension.Remove(0, 1);

                if (String.IsNullOrEmpty(extension) || !Int32.TryParse(extension, out version) || version < 0)
                    version = -1;
            }

            return version;
        }

        /// <summary>
        /// Returns directory name without version stump.
        /// </summary>
        /// <param name="directoryName">Name of directory.</param>
        /// <returns>Directory name without version stump.</returns>
        public static String TrimDirectoryVersion(String directoryName)
        {
            if (String.IsNullOrEmpty(directoryName))
                throw new ArgumentNullException("directoryName");

            if (GetDirectoryVersion(directoryName) < 0)
                return directoryName;
            else
                return directoryName.Substring(0, directoryName.LastIndexOf('.'));
        }
    }

    public class InstallOptions
    {
        String appName;
        String path;
        CultureInfo appCulture;
        Boolean isUseInstallScript;
        Boolean isUseCommon;
        Boolean isUseFixes;

        public InstallOptions(String applicationName, String installPath, String applicationLanguage, Boolean isExecuteInstallScript,
            Boolean isInstallApplication, Boolean isInstallFixes)
        {
            if (String.IsNullOrEmpty(applicationName))
                throw new ArgumentNullException("applicationName");
            if (String.IsNullOrEmpty(installPath))
                throw new ArgumentNullException("installPath");
            if (String.IsNullOrEmpty(applicationLanguage))
                throw new ArgumentNullException("applicationLanguage");

            appName = applicationName;
            path = installPath;
            appCulture = null;

            foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.AllCultures))
            {
                if (String.Compare(ci.EnglishName + " (" + ci.NativeName + ")", applicationLanguage, StringComparison.OrdinalIgnoreCase) == 0)
                    appCulture = ci;
            }

            if (appCulture == null)
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "Unknown language {0}.", applicationLanguage), "applicationLanguage");

            isUseInstallScript = isExecuteInstallScript;
            isUseCommon = isInstallApplication;
            isUseFixes = isInstallFixes;
        }

        public String ApplicationName
        {
            get { return appName; }
            set { appName = value; }
        }

        public String InstallPath
        {
            get { return path; }
            set { path = value; }
        }

        public CultureInfo ApplicationCulture
        {
            get { return appCulture; }
            set { appCulture = value; }
        }

        public Boolean IsUseInstallScript
        {
            get { return isUseInstallScript; }
            set { isUseInstallScript = value; }
        }

        public Boolean IsUseCommon
        {
            get { return isUseCommon; }
            set { isUseCommon = value; }
        }

        public Boolean IsUseFixes
        {
            get { return isUseFixes; }
            set { isUseFixes = value; }
        }

        public SteamDepotFileTypes FilesType
        {
            get
            {
                SteamDepotFileTypes filesType = SteamDepotFileTypes.None;

                if (isUseCommon)
                    filesType |= SteamDepotFileTypes.Common;
                if (isUseFixes)
                    filesType |= SteamDepotFileTypes.Fix;

                return filesType;
            }
        }
    }

    public class FilesMap
    {
        private SteamApplication app;
        private Dictionary<String, List<SteamDepotFile>> files;

        public FilesMap(SteamApplication application)
        {
            if (application == null)
                throw new ArgumentNullException("application");

            app = application;
            files = new Dictionary<String, List<SteamDepotFile>>();

            // Add files of application
            for (Int32 i = 0; i < application.DepotsCount; i++)
            {
                SteamDepot depot = application.GetDepot(i);

                if (depot.CheckState == CheckState.Installable)
                {
                    AddFiles(depot, depot.GetLatestVersion(SteamDepotFileTypes.Common), SteamDepotFileTypes.Common);
                    AddFiles(depot, SteamDepotFileTypes.Fix);
                }
            }
        }

        public SteamApplication Application
        {
            get { return app; }
            set { app = value; }
        }

        private void AddFiles(SteamDepot depot, SteamDepotFileTypes filesType)
        {
            Int32[] versions = depot.GetDepotVersions(filesType);

            for (Int32 j = 0; j < versions.Length; j++)
                AddFiles(depot, versions[j], filesType);
        }

        public void AddFiles(SteamDepot depot, Int32 filesVersion, SteamDepotFileTypes filesType)
        {
            if (depot == null)
                throw new ArgumentNullException("depot");

            FileInfo[] depotFiles = depot.GetFiles(filesVersion, filesType);

            foreach (FileInfo file in depotFiles)
                AddFile(file, filesVersion, filesType, depot);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public void AddFile(FileInfo file, Int32 fileVersion, SteamDepotFileTypes fileType, SteamDepot depot)
        {
            if (file == null)
                throw new ArgumentNullException("file");
            if (depot == null)
                throw new ArgumentNullException("depot");

            SteamDepotFile depotFile = new SteamDepotFile(file.FullName, fileVersion, fileType, depot);
            String relativeName = depotFile.RelativeName;

            if (files.ContainsKey(relativeName))
                files[relativeName].Add(depotFile);
            else
            {
                List<SteamDepotFile> filesList = new List<SteamDepotFile>();

                filesList.Add(depotFile);
                files.Add(relativeName, filesList);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public SteamDepotFile[] GetFiles(SteamDepotFileTypes fileTypes, CultureInfo culture)
        {
            if (fileTypes == SteamDepotFileTypes.None)
                return null;

            List<SteamDepotFile> result = new List<SteamDepotFile>();

            foreach (List<SteamDepotFile> filesList in files.Values)
            {
                SteamDepotFile resultFile = null;
                Int32 fileIndex = -1;
                Int32 fileVersion = -1;
                SteamDepotFile file;
                SteamDepot depot;

                // Find index of file (file should not be fix)
                for (Int32 i = 0; i < filesList.Count; i++)
                {
                    file = filesList[i];
                    depot = file.SteamDepot;

                    if (!depot.IsOptional || depot.IsHaveCulture(culture) || depot.IsInvariant)
                    {
                        if (file.FileType != SteamDepotFileTypes.Fix)
                            fileIndex = i;
                    }
                }

                // Finded not fix file
                if (fileIndex != -1)
                {
                    for (Int32 i = fileIndex; i < filesList.Count; i++)
                    {
                        file = filesList[i];
                        depot = file.SteamDepot;

                        if (!depot.IsOptional || depot.IsHaveCulture(culture) || depot.IsInvariant)
                        {
                            if (file.FileType != SteamDepotFileTypes.Fix && file.Version > fileVersion)
                                fileVersion = file.Version;

                            if ((file.FileType & fileTypes) > 0 && file.Version == fileVersion)
                            {
                                resultFile = file;
                                // Add not fix file to result list if result file is fix and method should return not only fix files
                                if (resultFile.FileType == SteamDepotFileTypes.Fix && fileTypes != SteamDepotFileTypes.Fix)
                                    result.Add(filesList[fileIndex]);
                            }
                        }
                    }
                }
                else // Finded fix file which do not replace any common or update file
                {
                    for (Int32 i = 0; i < filesList.Count; i++)
                    {
                        file = filesList[i];
                        depot = file.SteamDepot;

                        if (!depot.IsOptional || depot.IsHaveCulture(culture) || depot.IsInvariant)
                        {
                            if ((file.FileType & fileTypes) > 0 && file.Version == depot.GetLatestVersion(SteamDepotFileTypes.Common)) // TODO: file.Version == depot latest version include update files
                                resultFile = file;
                        }
                    }
                }

                if (resultFile != null)
                    result.Add(resultFile);
            }

            if (result.Count > 0)
                return result.ToArray();
            else
                return null;
        }
    }

    public class SteamDepotFile
    {
        String name;
        Int32 version;
        SteamDepotFileTypes type;
        SteamDepot depot;

        public SteamDepotFile(String fileFullName, Int32 fileVersion, SteamDepotFileTypes fileType, SteamDepot steamDepot)
        {
            name = fileFullName;
            version = fileVersion;
            type = fileType;
            depot = steamDepot;
        }

        public String FullName
        {
            get { return name; }
            set { name = value; }
        }

        public String RelativeName
        {
            get
            {
                String relativeName = name.Remove(0, depot.GetDirectory(version, type).FullName.Length);

                if (relativeName[0] == Path.DirectorySeparatorChar)
                    relativeName = relativeName.Remove(0, 1);

                return relativeName;
            }
        }

        public Int32 Version
        {
            get { return version; }
            set { version = value; }
        }

        public SteamDepotFileTypes FileType
        {
            get { return type; }
            set { type = value; }
        }

        public SteamDepot SteamDepot
        {
            get { return depot; }
            set { depot = value; }
        }

        public override string ToString()
        {
            return name;
        }
    }

    [Flags]
    public enum SteamDepotFileTypes
    {
        None = 0,
        Common = 0x1,
        Fix = 0x2,
        Update = 0x4,
        Any = Common | Fix | Update
    }

    public static class SgiUtils
    {
        #region Methods for working with files.

        public static Int64 CopyFile(SteamDepotFile file, String destinationFileFullName, Int64 filesSize, Int64 copiedFilesSize, BackgroundWorker worker)
        {
            if (file == null)
                throw new ArgumentNullException("file");
            if (String.IsNullOrEmpty(destinationFileFullName))
                throw new ArgumentNullException("destinationFileFullName");

            if (worker != null && worker.CancellationPending)
                return 0L;

            FileInfo sourceFile = new FileInfo(file.FullName);
            Byte[] buffer = new Byte[1024 * 1024 * 16]; // 16 MB buffer
            FileInfo destFile = new FileInfo(destinationFileFullName);
            FileStream source;
            FileStream destination;
            Int64 offset = 0;
            Int32 blockSize;
            Int32 progressPercent = 0;

            // Create directory for destination file if not exist
            if (!destFile.Directory.Exists)
            {
                destFile.Directory.Create();
                destFile.Directory.Attributes = sourceFile.Directory.Attributes;
                destFile.Directory.CreationTimeUtc = sourceFile.Directory.CreationTimeUtc;
            }

            // If destination file allready exists
            if (destFile.Exists)
            {
                if (file.FileType == SteamDepotFileTypes.Fix)
                {
                    FileInfo originalFile = new FileInfo(destinationFileFullName + ".original");

                    if (originalFile.Exists)
                    {
                        if (originalFile.IsReadOnly)
                            originalFile.IsReadOnly = false;
                        originalFile.Delete();
                    }

                    File.Move(destinationFileFullName, originalFile.FullName);
                }
                else if (destFile.IsReadOnly)
                    destFile.IsReadOnly = false;
            }

            source = sourceFile.OpenRead();
            destination = destFile.Create();

            if (filesSize > 0)
                progressPercent = (Int32)((float)copiedFilesSize / filesSize * 100);

            while (offset < sourceFile.Length)
            {
                if (worker != null && worker.CancellationPending)
                    break;

                if (offset + buffer.Length <= sourceFile.Length)
                    blockSize = buffer.Length;
                else
                    blockSize = (Int32)(sourceFile.Length - offset);

                source.Read(buffer, 0, blockSize);

                destination.Write(buffer, 0, blockSize);

                offset += blockSize;
                copiedFilesSize += blockSize;

                // Avoid spaming parent thread with report messages
                if (worker != null && filesSize > 0 && (Int32)((float)copiedFilesSize / filesSize * 100) > progressPercent)
                {
                    progressPercent = (Int32)((float)copiedFilesSize / filesSize * 100);
                    worker.ReportProgress(progressPercent);
                }
            }

            source.Close();
            destination.Close();

            destFile.Attributes = sourceFile.Attributes;
            destFile.CreationTimeUtc = sourceFile.CreationTimeUtc;
            destFile.LastAccessTimeUtc = sourceFile.LastAccessTimeUtc;
            destFile.LastWriteTimeUtc = sourceFile.LastWriteTimeUtc;

            return offset;
        }

        #endregion Methods for working with files.

        #region Methods for working with directories.

        // .NET 4.0 implementation of System.Environment.GetFolderPath method
        public static String GetFolderPath(SgiSpecialFolder folder)
        {
            return SgiUtils.GetFolderPath(folder, SgiSpecialFolderOption.None);
        }

        // .NET 4.0 implementation of System.Environment.GetFolderPath method
        public static String GetFolderPath(SgiSpecialFolder folder, SgiSpecialFolderOption option)
        {
            if (!Enum.IsDefined(typeof(SgiSpecialFolder), folder))
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "Illegal enum value: {0}.", (Int32)folder));
            if (!Enum.IsDefined(typeof(SgiSpecialFolderOption), option))
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "Illegal enum value: {0}.", (Int32)option));
            if (option == SgiSpecialFolderOption.Create)
                new FileIOPermission(PermissionState.None) { AllFiles = FileIOPermissionAccess.Write }.Demand();

            StringBuilder lpszPath = new StringBuilder(260); // MAX_PATH (260)
            Int32 num = SafeNativeMethods.SHGetFolderPath(IntPtr.Zero, (Int32)(folder | ((SgiSpecialFolder)((Int32)option))), IntPtr.Zero, 0, lpszPath);

            if (num < 0)
            {
                switch (num)
                {
                    case -2146233031: // COR_E_PLATFORMNOTSUPPORTED (0x80131539)
                        throw new PlatformNotSupportedException();
                }
            }

            String path = lpszPath.ToString();
            new FileIOPermission(FileIOPermissionAccess.PathDiscovery, path).Demand();

            return path;
        }

        public static void CopyDirectory(String sourcePath, String destinationPath, Boolean isCopyDirectoryContentOnly, Boolean isBackup, BackgroundWorker worker)
        {
            if (String.IsNullOrEmpty(sourcePath))
                throw new ArgumentNullException("sourcePath");
            if (String.IsNullOrEmpty(destinationPath))
                throw new ArgumentNullException("destinationPath");

            DirectoryInfo sourceDirectory = new DirectoryInfo(sourcePath);
            DirectoryInfo destDirectory = new DirectoryInfo(destinationPath);
            Queue<DirectoryInfo> directoriesQueue = new Queue<DirectoryInfo>();

            if (!isCopyDirectoryContentOnly)
            {
                destDirectory.Create();
                destDirectory.Attributes = sourceDirectory.Attributes;
            }

            do
            {
                if (directoriesQueue.Count > 0)
                {
                    sourceDirectory = directoriesQueue.Dequeue();
                    destDirectory = new DirectoryInfo(Path.Combine(destinationPath, sourceDirectory.FullName.Remove(0, sourcePath.Length + 1)));
                    destDirectory.Create();
                    destDirectory.Attributes = sourceDirectory.Attributes;
                }

                FileInfo[] files = sourceDirectory.GetFiles();

                foreach (FileInfo file in files)
                {
                    if (worker != null && worker.CancellationPending)
                        break;

                    String destFileName = Path.Combine(destDirectory.FullName, file.Name);

                    if (File.Exists(destFileName))
                    {
                        if (isBackup)
                        {
                            FileInfo originalFile = new FileInfo(destFileName + ".original");

                            if (originalFile.Exists)
                            {
                                if (originalFile.IsReadOnly)
                                    originalFile.IsReadOnly = false;
                                originalFile.Delete();
                            }

                            File.Move(destFileName, originalFile.FullName);
                        }
                        else
                        {
                            FileInfo destFile = new FileInfo(destFileName);

                            if (destFile.IsReadOnly)
                                destFile.IsReadOnly = false;

                            destFile.Delete();
                        }
                    }

                    file.CopyTo(destFileName, true);
                }

                DirectoryInfo[] subDirectories = sourceDirectory.GetDirectories();

                foreach (DirectoryInfo subDirectory in subDirectories)
                {
                    directoriesQueue.Enqueue(subDirectory);
                }
            } while (directoriesQueue.Count > 0 && (worker == null || (worker != null && !worker.CancellationPending)));
        }

        #endregion Methods for working with directories.

        #region Methods for working with strings.

        public static String Unescape(String input)
        {
            if (String.IsNullOrEmpty(input))
                throw new ArgumentNullException("input");

            if (input.Length <= 1)
                return input;

            Char[] buffer = new Char[input.Length];
            Int32 bufferIndex = 0;

            for (Int32 i = 0; i < input.Length; i++)
            {
                Char ch = input[i];

                if (ch == '\\')
                {
                    if (i + 1 < input.Length)
                    {
                        #region switch
                        switch (input[i + 1])
                        {
                            case '\a':
                                ch = '\a';
                                i++;
                                break;
                            case '\b':
                                ch = '\b';
                                i++;
                                break;
                            case '\f':
                                ch = '\f';
                                i++;
                                break;
                            case '\n':
                                ch = '\n';
                                i++;
                                break;
                            case '\r':
                                ch = '\r';
                                i++;
                                break;
                            case '\t':
                                ch = '\t';
                                i++;
                                break;
                            case '\v':
                                ch = '\v';
                                i++;
                                break;
                            case '\'':
                                ch = '\'';
                                i++;
                                break;
                            case '\"':
                                ch = '\"';
                                i++;
                                break;
                            case '\\':
                                i++;
                                break;
                            default:
                                buffer[bufferIndex++] = ch;
                                ch = input[i + 1];
                                i++;
                                break;
                        }
                        #endregion switch
                    }
                }

                buffer[bufferIndex++] = ch;
            }

            return new String(buffer, 0, bufferIndex);
        }

        #endregion Methods for working with strings.

        #region Methods for working with registry.

        // NOTE for future implementations of registry methods: for .NET 4.0 use Microsoft.Win32.RegistryKey.OpenBaseKey method

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static RegistryKey RegistryOpenKey(String key, RegistryView registryView)
        {
            return SgiUtils.RegistryGetKey(key, registryView, RegistryOperation.OpenKey);
        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static RegistryKey RegistryCreateKey(String key, RegistryView registryView)
        {
            return SgiUtils.RegistryGetKey(key, registryView, RegistryOperation.CreateKey);
        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static RegistryKey RegistryGetKey(String key, RegistryView registryView, RegistryOperation operation)
        {
            if (String.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            RegistryKey baseRegistryKey = RegistryGetBaseKey(key);

            if (baseRegistryKey == null)
                return null;

            if (key.IndexOf('\\') == -1 || (key.IndexOf('\\') != -1 && key.IndexOf('\\') + 1 >= key.Length))
                return baseRegistryKey;

            if (SgiUtils.GetRegistryKeyDangerousHandle(baseRegistryKey) == IntPtr.Zero)
                return null;

            String subkeyName = key.Substring(key.IndexOf('\\') + 1);
            RegistryKeySecurityAndAccessRightsKey registryViewKey;

            if (registryView == RegistryView.Registry64)
                registryViewKey = RegistryKeySecurityAndAccessRightsKey.Wow6464Key;
            else if (registryView == RegistryView.Registry32)
                registryViewKey = RegistryKeySecurityAndAccessRightsKey.Wow6432Key;
            else
                registryViewKey = RegistryKeySecurityAndAccessRightsKey.None;

            Int32 subKeyHandle;
            RegistryResultDisposition disposition = RegistryResultDisposition.None;
            Int32 result;

            switch (operation)
            {
                case RegistryOperation.CreateKey:
                    result = UnsafeNativeMethods.RegCreateKeyEx(GetRegistryKeyDangerousHandle(baseRegistryKey), subkeyName, 0, IntPtr.Zero, 0,
                        (Int32)(RegistryKeySecurityAndAccessRightsKey.AllAccess | registryViewKey), IntPtr.Zero, out subKeyHandle, out disposition);
                    break;
                case RegistryOperation.OpenKey:
                    result = UnsafeNativeMethods.RegOpenKeyEx(GetRegistryKeyDangerousHandle(baseRegistryKey), subkeyName, 0,
                        (Int32)(RegistryKeySecurityAndAccessRightsKey.AllAccess | registryViewKey), out subKeyHandle);
                    break;
                default:
                    return null;
            }

            if (result != 0) // Oh noooes! Unknown error :(
                return null;

            RegistryKey registryKey = SubkeyHandleToRegistryKey((IntPtr)subKeyHandle, true, false);

            if (disposition == RegistryResultDisposition.CreatedNewKey)
                registryKey.Flush();

            // Fix name of registryKey cuz it is empty and return
            return RegistryKeyFixName(registryKey, baseRegistryKey, subkeyName);
        }

        private static RegistryKey RegistryKeyFixName(RegistryKey registryKey, RegistryKey baseRegistryKey, String subkeyName)
        {
            if (registryKey == null)
                throw new ArgumentNullException("registryKey");
            if (baseRegistryKey == null)
                throw new ArgumentNullException("baseRegistryKey");
            if (String.IsNullOrEmpty(subkeyName))
                throw new ArgumentNullException("subkeyName");

            //Get the FieldInfo of the 'keyName' member of RegistryKey
            FieldInfo fieldInfo = typeof(RegistryKey).GetField("keyName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            //Set new name for registryKey
            fieldInfo.SetValue(registryKey, baseRegistryKey.Name + @"\" + subkeyName);

            return registryKey;
        }

        public static RegistryKey RegistryGetBaseKey(String key)
        {
            if (String.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            String rootKeyName;

            if (key.IndexOf('\\') == -1)
                rootKeyName = key;
            else
                rootKeyName = key.Substring(0, key.IndexOf('\\'));

            if (String.Compare(rootKeyName, Registry.LocalMachine.Name, StringComparison.OrdinalIgnoreCase) == 0)
                return Registry.LocalMachine;
            else if (String.Compare(rootKeyName, Registry.ClassesRoot.Name, StringComparison.OrdinalIgnoreCase) == 0)
                return Registry.ClassesRoot;
            else if (String.Compare(rootKeyName, Registry.CurrentConfig.Name, StringComparison.OrdinalIgnoreCase) == 0)
                return Registry.CurrentConfig;
            else if (String.Compare(rootKeyName, Registry.CurrentUser.Name, StringComparison.OrdinalIgnoreCase) == 0)
                return Registry.CurrentUser;
            else if (String.Compare(rootKeyName, Registry.DynData.Name, StringComparison.OrdinalIgnoreCase) == 0)
                return Registry.DynData;
            else if (String.Compare(rootKeyName, Registry.PerformanceData.Name, StringComparison.OrdinalIgnoreCase) == 0)
                return Registry.PerformanceData;
            else if (String.Compare(rootKeyName, Registry.Users.Name, StringComparison.OrdinalIgnoreCase) == 0)
                return Registry.Users;
            else
                return null;
        }

        /// <summary>
        /// Get a pointer to a registry key.
        /// </summary>
        /// <param name="registryKey">Registry key to obtain the pointer of.</param>
        /// <returns>Pointer to the given registry key.</returns>
        /// Published at http://www.pinvoke.net/default.aspx/advapi32/RegOpenKeyEx.html
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Runtime.InteropServices.SafeHandle.DangerousGetHandle"),
        EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        private static IntPtr GetRegistryKeyDangerousHandle(RegistryKey key)
        {
            //Get the FieldInfo of the 'hkey' member of RegistryKey
            FieldInfo fieldInfo = typeof(RegistryKey).GetField("hkey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            //Get the handle held by hkey
            SafeHandle handle = (SafeHandle)fieldInfo.GetValue(key);

            //Get the unsafe handle
            return handle.DangerousGetHandle();
        }

        /// <summary>
        /// Get a registry key from a pointer.
        /// </summary>
        /// <param name="keyHandle">Pointer to the registry key</param>
        /// <param name="writable">Whether or not the key is writable.</param>
        /// <param name="ownsHandle">Whether or not we own the handle.</param>
        /// <returns>Registry key pointed to by the given pointer.</returns>
        /// Published at http://www.pinvoke.net/default.aspx/advapi32/RegOpenKeyEx.html
        private static RegistryKey SubkeyHandleToRegistryKey(IntPtr keyHandle, Boolean writable, Boolean ownsHandle)
        {
            //Get the BindingFlags for private contructors
            BindingFlags privateConstructors = BindingFlags.Instance | BindingFlags.NonPublic;

            //Get the Type for the SafeRegistryHandle
            Type safeRegistryHandleType =
                typeof(Microsoft.Win32.SafeHandles.SafeHandleZeroOrMinusOneIsInvalid).Assembly.GetType("Microsoft.Win32.SafeHandles.SafeRegistryHandle");

            //Get the array of types matching the args of the ctor we want
            Type[] safeRegistryHandleCtorTypes = new Type[] { typeof(IntPtr), typeof(Boolean) };

            //Get the constructorinfo for our object
            System.Reflection.ConstructorInfo safeRegistryHandleCtorInfo = safeRegistryHandleType.GetConstructor(privateConstructors, null, safeRegistryHandleCtorTypes, null);

            //Invoke the constructor, getting us a SafeRegistryHandle
            Object safeHandle = safeRegistryHandleCtorInfo.Invoke(new Object[] { keyHandle, ownsHandle });

            //Get the type of a RegistryKey
            Type registryKeyType = typeof(RegistryKey);

            //Get the array of types matching the args of the ctor we want
            Type[] registryKeyConstructorTypes = new Type[] { safeRegistryHandleType, typeof(Boolean) };

            //Get the constructorinfo for our object
            System.Reflection.ConstructorInfo registryKeyCtorInfo = registryKeyType.GetConstructor(privateConstructors, null, registryKeyConstructorTypes, null);

            //Invoke the constructor, getting us a RegistryKey
            RegistryKey resultKey = (RegistryKey)registryKeyCtorInfo.Invoke(new Object[] { safeHandle, writable });

            //return the resulting key
            return resultKey;
        }

        #endregion Methods for working registry.
    }

    // The developer must take responsibility for assuring that the transition into unmanaged code is sufficiently protected by other means!
    [SuppressUnmanagedCodeSecurity]
    internal static class UnsafeNativeMethods
    {
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        internal static extern Int32 RegOpenKeyEx(IntPtr baseKeyHandle, String subkey, Int32 reserved, Int32 desired, out Int32 result);
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        internal static extern Int32 RegCreateKeyEx(IntPtr baseKeyHandle, String subkey, Int32 reserved, IntPtr keyClassType,
            Int32 options, Int32 desired, IntPtr securityAttributes, out Int32 result, out RegistryResultDisposition disposition);
    }

    [SuppressUnmanagedCodeSecurity]
    internal static class SafeNativeMethods
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2205:UseManagedEquivalentsOfWin32Api"), // WTF?! System.Environment.GetFolderPath method functioanlity in .NET 3.5 is not equal to native SHGetFolderPath
        DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        internal static extern Int32 SHGetFolderPath(IntPtr hwndOwner, Int32 nFolder, IntPtr hToken, Int32 dwFlags, StringBuilder lpszPath);
    }

    // .NET 4.0 implementation of System.Environment.SpecialFolder
    public enum SgiSpecialFolder
    {
        AdminTools = 0x30,
        ApplicationData = 0x1a,
        CDBurning = 0x3b,
        CommonAdminTools = 0x2f,
        CommonApplicationData = 0x23,
        CommonDesktopDirectory = 0x19,
        CommonDocuments = 0x2e,
        CommonMusic = 0x35,
        CommonOemLinks = 0x3a,
        CommonPictures = 0x36,
        CommonProgramFiles = 0x2b,
        CommonProgramFilesX86 = 0x2c,
        CommonPrograms = 0x17,
        CommonStartMenu = 0x16,
        CommonStartup = 0x18,
        CommonTemplates = 0x2d,
        CommonVideos = 0x37,
        Cookies = 0x21,
        Desktop = 0,
        DesktopDirectory = 0x10,
        Favorites = 6,
        Fonts = 20,
        History = 0x22,
        InternetCache = 0x20,
        LocalApplicationData = 0x1c,
        LocalizedResources = 0x39,
        MyComputer = 0x11,
        MyDocuments = 5,
        MyMusic = 13,
        MyPictures = 0x27,
        MyVideos = 14,
        NetworkShortcuts = 0x13,
        Personal = 5,
        PrinterShortcuts = 0x1b,
        ProgramFiles = 0x26,
        ProgramFilesX86 = 0x2a,
        Programs = 2,
        Recent = 8,
        Resources = 0x38,
        SendTo = 9,
        StartMenu = 11,
        Startup = 7,
        System = 0x25,
        SystemX86 = 0x29,
        Templates = 0x15,
        UserProfile = 40,
        Windows = 0x24
    }

    // .NET 4.0 implementation of System.Environment.SpecialFolderOption
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags")]
    public enum SgiSpecialFolderOption
    {
        Create = 0x8000,
        DoNotVerify = 0x4000,
        None = 0
    }

    public enum RegistryOperation
    {
        OpenKey,
        CreateKey
    }

    public enum RegistryResultDisposition
    {
        None = 0,
        /// <summary>
        /// REG_CREATED_NEW_KEY. The key did not exist and was created.
        /// </summary>
        CreatedNewKey,
        /// <summary>
        /// REG_OPENED_EXISTING_KEY. The key existed and was simply opened without being changed.
        /// </summary>
        OpenedExistingKey
    }

    public enum RegistryView
    {
        Default,
        Registry64,
        Registry32
    }

    // See Registry Key Security and Access Rights at http://msdn.microsoft.com/en-us/site/ms724878
    public enum RegistryKeySecurityAndAccessRightsKey
    {
        None = 0,
        /// <summary>
        /// KEY_ALL_ACCESS combines the STANDARD_RIGHTS_REQUIRED, KEY_QUERY_VALUE, KEY_SET_VALUE, KEY_CREATE_SUB_KEY,
        /// KEY_ENUMERATE_SUB_KEYS, KEY_NOTIFY, and KEY_CREATE_LINK access rights.
        /// </summary>
        AllAccess = 0xF003F,
        /// <summary>
        /// KEY_CREATE_LINK reserved for system use.
        /// </summary>
        CreateLink = 0x0020,
        /// <summary>
        /// KEY_CREATE_SUB_KEY required to create a subkey of a registry key.
        /// </summary>
        CreateSubkey = 0x0004,
        /// <summary>
        /// KEY_ENUMERATE_SUB_KEYS required to enumerate the subkeys of a registry key.
        /// </summary>
        EnumerateSubkeys = 0x0008,
        /// <summary>
        /// KEY_EXECUTE equivalent to KEY_READ.
        /// </summary>
        Execute = 0x20019,
        /// <summary>
        /// KEY_NOTIFY required to request change notifications for a registry key or for subkeys of a registry key.
        /// </summary>
        Notify = 0x0010,
        /// <summary>
        /// KEY_QUERY_VALUE required to query the values of a registry key.
        /// </summary>
        QueryValue = 0x0001,
        /// <summary>
        /// KEY_READ combines the STANDARD_RIGHTS_READ, KEY_QUERY_VALUE, KEY_ENUMERATE_SUB_KEYS, and KEY_NOTIFY values.
        /// </summary>
        Read = 0x20019,
        /// <summary>
        /// KEY_SET_VALUE required to create, delete, or set a registry value.
        /// </summary>
        SetValue = 0x0002,
        /// <summary>
        /// KEY_WOW64_32KEY indicates that an application on 64-bit Windows should operate on the 32-bit registry view.
        /// This flag is ignored by 32-bit Windows. For more information, see Accessing an Alternate Registry View
        /// (http://msdn.microsoft.com/en-us/site/aa384129).
        /// This flag must be combined using the OR operator with the other flags in this table that either
        /// query or access registry values.
        /// Windows 2000:  This flag is not supported.
        /// </summary>
        Wow6432Key = 0x0200,
        /// <summary>
        /// KEY_WOW64_64KEY indicates that an application on 64-bit Windows should operate on the 64-bit registry view.
        /// This flag is ignored by 32-bit Windows. For more information, see Accessing an Alternate Registry View
        /// (http://msdn.microsoft.com/en-us/site/aa384129).
        /// This flag must be combined using the OR operator with the other flags in this table that either
        /// query or access registry values.
        /// Windows 2000:  This flag is not supported.
        /// </summary>
        Wow6464Key = 0x0100,
        /// <summary>
        /// KEY_WRITE combines the STANDARD_RIGHTS_WRITE, KEY_SET_VALUE, and KEY_CREATE_SUB_KEY access rights.
        /// </summary>
        Write = 0x20006
    }

    public class ValveDataFile
    {
        private SteamApplication application;
        private FileInfo valveDataFile;
        private String[] script;
        private BackgroundWorker worker;
        private DirectoryInfo installDir;
        private String gameLanguage;
        private List<VdfCstNode> cst; // concrete syntax tree
        private VdfAstNode ast; // abstract syntax tree

        public ValveDataFile(SteamApplication app, String fileName, DirectoryInfo installDirectory, String language, BackgroundWorker worker)
        {
            this.application = app;
            this.valveDataFile = new FileInfo(fileName);
            this.installDir = installDirectory;
            this.gameLanguage = language;
            this.worker = worker;

            cst = new List<VdfCstNode>();
            ast = null;
        }

        public FileInfo DataFile
        {
            get { return valveDataFile; }
            set { valveDataFile = value; }
        }

        public String[] GetScript()
        {
            return script;
        }

        public void SetScript(String[] value)
        {
            script = value;
        }

        public BackgroundWorker BackgroundWorker
        {
            get { return worker; }
            set { worker = value; }
        }

        public DirectoryInfo InstallDirectory
        {
            get { return installDir; }
            set { installDir = value; }
        }

        public String ApplicationLanguage
        {
            get { return gameLanguage; }
            set { gameLanguage = value; }
        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public void ExecuteScript()
        {
            if (worker == null || !worker.CancellationPending)
            {
                script = File.ReadAllLines(valveDataFile.FullName);

                if (script != null)
                {
                    CreateCST();
                    CreateAST();
                    ExecuteAST();
                }
            }
        }

        /// <summary>
        /// Parses script and creates concrete syntax tree.
        /// </summary>
        private void CreateCST()
        {
            String valuableCharacters = @"/{}""\r\n";

            if (worker == null || !worker.CancellationPending)
            {
                for (Int32 i = 0; i < script.Length; i++)
                {
                    Int32 j = 0;

                    while (j < script[i].Length)
                    {
                        Int32 length;

                        // BlockStart node ({)
                        if (script[i][j] == '{')
                        {
                            cst.Add(new VdfCstNode(VdfSTNodeType.BlockStart, i, j, 1));
                            j++;
                        }
                        // BlockEnd node (})
                        else if (script[i][j] == '}')
                        {
                            cst.Add(new VdfCstNode(VdfSTNodeType.BlockEnd, i, j, 1));
                            j++;
                        }
                        // Comment node (//)
                        else if (script[i][j] == '/' && j + 1 < script[i].Length && script[i][j + 1] == '/')
                        {
                            length = script[i].Length - j; // to the end of line
                            cst.Add(new VdfCstNode(VdfSTNodeType.Comment, i, j, length));
                            j += length;
                        }
                        // Value node
                        else if (script[i][j] == '"')
                        {
                            if (j + 1 == script[i].Length)
                                throw new VdfSyntaxException(String.Format(CultureInfo.InvariantCulture, "Syntax error! Unexpected end of token in {0} at {1},{2}", valveDataFile.Name, i + 1, j + 1));
                            if (script[i].IndexOf('"', j + 1) < 0)
                                throw new VdfSyntaxException(String.Format(CultureInfo.InvariantCulture, "Syntax error! Unexpected end of token in {0} at {1},{2}", valveDataFile.Name, i + 1, j + 1));

                            // Process possible @"\""" strings
                            Int32 tokenInd = j + 1;

                            while (tokenInd < script[i].Length)
                            {
                                if (script[i][tokenInd] == '\\')
                                {
                                    tokenInd += 2;
                                }
                                else
                                {
                                    if (script[i][tokenInd] == '\"')
                                        break;
                                    tokenInd++;
                                }
                            }

                            if (tokenInd >= script[i].Length)
                                throw new VdfSyntaxException(String.Format(CultureInfo.InvariantCulture, "Syntax error! Unexpected end of token in {0} at {1},{2}", valveDataFile.Name, i + 1, j + 1));

                            length = tokenInd - j + 1;
                            cst.Add(new VdfCstNode(VdfSTNodeType.Value, i, j, length));
                            j += length;
                        }
                        // WhiteSpace node (all white space characters and unknown things)
                        else
                        {
                            length = 1;
                            while (j + length < script[i].Length && !valuableCharacters.Contains(script[i][j + length]))
                                length++;
                            cst.Add(new VdfCstNode(VdfSTNodeType.WhiteSpace, i, j, length));
                            j += length;
                        }
                    }

                    cst.Add(new VdfCstNode(VdfSTNodeType.NewLine, i, script[i].Length - 1, 0));
                }
            }
        }

        /// <summary>
        /// Creates abstract syntax tree from abstract syntax tree.
        /// </summary>
        private void CreateAST()
        {
            if (worker == null || !worker.CancellationPending)
            {
                Stack<VdfAstNode> astStack = new Stack<VdfAstNode>();
                VdfAstNode keyNode = new VdfAstNode(VdfSTNodeType.VdfScript, valveDataFile.FullName);
                Boolean isNextNodeHaveKey = false;

                ast = keyNode;
                astStack.Push(keyNode);

                // non recursive algorithm of creating abstract syntax tree.
                foreach (VdfCstNode cstNode in cst)
                {
                    if (cstNode.NodeType == VdfSTNodeType.Value)
                    {
                        VdfAstNode node = new VdfAstNode(cstNode.NodeType, GetNodeValue(cstNode));

                        keyNode.Add(node);

                        if (!isNextNodeHaveKey)
                        {
                            astStack.Push(keyNode);
                            keyNode = node;
                            isNextNodeHaveKey = true;
                        }
                        else
                        {
                            keyNode = astStack.Pop();
                            isNextNodeHaveKey = false;
                        }
                    }
                    else if (cstNode.NodeType == VdfSTNodeType.NewLine)
                    {
                        isNextNodeHaveKey = false;
                    }
                    else if (cstNode.NodeType == VdfSTNodeType.BlockStart)
                    {
                        isNextNodeHaveKey = false;
                    }
                    else if (cstNode.NodeType == VdfSTNodeType.BlockEnd)
                    {
                        keyNode = astStack.Pop();
                        isNextNodeHaveKey = false;
                    }
                }
            }
        }

        private String GetNodeValue(VdfCstNode node)
        {
            if (script != null && script.Length > 0)
            {
                String nodeValue = script[node.Line].Substring(node.Column, node.Length);

                if (nodeValue[0] == '\"' && nodeValue[nodeValue.Length - 1] == '\"')
                    nodeValue = nodeValue.Substring(1, nodeValue.Length - 2); // removes \" at start and end of string

                // Replace variables
                Int32 index = -1;
                String[] variables = new String[] { "%ALLUSERSPROFILE%", "%CDKEY%", "%INSTALLDIR%", "%SYSTEMROOT%", "%TEMP%", "%USERPROFILE%", "%USER_MYDOCS%", "%WINDIR%" };

                do
                {
                    Int32 variableIndex = -1;

                    for (Int32 i = 0; i < variables.Length; i++)
                    {
                        index = nodeValue.IndexOf(variables[i], StringComparison.OrdinalIgnoreCase);

                        if (index != -1)
                        {
                            variableIndex = i;
                            break;
                        }
                    }

                    if (index != -1 && variableIndex != -1)
                    {
                        // Remove variable
                        nodeValue = nodeValue.Remove(index, variables[variableIndex].Length);
                        // Insert some value
                        switch (variableIndex)
                        {
                            case 0: // %ALLUSERSPROFILE%
                                nodeValue = nodeValue.Insert(index, SgiUtils.GetFolderPath(SgiSpecialFolder.CommonDocuments));
                                break;
                            case 1: // %CDKEY%
                                // TODO: ask user input key
                                break;
                            case 2: // %INSTALLDIR%
                                nodeValue = nodeValue.Insert(index, this.installDir.FullName);
                                break;
                            case 3: // %SYSTEMROOT%
                            case 7: // %WINDIR%
                                nodeValue = nodeValue.Insert(index, SgiUtils.GetFolderPath(SgiSpecialFolder.Windows));
                                break;
                            case 4: // %TEMP%
                                nodeValue = nodeValue.Insert(index, Path.GetTempPath()); // Path.GetTempPath returns path to the temporary folder, ending with a backslash
                                break;
                            case 5: // %USERPROFILE%
                                nodeValue = nodeValue.Insert(index, SgiUtils.GetFolderPath(SgiSpecialFolder.UserProfile));
                                break;
                            case 6: // %USER_MYDOCS%
                                nodeValue = nodeValue.Insert(index, SgiUtils.GetFolderPath(SgiSpecialFolder.MyDocuments));
                                break;
                        }
                    }
                } while (index != -1);

                return SgiUtils.Unescape(nodeValue);
            }
            else
                return null;
        }

        /// <summary>
        /// Execute abstract syntax tree.
        /// </summary>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        private void ExecuteAST()
        {
            if (worker == null || !worker.CancellationPending)
            {
                if (ast.NodeType == VdfSTNodeType.VdfScript)
                {
                    for (Int32 i = 0; i < ast.Count; i++)
                    {
                        VdfAstNode node = ast[i];

                        if (String.Compare(node.Value, "InstallScript", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            ExecuteInstallScript(node);
                        }
                    }
                }
            }
        }

        // Current implementation of this method not support yet all possible tokens in installscript.vdf
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        private void ExecuteInstallScript(VdfAstNode installScriptNode)
        {
            for (Int32 i = 0; i < installScriptNode.Count; i++)
            {
                if (worker != null && worker.CancellationPending)
                    break;

                VdfAstNode node = installScriptNode[i];

                if (String.Compare(node.Value, "Registry", StringComparison.OrdinalIgnoreCase) == 0)
                    ExecuteInstallScriptRegistryNode(node);
                else if (String.Compare(node.Value, "Run Process", StringComparison.OrdinalIgnoreCase) == 0)
                    ExecuteInstallScriptRunProcessNode(node);
                else if (String.Compare(node.Value, "Copy Folders", StringComparison.OrdinalIgnoreCase) == 0)
                    ExecuteInstallScriptCopyFoldersNode(node);
            }
        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        private void ExecuteInstallScriptRegistryNode(VdfAstNode node)
        {
            for (Int32 i = 0; i < node.Count; i++)
            {
                VdfAstNode registryKeyNode = node[i];
                RegistryKey registryKey = SgiUtils.RegistryCreateKey(registryKeyNode.Value, RegistryView.Registry32);

                if (registryKey != null)
                {
                    for (Int32 j = 0; j < registryKeyNode.Count; j++)
                    {
                        VdfAstNode registryValueKindNode = registryKeyNode[j];
                        String valueKind = registryValueKindNode.Value;

                        if (!String.IsNullOrEmpty(valueKind))
                        {
                            for (Int32 k = 0; k < registryValueKindNode.Count; k++)
                            {
                                VdfAstNode registryValueNode = registryValueKindNode[k];

                                if (registryValueNode[0].Count > 0) // registryKeyValueNode.Value is language name
                                {
                                    if (String.Compare(gameLanguage, registryValueNode.Value, StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        registryValueNode = registryValueNode[0];
                                    }
                                }

                                if (registryValueNode != null)
                                {
                                    Object value;
                                    RegistryValueKind registryValueKind;

                                    if (String.Compare(valueKind, "string", StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        value = registryValueNode[0].Value;
                                        registryValueKind = RegistryValueKind.String;
                                    }
                                    else if (String.Compare(registryValueKindNode.Value, "dword", StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        Int32 integerValue;

                                        if (Int32.TryParse(registryValueNode[0].Value, out integerValue)) // TODO: dword may mean dword or data, so we should parse value (see decrypted appinfo.vdf)
                                            value = integerValue;
                                        else
                                            value = null;
                                        registryValueKind = RegistryValueKind.DWord;
                                    }
                                    else
                                    {
                                        value = null;
                                        registryValueKind = RegistryValueKind.Unknown;
                                    }

                                    if (value != null && registryValueKind != RegistryValueKind.Unknown)
                                        registryKey.SetValue(registryValueNode.Value, value, registryValueKind);
                                }
                            }
                        }
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"),
        EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        private void ExecuteInstallScriptRunProcessNode(VdfAstNode node)
        {
            for (Int32 i = 0; i < node.Count; i++)
            {
                if (worker != null && worker.CancellationPending)
                    break;

                VdfAstNode registryValueNode = node[i];
                String registryKey = @"HKEY_LOCAL_MACHINE\Software\Valve\Steam\Apps\" + application.Id.ToString();
                String processImage = null;
                String commandLine = null;
                Boolean isNoCleanUp = true;
                Boolean isIgnoreExitCode = false;

                for (Int32 j = 0; j < registryValueNode.Count; j++)
                {
                    if (String.Compare(registryValueNode[j].Value, "HasRunKey", StringComparison.OrdinalIgnoreCase) == 0)
                        registryKey = registryValueNode[j][0].Value;
                    else if (String.Compare(registryValueNode[j].Value, "process 1", StringComparison.OrdinalIgnoreCase) == 0)
                        processImage = registryValueNode[j][0].Value;
                    else if (String.Compare(registryValueNode[j].Value, "command 1", StringComparison.OrdinalIgnoreCase) == 0)
                        commandLine = registryValueNode[j][0].Value;
                    else if (String.Compare(registryValueNode[j].Value, "NoCleanUp", StringComparison.OrdinalIgnoreCase) == 0)
                        isNoCleanUp = registryValueNode[j][0].GetValueAsBoolean();
                    else if (String.Compare(registryValueNode[j].Value, "IgnoreExitCode", StringComparison.OrdinalIgnoreCase) == 0)
                        isIgnoreExitCode = registryValueNode[j][0].GetValueAsBoolean();
                }

                if (!String.IsNullOrEmpty(registryKey) && !String.IsNullOrEmpty(processImage))
                {
                    RegistryKey key = SgiUtils.RegistryCreateKey(registryKey, RegistryView.Registry32);

                    if (key != null)
                    {
                        Object keyValue = key.GetValue(registryValueNode.Value);

                        if (keyValue == null || (keyValue != null && (keyValue is Int32) && (Int32)keyValue != 1))
                        {
                            Process process = null;

                            if (!String.IsNullOrEmpty(commandLine))
                                process = Process.Start(processImage, commandLine);
                            else
                                process = Process.Start(processImage);

                            if (process != null)
                            {
                                // Wait for the associated process to exit or cancelation of worker thread
                                while (!process.WaitForExit(200) && (worker == null || !worker.CancellationPending)) { };

                                if (worker != null && worker.CancellationPending) // TODO: maybe worth to ask user what to do with process
                                    process.Kill();
                                else
                                {
                                    if (!isIgnoreExitCode && process.ExitCode != 0)
                                    {
                                        process.Close();
                                        worker.CancelAsync();
                                        throw new WarningException(String.Format(CultureInfo.InvariantCulture,
                                            "{0} exited with {1} error code!\nExecuting {2} will terminated!",
                                            processImage, process.ExitCode, valveDataFile.FullName));
                                    }
                                    else
                                        key.SetValue(registryValueNode.Value, 1, RegistryValueKind.DWord);

                                    if (!isNoCleanUp) // TODO: dunno what to do with this
                                    { }
                                }

                                process.Close();
                            }
                        }
                    }
                }
            }
        }

        private void ExecuteInstallScriptCopyFoldersNode(VdfAstNode node)
        {
            for (Int32 i = 0; i < node.Count; i++)
            {
                if (worker != null && worker.CancellationPending)
                    break;

                VdfAstNode folderNode = node[i];
                String sourcePath = null;
                String destinationPath = null;

                for (Int32 j = 0; j < folderNode.Count; j++)
                {
                    if (String.Compare(folderNode[j].Value, "SrcFolder 1", StringComparison.OrdinalIgnoreCase) == 0)
                        sourcePath = folderNode[j][0].Value;
                    else if (String.Compare(folderNode[j].Value, "DstFolder 1", StringComparison.OrdinalIgnoreCase) == 0)
                        destinationPath = folderNode[j][0].Value;
                }

                if (!String.IsNullOrEmpty(sourcePath) && !String.IsNullOrEmpty(destinationPath))
                    SgiUtils.CopyDirectory(sourcePath, destinationPath, false, false, worker);
            }
        }
    }

    [Serializable]
    public class VdfSyntaxException : Exception, ISerializable
    {
        protected VdfSyntaxException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
        public VdfSyntaxException()
            : base() { }
        public VdfSyntaxException(String message)
            : base(message) { }
        public VdfSyntaxException(String message, Exception innerException)
            : base(message, innerException) { }
    }

    /// <summary>
    /// Type of node of syntax tree for Valve Data File.
    /// </summary>
    public enum VdfSTNodeType
    {
        BlockEnd,
        BlockStart,
        Comment,
        NewLine,
        Value,
        VdfScript,
        WhiteSpace
    }

    /// <summary>
    /// Node of concrete syntax tree.
    /// </summary>
    public class VdfCstNode
    {
        private VdfSTNodeType type;
        private Int32 line; // start from zero
        private Int32 column; // start from zero
        private Int32 length;

        public VdfCstNode(VdfSTNodeType nodeType, Int32 fileLine, Int32 fileColumn, Int32 valueLength)
        {
            type = nodeType;
            line = fileLine;
            column = fileColumn;
            length = valueLength;
        }

        public VdfSTNodeType NodeType
        {
            get { return type; }
            set { type = value; }
        }

        public Int32 Line
        {
            get { return line; }
            set { line = value; }
        }

        public Int32 Column
        {
            get { return column; }
            set { column = value; }
        }

        public Int32 Length
        {
            get { return length; }
            set { length = value; }
        }

        public override string ToString()
        {
            return Enum.GetName(type.GetType(), type);
        }
    }

    public class VdfAstNode
    {
        private VdfSTNodeType type;
        private String nodeValue;
        private List<VdfAstNode> nodes;

        public VdfAstNode(VdfSTNodeType nodeType, String value)
        {
            if (String.IsNullOrEmpty(value))
                throw new ArgumentNullException("value");

            type = nodeType;
            nodeValue = value;
            nodes = new List<VdfAstNode>(1);
        }

        public VdfSTNodeType NodeType
        {
            get { return type; }
            set { type = value; }
        }

        public String Value
        {
            get { return this.nodeValue; }
            set { this.nodeValue = value; }
        }

        public VdfAstNode this[Int32 index]
        {
            get { return nodes[index]; }
        }

        public Int32 Count
        {
            get { return nodes.Count; }
        }

        public void Add(VdfAstNode node)
        {
            nodes.Add(node);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public Boolean GetValueAsBoolean()
        {
            if (String.Compare(nodeValue, "0", StringComparison.OrdinalIgnoreCase) == 0)
                return false;
            else if (String.Compare(nodeValue, "1", StringComparison.OrdinalIgnoreCase) == 0)
                return true;
            else
                throw new FormatException(nodeValue + " does not represent any form of boolean value.");
        }

        public override String ToString()
        {
            if (type == VdfSTNodeType.Value)
                return nodeValue;
            else
                return Enum.GetName(type.GetType(), type);
        }
    }
}
