// This file is subject of copyright notice which described in SgiLicense.txt file.
// Initial contributors of this source code (ErrorForm.cs): Mesenion (ArahnaBytes). Other contributors should be mentioned in comments.

using System;
using System.Windows.Forms;

namespace SteamGamesInstaller
{
    public partial class ErrorForm : Form
    {
        public ErrorForm()
        {
            InitializeComponent();
        }

        public ErrorForm(String message)
            : this()
        {
            this.Message = message;
        }

        private void copyButton_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(this.Message, TextDataFormat.UnicodeText);
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public String Message
        {
            get { return messageRichTextBox.Text; }
            set { messageRichTextBox.Text = value; }
        }
    }
}
