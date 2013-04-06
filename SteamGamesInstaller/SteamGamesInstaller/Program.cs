﻿// This file is subject of copyright notice which described in SgiLicense.txt file.
// Authors of this source code (Program.cs): Mesenion (ArahnaBytes). Other contributors should be mentioned in comments.

using SteamGamesInstaller.Properties;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

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
        /// replaces first version stump after "CHANGELOG:" phrase with assembly version, version name and current date.
        /// </summary>
        /// <param name="args">Arguments in command line.</param>
        /// <returns>Returns true if command line have /fixchangelogversion argument.</returns>
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
                            Int32 index = text.IndexOf(searchVersion, text.LastIndexOf("CHANGELOG:", StringComparison.Ordinal), StringComparison.Ordinal);
                            String changeLogVersionString = String.Format(CultureInfo.InvariantCulture, Resources.SgiVersionMessage, version.ToString()).Trim();

                            changeLogVersionString = String.Format(CultureInfo.InvariantCulture, Resources.SgiVersionMessage, version.ToString());
                            changeLogVersionString += " " + DateTime.Now.ToString(CultureInfo.InvariantCulture.DateTimeFormat.LongDatePattern,
                                CultureInfo.InvariantCulture.DateTimeFormat) + ":";

                            text = text.Remove(index, searchVersion.Length);
                            text = text.Insert(index, changeLogVersionString.Trim());
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
