// This file is subject of copyright notice which described in SgiLicense.txt file.
// Initial contributors of this source code (SgiForm.cs): Mesenion (ArahnaBytes). Other contributors should be mentioned in comments.

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using System.Windows.Forms;
using Microsoft.Win32;
using SteamGamesInstaller.Properties;

namespace SteamGamesInstaller
{
#if JIT
    public interface ISgiManager
    {
        String[] GetInstallableApplications();
        String[] GetInstallableLanguages(String appName);
        Int64 GetFilesSize(Object installOptions);
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        void InstallApplication(Object installOptions, BackgroundWorker worker);
    }
#endif

    public partial class SgiForm : Form
    {
#if JIT
        private Assembly managerAssembly;
#endif
        DirectoryInfo currentProcessDirectory;
        private ISgiManager manager;
        private Int64 appSize;
        private Int64 freeSpace;
        private Boolean isInstalling;
        private BackgroundWorker worker;

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public SgiForm()
        {
            currentProcessDirectory = new DirectoryInfo(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName));
            isInstalling = false;

            InitializeComponent();

            this.Text += String.Format(CultureInfo.CurrentUICulture, Resources.SgiVersionMessage, Application.ProductVersion);

#if JIT
            managerAssembly = CompileManagerAssembly();

            if (managerAssembly == null)
            {
                this.Close();
                return;
            }
#endif
            installDirectoryTextBox.Text = GetSteamAppsDirectory();
            CreateManager();
        }

        private void steamApplicationsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            applicationLanguageComboBox.Items.Clear();
            applicationLanguageComboBox.Items.AddRange(manager.GetInstallableLanguages(steamApplicationsComboBox.Text));

            if (applicationLanguageComboBox.Items.Count > 0)
                applicationLanguageComboBox.SelectedIndex = 0;
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
#if JIT
            managerAssembly = CompileManagerAssembly();
#endif
            CreateManager();
        }

        private void installApplicationCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ShowApplicationSize(true);
        }

        private void installFixesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ShowApplicationSize(true);
        }

        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            executeInstallScriptCheckBox.Enabled = notUseSteamRadioButton.Checked;
            executeInstallScriptCheckBox.Checked = executeInstallScriptCheckBox.Enabled;
        }

        private void browseInstallDirectoryButton_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK)
                {
                    installDirectoryTextBox.Text = fbd.SelectedPath;
                    ShowApplicationSize(false);
                }
            }
        }

        private void languageComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowApplicationSize(true);
        }

        private void installButton_Click(object sender, EventArgs e)
        {
            if (!isInstalling)
            {
                isInstalling = true;
                SetInstallButtonState();
                SetControlsState(false);

                toolStripProgressBar.Value = 0;
                toolStripStatusLabel.Text = String.Format(CultureInfo.InvariantCulture, Resources.InstallProgressMessage, 0); // Sets text to "0 %"

                worker = new BackgroundWorker();

                worker.DoWork += new DoWorkEventHandler(InstallApplication);
                worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
                worker.WorkerReportsProgress = true;
                worker.WorkerSupportsCancellation = true;

#if JIT
                Object options = managerAssembly.CreateInstance("SteamGamesInstaller.InstallOptions", false, BindingFlags.CreateInstance, null,
                    new Object[] { steamApplicationsComboBox.Text, installDirectoryTextBox.Text, applicationLanguageComboBox.Text,
                    executeInstallScriptCheckBox.Checked, installApplicationCheckBox.Checked, installFixesCheckBox.Checked }, CultureInfo.InvariantCulture, null);
#else
                Object options = new InstallOptions(steamApplicationsComboBox.Text, installDirectoryTextBox.Text, applicationLanguageComboBox.Text,
                    executeInstallScriptCheckBox.Checked, installApplicationCheckBox.Checked, installFixesCheckBox.Checked);
#endif

                worker.RunWorkerAsync(options);
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            if (!isInstalling)
                Application.Exit();
            else
                worker.CancelAsync();
        }

        private void InstallApplication(object sender, DoWorkEventArgs e)
        {
            manager.InstallApplication(e.Argument, worker);

            if (worker.CancellationPending)
                e.Cancel = true;
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            toolStripProgressBar.Value = e.ProgressPercentage;
            toolStripStatusLabel.Text = String.Format(CultureInfo.CurrentUICulture, Resources.InstallProgressMessage, e.ProgressPercentage);
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                toolStripProgressBar.Value = 0;
                toolStripStatusLabel.Text = Resources.CanceledMessage;
            }
            else if (e.Error != null)
            {
                String message = e.Error.Message + Environment.NewLine;

                if (e.Error.InnerException != null)
                    message += e.Error.InnerException.Message + Environment.NewLine;
                message += Environment.NewLine + Resources.StackTraceMessage + Environment.NewLine + e.Error.StackTrace + Environment.NewLine;
                if (e.Error.InnerException != null)
                    message += Environment.NewLine + Resources.BackgroundThreadStackTraceMessage + Environment.NewLine + e.Error.InnerException.StackTrace + Environment.NewLine;

                using (ErrorForm errorForm = new ErrorForm(message))
                {
                    errorForm.ShowDialog();
                }

                toolStripStatusLabel.Text = Resources.ErrorMessage;
            }
            else
                toolStripStatusLabel.Text = Resources.DoneMessage;

            isInstalling = false;
            SetInstallButtonState();
            SetControlsState(true);
            worker.Dispose();
        }

