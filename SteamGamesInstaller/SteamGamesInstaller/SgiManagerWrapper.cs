using System;
using System.ComponentModel;

namespace SteamGamesInstaller
{
#if JIT
    internal class SgiManagerWrapper : ISgiManager
    {
        Object manager;

        public SgiManagerWrapper(Object managerObject)
        {
            manager = managerObject;
        }

        public String[] GetInstallableApplications()
        {
            return (String[])manager.GetType().GetMethod("GetInstallableApplications").Invoke(manager, null);
        }

        public String[] GetInstallableLanguages(String appName)
        {
            return (String[])manager.GetType().GetMethod("GetInstallableLanguages").Invoke(manager, new Object[] { appName });
        }

        public Int64 GetFilesSize(Object installOptions)
        {
            return (Int64)manager.GetType().GetMethod("GetFilesSize").Invoke(manager, new Object[] { installOptions });
        }

        public void InstallApplication(Object installOptions, BackgroundWorker worker)
        {
            manager.GetType().GetMethod("InstallApplication").Invoke(manager, new Object[] { installOptions, worker });
        }
    }
#endif
}
