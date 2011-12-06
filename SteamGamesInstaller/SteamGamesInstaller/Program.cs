// This file is subject of copyright notice which described in SgiLicense.txt file.
// Authors of this source code (Program.cs): Mesenion (ArahnaBytes). Other contributors should be mentioned in comments.

using System;
using System.Windows.Forms;

namespace SteamGamesInstaller
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                Application.Run(new SgiForm());
            }
            catch (ObjectDisposedException) { }
        }
    }
}
