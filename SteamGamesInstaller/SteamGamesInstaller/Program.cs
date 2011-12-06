// This file is subject of copyright notice which described in SgiLicense.txt file.
// Authors of this source code (Program.cs): Mesenion (ArahnaBytes). Other contributors should be mentioned in comments.

using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Text;
using System.Globalization;
using SteamGamesInstaller.Properties;

namespace SteamGamesInstaller
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(String[] args)
        {
            if (args != null && args.Length > 0 && Program.FixChangeLogVersion(args))
                return;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                Application.Run(new SgiForm());
            }
            catch (ObjectDisposedException) { }
        }

        /// <summary>
        /// If command line have /fixchangelogversion argument, then in SgiReadme.txt file in current directory
        /// replaces first version stump after "CHANGELOG" word.
        /// </summary>
        /// <param name="args">Arguments in command line.</param>
        /// <returns>Returns true if if command line have /fixchangelogversion argument.</returns>
        private static Boolean FixChangeLogVersion(String[] args)
        {
            String readmeFileName = Path.Combine(Environment.CurrentDirectory, "SgiReadme.txt");

            foreach (String argument in args)
            {
                if (String.Compare(argument, "/fixchangelogversion", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (File.Exists(readmeFileName))
                    {
                        String text = File.ReadAllText(readmeFileName, Encoding.Default);
                        Version version = Assembly.GetExecutingAssembly().GetName().Version;
                        String searchVersion = version.ToString(2) + ".*";

                        if (!String.IsNullOrEmpty(text))
                        {
                            Int32 index = text.IndexOf(searchVersion, text.LastIndexOf("CHANGELOG", StringComparison.Ordinal), StringComparison.Ordinal);

                            text = text.Remove(index, searchVersion.Length);
                            text = text.Insert(index, String.Format(CultureInfo.InvariantCulture, Resources.SgiVersionMessage, version.ToString()).Trim());
                            File.WriteAllText(readmeFileName, text, Encoding.Default);
                        }
                    }

                    return true;
                }
            }

            return false;
        }
    }
}
