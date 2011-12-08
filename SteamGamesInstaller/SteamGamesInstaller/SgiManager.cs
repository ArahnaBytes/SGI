// This file is subject of copyright notice which described in SgiLicense.txt file.
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
    public delegate void ComponentsCheckMethod(SteamGame game, DirectoryInfo[] directories);
    public delegate Int64 CalculateSizeMethod(SteamGame game, InstallOptions installOptions);
    public delegate Int64 GameInstallMethod(SteamGame game, InstallOptions installOptions, Int64 gameSize, BackgroundWorker worker);
    public delegate void FixesInstallMethod(SteamGame game, InstallOptions installOptions, BackgroundWorker worker);

    public class SgiManager
    {
        private List<SteamGame> games;

        # region Front end.

        /// <summary>
        /// Creates instance of manager and search installable components (components are directories with files, for example: "skyrim exe.v2" or "skyrim content.3" or "skyrim english") of games in curent directory.
        /// </summary>
        public SgiManager()
        {
            games = new List<SteamGame>();

            PopulateGamesList();
            CheckGames();
        }

        /// <summary>
        /// Returns array of strings with names of installable games which components are present in curent directory.
        /// </summary>
        /// <returns>Array of strings with names of installable games which components are present in curent directory.</returns>
        public String[] GetInstallableGames()
        {
            List<String> gameNames = new List<String>();

            foreach (SteamGame game in this.games)
            {
                if (game.CheckState == CheckState.Installable)
                    gameNames.Add(game.AppName);
            }

            if (gameNames.Count > 0)
                return gameNames.ToArray();
            else
                return null;
        }

        /// <summary>
        /// Returns array of strings with installable languages for specified game which components are present in curent directory.
        /// </summary>
        /// <param name="gameName">Name of the game.</param>
        /// <returns>Array of strings with languages for specified game which components are present in curent directory.</returns>
        public String[] GetInstallableLanguages(String gameName)
        {
            Dictionary<String, String> languages;
            SteamGame game = GetGame(gameName);

            languages = new Dictionary<String, String>();

            if (game != null)
            {
                for (Int32 i = 0; i < game.ComponentsCount; i++)
                {
                    GameComponent component = game.GetComponent(i);

                    if (component.CheckState == CheckState.Installable)
                    {
                        for (Int32 j = 0; j < component.ComponentCulturesCount; j++)
                        {
                            CultureInfo componentCulture = component.GetCulture(j);

                            if (!component.IsInvariant && !languages.ContainsKey(componentCulture.EnglishName))
                                languages.Add(componentCulture.EnglishName, componentCulture.EnglishName + " (" + componentCulture.NativeName + ")");
                        }
                    }
                }

                if (languages.Count > 0)
                    return languages.Values.ToArray<String>();
            }

            return null;
        }

        /// <summary>
        /// Returns size of the game in bytes.
        /// </summary>
        /// <param name="installOptions">Install options.</param>
        /// <returns>Size of the game in bytes</returns>
        public Int64 GetGameSize(InstallOptions installOptions)
        {
            if (installOptions == null)
                throw new ArgumentNullException("installOptions");

            SteamGame game = GetGame(installOptions.GameName);

            if (game != null)
                return game.CalculateSizeMethod(game, installOptions);
            else
                return -1;
        }

        /// <summary>
        /// Installs game with specified options.
        /// </summary>
        /// <param name="installOptions">Install options.</param>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public void InstallGame(InstallOptions installOptions, BackgroundWorker worker)
        {
            if (installOptions == null)
                throw new ArgumentNullException("installOptions");

            SteamGame game = GetGame(installOptions.GameName);

            if (game != null)
            {
                game.GameInstallMethod(game, installOptions, -1L, worker);


                if (!installOptions.IsUseSteam && installOptions.IsUseInstallscript && game.Installscript != null)
                {
                    ValveDataFile vdf = new ValveDataFile(game.Installscript);

                    vdf.BackgroundWorker = worker;
                    vdf.InstallDirectory = new DirectoryInfo(Path.Combine(installOptions.InstallDirectory, game.InstallDirectory));
                    vdf.GameLanguage = installOptions.Culture.EnglishName;

                    vdf.ExecuteScript();
                }

                if (installOptions.IsUseFix)
                    game.FixesInstallMethod(game, installOptions, worker);
            }
        }

        # endregion Front end.

        # region Games list.

        /// <summary>
        /// Populates games list with information about Steam games and their components.
        /// </summary>
        private void PopulateGamesList()
        {
            // The Elder Scrolls V: Skyrim
            games.Add(new SteamGame(72850, @"The Elder Scrolls V: Skyrim", @"common\Skyrim",
                ComponentsCheckMethod72850, CalculateSizeMethod72850, GameInstallMethod72850, FixesInstallDefaultMethod));
            games[games.Count - 1].AddComponent(new GameComponent(72851, true, @"Skyrim Content",
                @"Skyrim Content", new CultureInfo[] { CultureInfo.InvariantCulture }));
            games[games.Count - 1].AddComponent(new GameComponent(72852, true, @"Skyrim exe",
                @"Skyrim exe", new CultureInfo[] { CultureInfo.InvariantCulture }));
            games[games.Count - 1].AddComponent(new GameComponent(72853, false, @"The Elder Scrolls V: Skyrim english",
                @"Skyrim english", new CultureInfo[] { CultureInfo.GetCultureInfo("en") }));
            games[games.Count - 1].AddComponent(new GameComponent(72854, false, @"The Elder Scrolls V: Skyrim french",
                @"Skyrim french", new CultureInfo[] { CultureInfo.GetCultureInfo("fr") }));
            games[games.Count - 1].AddComponent(new GameComponent(72855, false, @"The Elder Scrolls V: Skyrim italian",
                @"Skyrim italian", new CultureInfo[] { CultureInfo.GetCultureInfo("it") }));
            games[games.Count - 1].AddComponent(new GameComponent(72856, false, @"The Elder Scrolls V: Skyrim german",
                @"Skyrim german", new CultureInfo[] { CultureInfo.GetCultureInfo("de") }));
            games[games.Count - 1].AddComponent(new GameComponent(72857, false, @"The Elder Scrolls V: Skyrim spanish",
                @"Skyrim spanish", new CultureInfo[] { CultureInfo.GetCultureInfo("es") }));
            games[games.Count - 1].AddComponent(new GameComponent(72858, false, @"The Elder Scrolls V: Skyrim Polish",
                @"Skyrim Polish", new CultureInfo[] { CultureInfo.GetCultureInfo("pl") }));
            games[games.Count - 1].AddComponent(new GameComponent(72859, false, @"The Elder Scrolls V: Skyrim Czech",
                @"Skyrim Czech", new CultureInfo[] { CultureInfo.GetCultureInfo("cs") }));
            games[games.Count - 1].AddComponent(new GameComponent(72860, false, @"The Elder Scrolls V: Skyrim Russian",
                @"Skyrim Russian", new CultureInfo[] { CultureInfo.GetCultureInfo("ru") }));
            games[games.Count - 1].AddComponent(new GameComponent(72861, false, @"The Elder Scrolls V: Skyrim Japanese",
                @"The Elder Scrolls V Skyrim Japanese", new CultureInfo[] { CultureInfo.GetCultureInfo("ja") }));
            // "Skyrim Czech" and "Skyrim Polish" components use "common\Skyrim\Data\Skyrim - Voices.bsa" and "common\Skyrim\Data\Skyrim - VoicesExtra.bsa"
            // files from "Skyrim english" component.
            games[games.Count - 1].CustomObject = new String[] { @"common\Skyrim\Data\Skyrim - Voices.bsa", @"common\Skyrim\Data\Skyrim - VoicesExtra.bsa" };

            // Terraria
            games.Add(new SteamGame(105600, @"Terraria", @"common\Terraria",
                ComponentsCheckDefaultMethod, CalculateSizeDefaultMethod, GameInstallDefaultMethod, FixesInstallDefaultMethod));
            games[games.Count - 1].AddComponent(new GameComponent(105601, true, @"TerrariaRelease",
                @"TerrariaRelease", new CultureInfo[] { CultureInfo.GetCultureInfo("en") }));
        }

        # endregion Games list.

        #region Component check methods.

        /// <summary>
        /// Default method for checking game components.
        /// </summary>
        /// <param name="game">SteamGame object.</param>
        /// <param name="directories">Directories in curent directory.</param>
        private static void ComponentsCheckDefaultMethod(SteamGame game, DirectoryInfo[] directories)
        {
            Boolean isNonInvariantComponentExists = false;
            Boolean isNotInstallableRequiredComponentExists = false;

            for (Int32 i = 0; i < game.ComponentsCount; i++)
            {
                GameComponent component = game.GetComponent(i);

                foreach (DirectoryInfo directory in directories)
                {
                    if (String.Compare(component.NcfFileName, SgiUtils.TrimDirectoryVersion(directory.Name), StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        component.CheckState = CheckState.Installable;
                        component.AddDirectory(directory);

                        if (!component.IsInvariant)
                            isNonInvariantComponentExists = true;
                    }
                }

                if (component.CheckState != CheckState.Installable)
                {
                    game.CheckState = CheckState.NotInstallable;
                    if (component.IsRequired)
                        isNotInstallableRequiredComponentExists = true;
                }
            }

            if (isNotInstallableRequiredComponentExists || !isNonInvariantComponentExists)
                game.CheckState = CheckState.NotInstallable;
            else
                game.CheckState = CheckState.Installable;
        }

        /// <summary>
        /// Checks components for "The Elder Scrolls V: Skyrim".
        /// </summary>
        /// <param name="game">SteamGame object.</param>
        /// <param name="directories">Directories in curent directory.</param>
        private void ComponentsCheckMethod72850(SteamGame game, DirectoryInfo[] directories)
        {
            ComponentsCheckDefaultMethod(game, directories);

            // Game specific checks
            GameComponent polish = game.GetComponentByAppId(72858);
            GameComponent czech = game.GetComponentByAppId(72859);

            if (polish != null || czech != null)
            {
                Boolean isSharedFilesExists = false;
                GameComponent english = game.GetComponentByAppId(72853);

                if (english != null)
                {
                    DirectoryInfo englishDirectory = english.DirectoryWithLatestVersion;
                    String file1 = ((String[])game.CustomObject)[0];
                    String file2 = ((String[])game.CustomObject)[1];

                    if (englishDirectory != null &&
                        File.Exists(Path.Combine(englishDirectory.FullName, file1)) && File.Exists(Path.Combine(englishDirectory.FullName, file2)))
                    {
                        isSharedFilesExists = true;
                    }
                }

                if (!isSharedFilesExists)
                {
                    polish.CheckState = CheckState.NotInstallable;
                    czech.CheckState = CheckState.NotInstallable;
                }
            }
        }

        #endregion Component check methods.

        #region Calculate size of installed game methods.

        private static Int64 CalculateSizeDefaultMethod(SteamGame game, InstallOptions installOptions)
        {
            Int64 size = 0;

            DirectoryInfo[] directories = GetDirectoriesToInstall(game, installOptions);

            if (directories == null)
                return -1;

            foreach (DirectoryInfo directory in directories)
            {
                foreach (FileInfo file in directory.GetFiles("*", SearchOption.AllDirectories))
                {
                    size += file.Length;
                }
            }

            return size;
        }

        private static Int64 CalculateSizeMethod72850(SteamGame game, InstallOptions installOptions)
        {
            Int64 size = CalculateSizeDefaultMethod(game, installOptions);

            if (size != -1)
            {
                if (CultureInfo.Equals(installOptions.Culture, CultureInfo.GetCultureInfo("pl")) ||
                    CultureInfo.Equals(installOptions.Culture, CultureInfo.GetCultureInfo("cs")))
                {
                    GameComponent english = game.GetComponentByAppId(72853);

                    if (english != null)
                    {
                        DirectoryInfo englishDirectory = english.DirectoryWithLatestVersion;
                        String file1 = ((String[])game.CustomObject)[0];
                        String file2 = ((String[])game.CustomObject)[1];

                        if (englishDirectory != null &&
                            File.Exists(Path.Combine(englishDirectory.FullName, file1)) && File.Exists(Path.Combine(englishDirectory.FullName, file2)))
                        {
                            size += new FileInfo(Path.Combine(english.DirectoryWithLatestVersion.FullName, file1)).Length;
                            size += new FileInfo(Path.Combine(english.DirectoryWithLatestVersion.FullName, file2)).Length;
                        }
                    }

                }
            }

            return size;
        }

        #endregion Calculate size of installed game methods.

        #region Game install methods.

        /// <summary>
        /// Default installation method.
        /// </summary>
        /// <param name="game">SteamGame object.</param>
        /// <param name="installOptions">InstallOptions object.</param>
        /// <param name="gameSize">Size of files for installation. Method calculate this size if parameter equals to -1.</param>
        /// <param name="worker">BackgroundWorker object.</param>
        /// <returns>Size of installed files.</returns>
        private static Int64 GameInstallDefaultMethod(SteamGame game, InstallOptions installOptions, Int64 gameSize, BackgroundWorker worker)
        {
            DirectoryInfo[] directories = GetDirectoriesToInstall(game, installOptions);
            Queue<DirectoryInfo> directoriesQueue = new Queue<DirectoryInfo>();
            Int64 copiedFilesSize = 0;
            DirectoryInfo sourceDirectory;
            DirectoryInfo destDirectory;

            if (gameSize == -1)
                gameSize = game.CalculateSizeMethod(game, installOptions);
            game.Installscript = null;

            // Non-recursive algorithm
            foreach (DirectoryInfo componentDirectory in directories)
            {
                sourceDirectory = componentDirectory;
                destDirectory = new DirectoryInfo(installOptions.InstallDirectory);

                do
                {
                    if (directoriesQueue.Count > 0)
                    {
                        sourceDirectory = directoriesQueue.Dequeue();
                        destDirectory = new DirectoryInfo(Path.Combine(installOptions.InstallDirectory, sourceDirectory.FullName.Remove(0, componentDirectory.FullName.Length + 1)));
                        destDirectory.Create();
                        destDirectory.Attributes = sourceDirectory.Attributes;
                    }

                    FileInfo[] files = sourceDirectory.GetFiles();

                    foreach (FileInfo file in files)
                    {
                        if (worker.CancellationPending)
                            break;

                        copiedFilesSize += CopyFile(file, Path.Combine(destDirectory.FullName, file.Name), worker, gameSize, copiedFilesSize);

                        if (String.Compare(file.Name, "installscript.vdf", StringComparison.OrdinalIgnoreCase) == 0)
                            game.Installscript = new FileInfo(Path.Combine(destDirectory.FullName, file.Name));
                    }

                    DirectoryInfo[] subDirectories = sourceDirectory.GetDirectories();

                    foreach (DirectoryInfo subDirectory in subDirectories)
                    {
                        directoriesQueue.Enqueue(subDirectory);
                    }
                } while (directoriesQueue.Count > 0 && !worker.CancellationPending);

                if (worker.CancellationPending)
                    break;
            }

            return copiedFilesSize;
        }

        /// <summary>
        /// Installation method for "The Elder Scrolls V: Skyrim".
        /// </summary>
        /// <param name="game">SteamGame object.</param>
        /// <param name="installOptions">InstallOptions object.</param>
        /// <param name="gameSize">Size of files for installation. Method calculate this size if parameter equals to -1.</param>
        /// <param name="worker">BackgroundWorker object.</param>
        /// <returns>Size of installed files.</returns>
        private Int64 GameInstallMethod72850(SteamGame game, InstallOptions installOptions, Int64 gameSize, BackgroundWorker worker)
        {
            if (gameSize == -1)
                gameSize = game.CalculateSizeMethod(game, installOptions);

            Int64 installedFilesSize = GameInstallDefaultMethod(game, installOptions, gameSize, worker);

            if (!worker.CancellationPending)
            {
                if (CultureInfo.Equals(installOptions.Culture, CultureInfo.GetCultureInfo("pl")) ||
                    CultureInfo.Equals(installOptions.Culture, CultureInfo.GetCultureInfo("cs")))
                {
                    GameComponent english = game.GetComponentByAppId(72853);

                    if (english != null)
                    {
                        DirectoryInfo englishDirectory = english.DirectoryWithLatestVersion;
                        String fileName1 = ((String[])game.CustomObject)[0];
                        String fileName2 = ((String[])game.CustomObject)[1];
                        FileInfo file1 = new FileInfo(Path.Combine(english.DirectoryWithLatestVersion.FullName, fileName1));
                        FileInfo file2 = new FileInfo(Path.Combine(english.DirectoryWithLatestVersion.FullName, fileName2));

                        if (englishDirectory != null && File.Exists(file1.FullName) && File.Exists(file2.FullName))
                        {
                            installedFilesSize += CopyFile(file1, Path.Combine(installOptions.InstallDirectory, fileName1), worker, gameSize, installedFilesSize);
                            installedFilesSize += CopyFile(file2, Path.Combine(installOptions.InstallDirectory, fileName2), worker, gameSize, installedFilesSize);
                        }
                    }
                }
            }

            return installedFilesSize;
        }

        #endregion Game install methods.

        #region Fixes install methods.

        private void FixesInstallDefaultMethod(SteamGame game, InstallOptions installOptions, BackgroundWorker worker)
        {
            DirectoryInfo fixesDirectory = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "_Fixes"));

            if (fixesDirectory.Exists)
            {
                DirectoryInfo[] fixDirectories = fixesDirectory.GetDirectories();

                if (fixDirectories != null && fixDirectories.Length > 0)
                {
                    for (Int32 i = 0; i < game.ComponentsCount; i++)
                    {
                        GameComponent component = game.GetComponent(i);

                        if (component.IsInvariant || component.IsHaveCulture(installOptions.Culture))
                        {
                            DirectoryInfo componentDirectory = component.DirectoryWithLatestVersion;

                            if (componentDirectory != null)
                            {
                                DirectoryInfo fixDirectory = null;

                                foreach (DirectoryInfo directory in fixDirectories)
                                {
                                    if (String.Compare(directory.Name, componentDirectory.Name, StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        fixDirectory = directory;
                                        break;
                                    }
                                }

                                if (fixDirectory != null)
                                    SgiUtils.CopyDirectory(fixDirectory.FullName, installOptions.InstallDirectory, true, true, worker);
                            }
                        }
                    }
                }
            }
        }

        #endregion Fixes install methods.

        private void CheckGames()
        {
            DirectoryInfo[] directories = new DirectoryInfo(Environment.CurrentDirectory).GetDirectories("*", SearchOption.TopDirectoryOnly);

            foreach (SteamGame game in this.games)
                game.ComponentsCheckMethod(game, directories);
        }

        private SteamGame GetGame(String gameName)
        {
            foreach (SteamGame game in this.games)
            {
                if (game.AppName == gameName)
                {
                    return game;
                }
            }

            return null;
        }

        private static DirectoryInfo[] GetDirectoriesToInstall(SteamGame game, InstallOptions installOptions)
        {
            List<DirectoryInfo> directories = new List<DirectoryInfo>();

            if (game.CheckState == CheckState.Installable)
            {
                // Add directories of required components and directories of components with culture specified in installOptions
                // and directories of invariant components
                for (Int32 i = 0; i < game.ComponentsCount; i++)
                {
                    GameComponent component = game.GetComponent(i);

                    if (component.CheckState == CheckState.Installable && (component.IsRequired || component.IsHaveCulture(installOptions.Culture)) || component.IsInvariant)
                    {
                        DirectoryInfo directory = component.DirectoryWithLatestVersion;

                        if (directory != null)
                            directories.Add(directory);
                    }
                }
            }

            if (directories.Count > 0)
                return directories.ToArray();
            else
                return null;
        }

        private static Int64 CopyFile(FileInfo sourceFile, string destPath, BackgroundWorker worker, Int64 gameSize, Int64 copiedFilesSize)
        {
            if (!worker.CancellationPending)
            {
                Byte[] buffer = new Byte[1024 * 1024 * 10]; // 10 MB buffer
                FileInfo destFile = new FileInfo(destPath);
                FileStream source;
                FileStream destination;
                Int64 offset = 0;
                Int32 blockSize;
                Int32 progressPercent = (Int32)((float)copiedFilesSize / gameSize * 100);

                source = sourceFile.OpenRead();
                destination = destFile.Create();

                while (offset < sourceFile.Length && !worker.CancellationPending)
                {
                    if (offset + buffer.Length <= sourceFile.Length)
                        blockSize = buffer.Length;
                    else
                        blockSize = (Int32)(sourceFile.Length - offset);

                    source.Read(buffer, 0, blockSize);

                    destination.Write(buffer, 0, blockSize);

                    offset += blockSize;
                    copiedFilesSize += blockSize;

                    // We don't want spam parent thread with report messages
                    if ((Int32)((float)copiedFilesSize / gameSize * 100) > progressPercent)
                    {
                        progressPercent = (Int32)((float)copiedFilesSize / gameSize * 100);
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
            else
                return 0;
        }
    }

    public enum CheckState
    {
        NotChecked,
        Installable,
        NotInstallable
    }

    public class SteamGame
    {
        private Int32 appId;
        private String appName;
        private String installDirectory;
        private ComponentsCheckMethod componentsCheckMethod;
        private CalculateSizeMethod calculateSizeMethod;
        private GameInstallMethod gameInstallMethod;
        private FixesInstallMethod fixesInstallMethod;
        private CheckState checkState;
        private List<GameComponent> components;
        private FileInfo installscript;
        private Object customObject;

        public SteamGame(Int32 id, String name, String directory, ComponentsCheckMethod checkMethod,
            CalculateSizeMethod calcSizeMethod, GameInstallMethod installMethod, FixesInstallMethod fixesMethod)
        {
            appId = id;
            appName = name;
            installDirectory = directory;
            componentsCheckMethod = checkMethod;
            calculateSizeMethod = calcSizeMethod;
            gameInstallMethod = installMethod;
            fixesInstallMethod = fixesMethod;
            checkState = CheckState.NotChecked;
            components = new List<GameComponent>();
            installscript = null;
            customObject = null;
        }

        public Int32 AppId
        {
            get { return appId; }
            set { appId = value; }
        }

        public String AppName
        {
            get { return appName; }
            set { appName = value; }
        }

        public String InstallDirectory
        {
            get { return installDirectory; }
            set { installDirectory = value; }
        }

        public ComponentsCheckMethod ComponentsCheckMethod
        {
            get { return componentsCheckMethod; }
            set { componentsCheckMethod = value; }
        }

        public CalculateSizeMethod CalculateSizeMethod
        {
            get { return calculateSizeMethod; }
            set { calculateSizeMethod = value; }
        }

        public GameInstallMethod GameInstallMethod
        {
            get { return gameInstallMethod; }
            set { gameInstallMethod = value; }
        }

        public FixesInstallMethod FixesInstallMethod
        {
            get { return fixesInstallMethod; }
            set { fixesInstallMethod = value; }
        }

        public CheckState CheckState
        {
            get { return checkState; }
            set { checkState = value; }
        }

        public FileInfo Installscript
        {
            get { return installscript; }
            set { installscript = value; }
        }

        public Object CustomObject
        {
            get { return customObject; }
            set { customObject = value; }
        }

        public void AddComponent(GameComponent component)
        {
            components.Add(component);
        }

        public GameComponent GetComponent(Int32 index)
        {
            return components[index];
        }

        public Int32 ComponentsCount
        {
            get { return components.Count; }
        }

        public GameComponent GetComponentByNcfFileName(String directoryName)
        {
            directoryName = SgiUtils.TrimDirectoryVersion(directoryName);

            foreach (GameComponent component in components)
            {
                if (String.Compare(component.NcfFileName, directoryName, StringComparison.OrdinalIgnoreCase) == 0)
                    return component;
            }

            return null;
        }

        public GameComponent GetComponentByAppId(Int32 applicationId)
        {
            foreach (GameComponent component in components)
            {
                if (component.AppId == applicationId)
                    return component;
            }

            return null;
        }
    }

    public class GameComponent
    {
        private Int32 appId;
        private Boolean isComponentRequired;
        private String appName;
        private String ncfFileName;
        private List<CultureInfo> componentCultures;
        private CheckState checkState;
        private List<DirectoryInfo> directories;

        public GameComponent(Int32 id, Boolean isRequired, String name, String fileName, CultureInfo[] cultures)
        {
            if (String.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName");
            if (cultures == null)
                throw new ArgumentNullException("cultures");
            if (cultures.Length < 1)
                throw new ArgumentException("Component should have at least one culture.", "cultures");

            appId = id;
            isComponentRequired = isRequired;
            appName = name;
            ncfFileName = fileName;
            componentCultures = new List<CultureInfo>(1);

            foreach (CultureInfo culture in cultures)
                componentCultures.Add(culture);

            checkState = CheckState.NotChecked;
            directories = new List<DirectoryInfo>();
        }

        public Int32 AppId
        {
            get { return appId; }
            set { appId = value; }
        }

        public Boolean IsRequired
        {
            get { return isComponentRequired; }
            set { isComponentRequired = value; }
        }

        public String AppName
        {
            get { return appName; }
            set { appName = value; }
        }

        public String NcfFileName
        {
            get { return ncfFileName; }
            set { ncfFileName = value; }
        }

        public CultureInfo GetCulture(Int32 index)
        {
            return componentCultures[index];
        }

        public Int32 ComponentCulturesCount
        {
            get { return componentCultures.Count; }
        }

        public CheckState CheckState
        {
            get { return checkState; }
            set { checkState = value; }
        }

        public void AddDirectory(DirectoryInfo directory)
        {
            directories.Add(directory);
        }

        public DirectoryInfo GetDirectory(Int32 index)
        {
            return directories[index];
        }

        public Int32 DirectoriesCount
        {
            get { return directories.Count; }
        }

        public Boolean IsInvariant
        {
            get
            {
                foreach (CultureInfo componentCulture in this.componentCultures)
                {
                    if (CultureInfo.Equals(componentCulture, CultureInfo.InvariantCulture))
                        return true;
                }

                return false;
            }
        }

        public Boolean IsHaveCulture(CultureInfo culture)
        {
            foreach (CultureInfo componentCulture in this.componentCultures)
            {
                if (CultureInfo.Equals(componentCulture, culture))
                    return true;
            }

            return false;
        }

        public DirectoryInfo DirectoryWithLatestVersion
        {
            get
            {
                Int32 latestVersion = -1;
                DirectoryInfo latestVersionDirectory = null;

                foreach (DirectoryInfo directory in this.directories)
                {
                    Int32 directoryVersion = SgiUtils.GetDirectoryVersion(directory.Name);

                    if (directoryVersion >= latestVersion)
                    {
                        latestVersion = directoryVersion;
                        latestVersionDirectory = directory;
                    }
                }

                return latestVersionDirectory;
            }
        }
    }

    public class InstallOptions
    {
        String gameName;
        Boolean isUseSteam;
        Boolean isUseInstallscript;
        Boolean isUseFix;
        String installDirectory;
        CultureInfo culture;

        public InstallOptions(String name, Boolean isSteam, Boolean isExecuteInstallscript, Boolean isInstallFixes, String directory, String language)
        {
            gameName = name;
            isUseSteam = isSteam;
            isUseInstallscript = isExecuteInstallscript;
            isUseFix = isInstallFixes;
            installDirectory = directory;
            culture = CultureInfo.InvariantCulture;

            foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.AllCultures))
            {
                if (String.Compare(ci.EnglishName + " (" + ci.NativeName + ")", language, StringComparison.OrdinalIgnoreCase) == 0)
                    culture = ci;
            }
        }

        public String GameName
        {
            get { return gameName; }
            set { gameName = value; }
        }

        public Boolean IsUseSteam
        {
            get { return isUseSteam; }
            set { isUseSteam = value; }
        }

        public Boolean IsUseInstallscript
        {
            get { return isUseInstallscript; }
            set { isUseInstallscript = value; }
        }

        public Boolean IsUseFix
        {
            get { return isUseFix; }
            set { isUseFix = value; }
        }

        public String InstallDirectory
        {
            get { return installDirectory; }
            set { installDirectory = value; }
        }

        public CultureInfo Culture
        {
            get { return culture; }
            set { culture = value; }
        }
    }

    public static class SgiUtils
    {
        #region Methods for working with directories.

        public static String TrimDirectoryVersion(String directoryName)
        {
            if (String.IsNullOrEmpty(directoryName))
                throw new ArgumentNullException("directoryName");

            Int32 trimIndex = -1;

            if (directoryName.Contains('.') && (directoryName.LastIndexOf('.') != directoryName.Length - 1))
            {
                trimIndex = directoryName.LastIndexOf('.');
                Int32 i = directoryName.LastIndexOf('.') + 1;

                if (Char.ToLowerInvariant(directoryName[i]) == 'v')
                    i++;

                if (i == directoryName.Length) // if string ended with ".v"
                    trimIndex = -1;

                while (i < directoryName.Length)
                {
                    if (Char.IsNumber(directoryName[i]) == false)
                    {
                        trimIndex = -1;
                        break;
                    }

                    i++;
                }
            }

            if (trimIndex != -1)
                return directoryName.Substring(0, trimIndex);

            return directoryName;
        }

        public static Int32 GetDirectoryVersion(String directoryName)
        {
            if (String.IsNullOrEmpty(directoryName))
                throw new ArgumentNullException("directoryName");

            Int32 version;
            Int32 trimedStringLength = SgiUtils.TrimDirectoryVersion(directoryName).Length;
            String versionString = String.Empty;

            if (trimedStringLength < directoryName.Length)
            {
                versionString = directoryName.Substring(trimedStringLength, directoryName.Length - trimedStringLength - 1);

                if (Char.ToLowerInvariant(versionString[0]) == 'v')
                    versionString = versionString.Remove(0, 1);
            }

            if (String.IsNullOrEmpty(versionString) || !Int32.TryParse(versionString, out version))
                version = -1;

            return version;
        }

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

                    if (isBackup && File.Exists(destFileName))
                    {
                        String newDestFileName = destFileName + ".original";

                        File.Delete(newDestFileName);
                        File.Move(destFileName, newDestFileName);
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

        #region Methods for working strings.

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
                        #region switch (input[i + 1])
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
                        #endregion switch (input[i + 1])
                    }
                }

                buffer[bufferIndex++] = ch;
            }

            return new String(buffer, 0, bufferIndex);
        }

        #endregion Methods for working strings.

        #region Methods for working registry.

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

    // The developer must take responsibility for assuring that the transition into unmanaged code is sufficiently protected by other means!
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
        private FileInfo valveDataFile;
        private String[] script;
        private BackgroundWorker worker;
        private DirectoryInfo installDir;
        private String gameLanguage;
        private List<VdfCstNode> cst; // concrete syntax tree
        private VdfAstNode ast; // abstract syntax tree

        public ValveDataFile(FileInfo file)
        {
            valveDataFile = file;
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

        public String GameLanguage
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
                String registryKey = null;
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
