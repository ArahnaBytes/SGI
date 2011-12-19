namespace SteamGamesInstaller
{
    partial class SgiForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SgiForm));
            this.steamApplicationLabel = new System.Windows.Forms.Label();
            this.steamApplicationsComboBox = new System.Windows.Forms.ComboBox();
            this.useSteamRadioButton = new System.Windows.Forms.RadioButton();
            this.notUseSteamRadioButton = new System.Windows.Forms.RadioButton();
            this.executeInstallScriptCheckBox = new System.Windows.Forms.CheckBox();
            this.installFixesCheckBox = new System.Windows.Forms.CheckBox();
            this.optionsGroupBox = new System.Windows.Forms.GroupBox();
            this.installApplicationCheckBox = new System.Windows.Forms.CheckBox();
            this.filesSizeInMbLabel = new System.Windows.Forms.Label();
            this.filesSizeLabel = new System.Windows.Forms.Label();
            this.applicationLanguageLabel = new System.Windows.Forms.Label();
            this.applicationLanguageComboBox = new System.Windows.Forms.ComboBox();
            this.browseInstallDirectoryButton = new System.Windows.Forms.Button();
            this.installDirectoryTextBox = new System.Windows.Forms.TextBox();
            this.installDirectoryLabel = new System.Windows.Forms.Label();
            this.installApplicationButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.refreshApplicationsButton = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.optionsGroupBox.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // steamApplicationLabel
            // 
            resources.ApplyResources(this.steamApplicationLabel, "steamApplicationLabel");
            this.steamApplicationLabel.Name = "steamApplicationLabel";
            // 
            // steamApplicationsComboBox
            // 
            resources.ApplyResources(this.steamApplicationsComboBox, "steamApplicationsComboBox");
            this.steamApplicationsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.steamApplicationsComboBox.FormattingEnabled = true;
            this.steamApplicationsComboBox.Name = "steamApplicationsComboBox";
            this.steamApplicationsComboBox.Sorted = true;
            this.steamApplicationsComboBox.SelectedIndexChanged += new System.EventHandler(this.steamApplicationsComboBox_SelectedIndexChanged);
            // 
            // useSteamRadioButton
            // 
            resources.ApplyResources(this.useSteamRadioButton, "useSteamRadioButton");
            this.useSteamRadioButton.Checked = true;
            this.useSteamRadioButton.Name = "useSteamRadioButton";
            this.useSteamRadioButton.TabStop = true;
            this.useSteamRadioButton.UseVisualStyleBackColor = true;
            this.useSteamRadioButton.CheckedChanged += new System.EventHandler(this.RadioButton_CheckedChanged);
            // 
            // notUseSteamRadioButton
            // 
            resources.ApplyResources(this.notUseSteamRadioButton, "notUseSteamRadioButton");
            this.notUseSteamRadioButton.Name = "notUseSteamRadioButton";
            this.notUseSteamRadioButton.UseVisualStyleBackColor = true;
            // 
            // executeInstallScriptCheckBox
            // 
            resources.ApplyResources(this.executeInstallScriptCheckBox, "executeInstallScriptCheckBox");
            this.executeInstallScriptCheckBox.Name = "executeInstallScriptCheckBox";
            this.executeInstallScriptCheckBox.UseVisualStyleBackColor = true;
            // 
            // installFixesCheckBox
            // 
            resources.ApplyResources(this.installFixesCheckBox, "installFixesCheckBox");
            this.installFixesCheckBox.Name = "installFixesCheckBox";
            this.installFixesCheckBox.UseVisualStyleBackColor = true;
            this.installFixesCheckBox.CheckedChanged += new System.EventHandler(this.installFixesCheckBox_CheckedChanged);
            // 
            // optionsGroupBox
            // 
            resources.ApplyResources(this.optionsGroupBox, "optionsGroupBox");
            this.optionsGroupBox.Controls.Add(this.installApplicationCheckBox);
            this.optionsGroupBox.Controls.Add(this.filesSizeInMbLabel);
            this.optionsGroupBox.Controls.Add(this.filesSizeLabel);
            this.optionsGroupBox.Controls.Add(this.applicationLanguageLabel);
            this.optionsGroupBox.Controls.Add(this.applicationLanguageComboBox);
            this.optionsGroupBox.Controls.Add(this.browseInstallDirectoryButton);
            this.optionsGroupBox.Controls.Add(this.installDirectoryTextBox);
            this.optionsGroupBox.Controls.Add(this.installDirectoryLabel);
            this.optionsGroupBox.Controls.Add(this.executeInstallScriptCheckBox);
            this.optionsGroupBox.Controls.Add(this.installFixesCheckBox);
            this.optionsGroupBox.Controls.Add(this.useSteamRadioButton);
            this.optionsGroupBox.Controls.Add(this.notUseSteamRadioButton);
            this.optionsGroupBox.Name = "optionsGroupBox";
            this.optionsGroupBox.TabStop = false;
            // 
            // installApplicationCheckBox
            // 
            resources.ApplyResources(this.installApplicationCheckBox, "installApplicationCheckBox");
            this.installApplicationCheckBox.Checked = true;
            this.installApplicationCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.installApplicationCheckBox.Name = "installApplicationCheckBox";
            this.installApplicationCheckBox.UseVisualStyleBackColor = true;
            this.installApplicationCheckBox.CheckedChanged += new System.EventHandler(this.installApplicationCheckBox_CheckedChanged);
            // 
            // filesSizeInMbLabel
            // 
            resources.ApplyResources(this.filesSizeInMbLabel, "filesSizeInMbLabel");
            this.filesSizeInMbLabel.Name = "filesSizeInMbLabel";
            // 
            // filesSizeLabel
            // 
            resources.ApplyResources(this.filesSizeLabel, "filesSizeLabel");
            this.filesSizeLabel.Name = "filesSizeLabel";
            // 
            // applicationLanguageLabel
            // 
            resources.ApplyResources(this.applicationLanguageLabel, "applicationLanguageLabel");
            this.applicationLanguageLabel.Name = "applicationLanguageLabel";
            // 
            // applicationLanguageComboBox
            // 
            resources.ApplyResources(this.applicationLanguageComboBox, "applicationLanguageComboBox");
            this.applicationLanguageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.applicationLanguageComboBox.FormattingEnabled = true;
            this.applicationLanguageComboBox.Name = "applicationLanguageComboBox";
            this.applicationLanguageComboBox.Sorted = true;
            this.applicationLanguageComboBox.SelectedIndexChanged += new System.EventHandler(this.languageComboBox_SelectedIndexChanged);
            // 
            // browseInstallDirectoryButton
            // 
            resources.ApplyResources(this.browseInstallDirectoryButton, "browseInstallDirectoryButton");
            this.browseInstallDirectoryButton.Name = "browseInstallDirectoryButton";
            this.browseInstallDirectoryButton.UseVisualStyleBackColor = true;
            this.browseInstallDirectoryButton.Click += new System.EventHandler(this.browseInstallDirectoryButton_Click);
            // 
            // installDirectoryTextBox
            // 
            resources.ApplyResources(this.installDirectoryTextBox, "installDirectoryTextBox");
            this.installDirectoryTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.installDirectoryTextBox.Name = "installDirectoryTextBox";
            this.installDirectoryTextBox.ReadOnly = true;
            // 
            // installDirectoryLabel
            // 
            resources.ApplyResources(this.installDirectoryLabel, "installDirectoryLabel");
            this.installDirectoryLabel.Name = "installDirectoryLabel";
            // 
            // installApplicationButton
            // 
            resources.ApplyResources(this.installApplicationButton, "installApplicationButton");
            this.installApplicationButton.Name = "installApplicationButton";
            this.installApplicationButton.UseVisualStyleBackColor = true;
            this.installApplicationButton.Click += new System.EventHandler(this.installButton_Click);
            // 
            // cancelButton
            // 
            resources.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // refreshApplicationsButton
            // 
            resources.ApplyResources(this.refreshApplicationsButton, "refreshApplicationsButton");
            this.refreshApplicationsButton.Name = "refreshApplicationsButton";
            this.refreshApplicationsButton.UseVisualStyleBackColor = true;
            this.refreshApplicationsButton.Click += new System.EventHandler(this.refreshButton_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar,
            this.toolStripStatusLabel});
            resources.ApplyResources(this.statusStrip, "statusStrip");
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.SizingGrip = false;
            // 
            // toolStripProgressBar
            // 
            this.toolStripProgressBar.Name = "toolStripProgressBar";
            resources.ApplyResources(this.toolStripProgressBar, "toolStripProgressBar");
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            resources.ApplyResources(this.toolStripStatusLabel, "toolStripStatusLabel");
            // 
            // SgiForm
            // 
            this.AcceptButton = this.installApplicationButton;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.refreshApplicationsButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.installApplicationButton);
            this.Controls.Add(this.optionsGroupBox);
            this.Controls.Add(this.steamApplicationsComboBox);
            this.Controls.Add(this.steamApplicationLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "SgiForm";
            this.optionsGroupBox.ResumeLayout(false);
            this.optionsGroupBox.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label steamApplicationLabel;
        private System.Windows.Forms.ComboBox steamApplicationsComboBox;
        private System.Windows.Forms.RadioButton useSteamRadioButton;
        private System.Windows.Forms.RadioButton notUseSteamRadioButton;
        private System.Windows.Forms.CheckBox executeInstallScriptCheckBox;
        private System.Windows.Forms.CheckBox installFixesCheckBox;
        private System.Windows.Forms.GroupBox optionsGroupBox;
        private System.Windows.Forms.Button browseInstallDirectoryButton;
        private System.Windows.Forms.TextBox installDirectoryTextBox;
        private System.Windows.Forms.Label installDirectoryLabel;
        private System.Windows.Forms.Label applicationLanguageLabel;
        private System.Windows.Forms.ComboBox applicationLanguageComboBox;
        private System.Windows.Forms.Button installApplicationButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button refreshApplicationsButton;
        private System.Windows.Forms.Label filesSizeInMbLabel;
        private System.Windows.Forms.Label filesSizeLabel;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.CheckBox installApplicationCheckBox;
    }
}