#if JIT
        private Assembly CompileManagerAssembly()
        {
            CompilerResults results = null;
            CompilerParameters parameters = new CompilerParameters(new String[] { "System.dll", "System.Core.dll", "System.Windows.Forms.dll",
                Assembly.GetExecutingAssembly().Location});

            parameters.GenerateInMemory = true;
            parameters.TempFiles = new TempFileCollection(currentProcessDirectory.FullName, false);

            using (CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp"))
            {
                results = provider.CompileAssemblyFromSource(parameters, File.ReadAllText(Path.Combine(currentProcessDirectory.FullName, "SgiManager.cs")));
            }

            if (results != null && results.Errors.Count > 0)
            {
                String message = String.Empty;

                foreach (String str in results.Output)
                    message += str + Environment.NewLine;

                using (ErrorForm errorForm = new ErrorForm(message))
                {
                    errorForm.ShowInTaskbar = true;
                    errorForm.ShowDialog();
                }

                return null;
            }

            return results.CompiledAssembly;
        }
#endif

        private String GetSteamAppsDirectory()
        {
            String steamPath = (String)Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam").GetValue("SteamPath");

            if (!String.IsNullOrEmpty(steamPath))
                return Path.Combine(Path.GetFullPath((String)steamPath), "SteamApps");
            else
            {
#if JIT
                steamPath = (String)managerAssembly.GetType("SteamGamesInstaller.SgiUtils").
                    GetMethod("GetFolderPath", new Type[] { managerAssembly.GetType("SteamGamesInstaller.SgiSpecialFolder") }).
                    Invoke(null, new Object[] { 0x2a }); // SgiSpecialFolder.ProgramFilesX86 = 0x2a
#else
                steamPath = SgiUtils.GetFolderPath(SgiSpecialFolder.ProgramFilesX86);
#endif

                if (String.IsNullOrEmpty(steamPath)) // Dunno what returns with SgiSpecialFolder.ProgramFilesX86 on x86 OS, so check here
                    steamPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                if (String.IsNullOrEmpty(steamPath)) // Oh, nothing work :(
                    steamPath = currentProcessDirectory.Root.FullName;

                steamPath = Path.Combine(steamPath, @"Steam\SteamApps");

                return steamPath;
            }
        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        private void CreateManager()
        {
            SetControlsState(false);
#if JIT
            manager = new SgiManagerWrapper(managerAssembly.CreateInstance("SteamGamesInstaller.SgiManager"));
#else
            manager = new SgiManager();
#endif

            if (manager == null)
                this.Close();
            else
            {
                ShowFindedApps();
                SetControlsState(true);
            }
        }

        private void ShowFindedApps()
        {
            String[] findedApps = manager.GetInstallableApplications();

            if (findedApps != null)
            {
                steamApplicationsComboBox.Items.Clear();
                steamApplicationsComboBox.Items.AddRange(findedApps);
                if (steamApplicationsComboBox.Items.Count > 0)
                    steamApplicationsComboBox.SelectedIndex = 0;
            }
        }

        private void SetControlsState(Boolean isEnabled)
        {
            steamApplicationsComboBox.Enabled = isEnabled;
            refreshApplicationsButton.Enabled = isEnabled;
            optionsGroupBox.Enabled = isEnabled;

            if (!isEnabled)
                installApplicationButton.Enabled = false;
        }

        private void SetInstallButtonState()
        {
            if (!String.IsNullOrEmpty(installDirectoryTextBox.Text) && Directory.Exists(installDirectoryTextBox.Text) && appSize != -1 && freeSpace > appSize && !isInstalling)
                installApplicationButton.Enabled = true;
            else
                installApplicationButton.Enabled = false;
        }

        private void ShowApplicationSize(Boolean isRecalculate)
        {
            if (isRecalculate)
            {
#if JIT
                Object options = managerAssembly.CreateInstance("SteamGamesInstaller.InstallOptions", false, BindingFlags.CreateInstance, null,
                    new Object[] { steamApplicationsComboBox.Text, installDirectoryTextBox.Text, applicationLanguageComboBox.Text,
                    executeInstallScriptCheckBox.Checked, installApplicationCheckBox.Checked, installFixesCheckBox.Checked }, CultureInfo.InvariantCulture, null);
#else
                Object options = new InstallOptions(steamApplicationsComboBox.Text, installDirectoryTextBox.Text, applicationLanguageComboBox.Text,
                    executeInstallScriptCheckBox.Checked, installApplicationCheckBox.Checked, installFixesCheckBox.Checked);
#endif

                appSize = manager.GetFilesSize(options);
            }

            if (!String.IsNullOrEmpty(installDirectoryTextBox.Text) && Directory.Exists(installDirectoryTextBox.Text) && appSize != -1)
            {
                DriveInfo drive = GetDrive(installDirectoryTextBox.Text);

                filesSizeInMbLabel.ForeColor = SystemColors.ControlText;
                filesSizeInMbLabel.Text = String.Format(CultureInfo.CurrentUICulture, Resources.AppSizeInMbMessage, appSize / 1024 / 1024);
                freeSpace = drive.AvailableFreeSpace;

                if (freeSpace <= appSize)
                {
                    filesSizeInMbLabel.ForeColor = Color.Red;
                    filesSizeInMbLabel.Text += String.Format(CultureInfo.CurrentUICulture, Resources.FreeSpaceMessage, drive.Name, freeSpace / 1024 / 1024);
                }
            }
            else
                filesSizeInMbLabel.Text = String.Empty;

            SetInstallButtonState();
        }

        private static DriveInfo GetDrive(String path)
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (String.Compare(drive.Name, Path.GetPathRoot(path), StringComparison.OrdinalIgnoreCase) == 0)
                    return drive;
            }

            return null;
        }
    }
}
