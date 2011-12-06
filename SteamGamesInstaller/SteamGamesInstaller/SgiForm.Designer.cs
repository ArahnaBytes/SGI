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
            this.steamGameLabel = new System.Windows.Forms.Label();
            this.steamGameComboBox = new System.Windows.Forms.ComboBox();
            this.useSteamRadioButton = new System.Windows.Forms.RadioButton();
            this.notUseSteamRadioButton = new System.Windows.Forms.RadioButton();
            this.installScriptCheckBox = new System.Windows.Forms.CheckBox();
            this.fixCheckBox = new System.Windows.Forms.CheckBox();
            this.optionsGroupBox = new System.Windows.Forms.GroupBox();
            this.gameSizeInMbLabel = new System.Windows.Forms.Label();
            this.gameSizeLabel = new System.Windows.Forms.Label();
            this.languageLabel = new System.Windows.Forms.Label();
            this.languageComboBox = new System.Windows.Forms.ComboBox();
            this.browseButton = new System.Windows.Forms.Button();
            this.installDirectoryTextBox = new System.Windows.Forms.TextBox();
            this.installDirectoryLabel = new System.Windows.Forms.Label();
            this.installButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.refreshButton = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.optionsGroupBox.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // steamGameLabel
            // 
            resources.ApplyResources(this.steamGameLabel, "steamGameLabel");
            this.steamGameLabel.Name = "steamGameLabel";
            // 
            // steamGameComboBox
            // 
            resources.ApplyResources(this.steamGameComboBox, "steamGameComboBox");
            this.steamGameComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.steamGameComboBox.FormattingEnabled = true;
            this.steamGameComboBox.Name = "steamGameComboBox";
            this.steamGameComboBox.Sorted = true;
            this.steamGameComboBox.SelectedIndexChanged += new System.EventHandler(this.steamGameComboBox_SelectedIndexChanged);
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
            // installScriptCheckBox
            // 
            resources.ApplyResources(this.installScriptCheckBox, "installScriptCheckBox");
            this.installScriptCheckBox.Checked = true;
            this.installScriptCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.installScriptCheckBox.Name = "installScriptCheckBox";
            this.installScriptCheckBox.UseVisualStyleBackColor = true;
            // 
            // fixCheckBox
            // 
            resources.ApplyResources(this.fixCheckBox, "fixCheckBox");
            this.fixCheckBox.Name = "fixCheckBox";
            this.fixCheckBox.UseVisualStyleBackColor = true;
            // 
            // optionsGroupBox
            // 
            resources.ApplyResources(this.optionsGroupBox, "optionsGroupBox");
            this.optionsGroupBox.Controls.Add(this.gameSizeInMbLabel);
            this.optionsGroupBox.Controls.Add(this.gameSizeLabel);
            this.optionsGroupBox.Controls.Add(this.languageLabel);
            this.optionsGroupBox.Controls.Add(this.languageComboBox);
            this.optionsGroupBox.Controls.Add(this.browseButton);
            this.optionsGroupBox.Controls.Add(this.installDirectoryTextBox);
            this.optionsGroupBox.Controls.Add(this.installDirectoryLabel);
            this.optionsGroupBox.Controls.Add(this.installScriptCheckBox);
            this.optionsGroupBox.Controls.Add(this.fixCheckBox);
            this.optionsGroupBox.Controls.Add(this.useSteamRadioButton);
            this.optionsGroupBox.Controls.Add(this.notUseSteamRadioButton);
            this.optionsGroupBox.Name = "optionsGroupBox";
            this.optionsGroupBox.TabStop = false;
            // 
            // gameSizeInMbLabel
            // 
            resources.ApplyResources(this.gameSizeInMbLabel, "gameSizeInMbLabel");
            this.gameSizeInMbLabel.Name = "gameSizeInMbLabel";
            // 
            // gameSizeLabel
            // 
            resources.ApplyResources(this.gameSizeLabel, "gameSizeLabel");
            this.gameSizeLabel.Name = "gameSizeLabel";
            // 
            // languageLabel
            // 
            resources.ApplyResources(this.languageLabel, "languageLabel");
            this.languageLabel.Name = "languageLabel";
            // 
            // languageComboBox
            // 
            resources.ApplyResources(this.languageComboBox, "languageComboBox");
            this.languageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.languageComboBox.FormattingEnabled = true;
            this.languageComboBox.Name = "languageComboBox";
            this.languageComboBox.Sorted = true;
            this.languageComboBox.SelectedIndexChanged += new System.EventHandler(this.languageComboBox_SelectedIndexChanged);
            // 
            // browseButton
            // 
            resources.ApplyResources(this.browseButton, "browseButton");
            this.browseButton.Name = "browseButton";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
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
            // installButton
            // 
            resources.ApplyResources(this.installButton, "installButton");
            this.installButton.Name = "installButton";
            this.installButton.UseVisualStyleBackColor = true;
            this.installButton.Click += new System.EventHandler(this.installButton_Click);
            // 
            // cancelButton
            // 
            resources.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // refreshButton
            // 
            resources.ApplyResources(this.refreshButton, "refreshButton");
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.UseVisualStyleBackColor = true;
            this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
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
            this.AcceptButton = this.installButton;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.refreshButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.installButton);
            this.Controls.Add(this.optionsGroupBox);
            this.Controls.Add(this.steamGameComboBox);
            this.Controls.Add(this.steamGameLabel);
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

        private System.Windows.Forms.Label steamGameLabel;
        private System.Windows.Forms.ComboBox steamGameComboBox;
        private System.Windows.Forms.RadioButton useSteamRadioButton;
        private System.Windows.Forms.RadioButton notUseSteamRadioButton;
        private System.Windows.Forms.CheckBox installScriptCheckBox;
        private System.Windows.Forms.CheckBox fixCheckBox;
        private System.Windows.Forms.GroupBox optionsGroupBox;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.TextBox installDirectoryTextBox;
        private System.Windows.Forms.Label installDirectoryLabel;
        private System.Windows.Forms.Label languageLabel;
        private System.Windows.Forms.ComboBox languageComboBox;
        private System.Windows.Forms.Button installButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.Label gameSizeInMbLabel;
        private System.Windows.Forms.Label gameSizeLabel;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
    }
}

