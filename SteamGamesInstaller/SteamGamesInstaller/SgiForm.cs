// This file is subject of copyright notice which described in SgiLicense.txt file.
// Initial contributors of this source code (SgiForm.cs): Mesenion (ArahnaBytes). Other contributors should be mentioned in comments.

#define JIT

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
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
    public partial class SgiForm : Form
    {
#if JIT
        private Object manager;
        private Assembly managerAssembly;
#else
        private SgiManager manager;
#endif
        Int64 gameSize;
        Int64 freeSpace;
        Boolean isInstalling;
        BackgroundWorker worker;

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public SgiForm()
        {
            isInstalling = false;

            InitializeComponent();

            this.Text += String.Format(CultureInfo.CurrentUICulture, Resources.SgiVersionMessage, Application.ProductVersion);

            installDirectoryTextBox.Text = GetSteamAppsDirectory();
            CreateManager();

            if (manager == null)
                this.Close();
        }

        private void steamGameComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            languageComboBox.Items.Clear();
#if JIT
            languageComboBox.Items.AddRange((String[])manager.GetType().GetMethod("GetInstallableLanguages").Invoke(manager,
                new Object[] { steamGameComboBox.Text }));
#else
            languageComboBox.Items.AddRange(manager.GetInstallableLanguages(steamGameComboBox.Text));
#endif
            if (languageComboBox.Items.Count > 0)
                languageComboBox.SelectedIndex = 0;
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            CreateManager();
        }

        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            installScriptCheckBox.Enabled = notUseSteamRadioButton.Checked;
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK)
                {
                    installDirectoryTextBox.Text = fbd.SelectedPath;
                    ShowGameSize();
                    SetInstallButtonState();
                }
            }
        }

        private void languageComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
#if JIT
                Object options = managerAssembly.CreateInstance("SteamGamesInstaller.InstallOptions", false, BindingFlags.CreateInstance, null,
                    new Object[] { steamGameComboBox.Text, useSteamRadioButton.Checked, installScriptCheckBox.Checked, fixCheckBox.Checked,
                    installDirectoryTextBox.Text, languageComboBox.Text }, CultureInfo.InvariantCulture, null);

            gameSize = (Int64)manager.GetType().GetMethod("GetGameSize").Invoke(manager, new Object[] { options });
#else
            InstallOptions options = new InstallOptions(steamGameComboBox.Text, useSteamRadioButton.Checked, installScriptCheckBox.Checked,
                    fixCheckBox.Checked, installDirectoryTextBox.Text, languageComboBox.Text);

            gameSize = manager.GetGameSize(options);
#endif
            ShowGameSize();
            SetInstallButtonState();
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

                worker.DoWork += new DoWorkEventHandler(InstallGame);
                worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
                worker.WorkerReportsProgress = true;
                worker.WorkerSupportsCancellation = true;

#if JIT
                Object options = managerAssembly.CreateInstance("SteamGamesInstaller.InstallOptions", false, BindingFlags.CreateInstance, null,
                    new Object[] { steamGameComboBox.Text, useSteamRadioButton.Checked, installScriptCheckBox.Checked, fixCheckBox.Checked,
                    installDirectoryTextBox.Text, languageComboBox.Text }, CultureInfo.InvariantCulture, null);
#else
                InstallOptions options = new InstallOptions(steamGameComboBox.Text, useSteamRadioButton.Checked, installScriptCheckBox.Checked,
                    fixCheckBox.Checked, installDirectoryTextBox.Text, languageComboBox.Text);
#endif

                worker.RunWorkerAsync(options);
            }
        }

        void InstallGame(object sender, DoWorkEventArgs e)
        {
#if JIT
            manager.GetType().GetMethod("InstallGame").Invoke(manager, new Object[] { e.Argument, worker });
#else
            manager.InstallGame((InstallOptions)e.Argument, worker);
#endif

            if (worker.CancellationPending)
                e.Cancel = true;
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            toolStripProgressBar.Value = e.ProgressPercentage;
            toolStripStatusLabel.Text = String.Format(CultureInfo.CurrentUICulture, Resources.InstallProgressMessage, e.ProgressPercentage);
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
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

        private void cancelButton_Click(object sender, EventArgs e)
        {
            if (!isInstalling)
                Application.Exit();
            else
                worker.CancelAsync();
        }

        private static String GetSteamAppsDirectory()
        {
            String steamPath = (String)Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam").GetValue("SteamPath");

            if (!String.IsNullOrEmpty(steamPath))
                return Path.Combine(Path.GetFullPath((String)steamPath), "SteamApps");

            return String.Empty;
        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        private void CreateManager()
        {
            SetControlsState(false);
#if JIT
            CompilerResults results = null;
            CompilerParameters parameters = new CompilerParameters(new String[] { "System.dll", "System.Core.dll" });

            parameters.GenerateInMemory = true;
            parameters.TempFiles = new TempFileCollection(Environment.CurrentDirectory, false);

            using (CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp"))
            {
                results = provider.CompileAssemblyFromSource(parameters, File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "SgiManager.cs")));
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

                return;
            }

            managerAssembly = results.CompiledAssembly;
            manager = managerAssembly.CreateInstance("SteamGamesInstaller.SgiManager");
#else
            manager = new SgiManager();
#endif

            ShowFindedGames();
            SetControlsState(true);
        }

        private void ShowFindedGames()
        {
#if JIT
            String[] findedGames = (String[])manager.GetType().GetMethod("GetInstallableGames").Invoke(manager, null);
#else
            String[] findedGames = manager.GetInstallableGames();
#endif

            if (findedGames != null)
            {
                steamGameComboBox.Items.Clear();
                steamGameComboBox.Items.AddRange(findedGames);
                if (steamGameComboBox.Items.Count > 0)
                    steamGameComboBox.SelectedIndex = 0;
            }
        }

        private void SetControlsState(Boolean isEnabled)
        {
            steamGameComboBox.Enabled = isEnabled;
            refreshButton.Enabled = isEnabled;
            optionsGroupBox.Enabled = isEnabled;

            if (!isEnabled)
                installButton.Enabled = false;
        }

        private void SetInstallButtonState()
        {
            if (!String.IsNullOrEmpty(installDirectoryTextBox.Text) && Directory.Exists(installDirectoryTextBox.Text) && gameSize != -1 && freeSpace > gameSize && !isInstalling)
                installButton.Enabled = true;
            else
                installButton.Enabled = false;
        }

        private void ShowGameSize()
        {
            if (!String.IsNullOrEmpty(installDirectoryTextBox.Text) && Directory.Exists(installDirectoryTextBox.Text) && gameSize != -1)
            {
                DriveInfo drive = GetDrive(installDirectoryTextBox.Text);

                gameSizeInMbLabel.ForeColor = SystemColors.ControlText;
                gameSizeInMbLabel.Text = String.Format(CultureInfo.CurrentUICulture, Resources.GameSizeInMbMessage, gameSize / 1024 / 1024);
                freeSpace = drive.AvailableFreeSpace;

                if (freeSpace <= gameSize)
                {
                    gameSizeInMbLabel.ForeColor = Color.Red;
                    gameSizeInMbLabel.Text += String.Format(CultureInfo.CurrentUICulture, Resources.FreeSpaceMessage, drive.Name, freeSpace / 1024 / 1024);
                }
            }
            else
                gameSizeInMbLabel.Text = String.Empty;
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
