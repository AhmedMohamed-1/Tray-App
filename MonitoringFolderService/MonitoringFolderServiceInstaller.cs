using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace MonitoringFolderService
{
    [RunInstaller(true)]
    public partial class MonitoringFolderServiceInstaller : Installer
    {
        private ServiceProcessInstaller serviceProcessInstaller;
        private ServiceInstaller serviceInstaller;
        public MonitoringFolderServiceInstaller()
        {
            InitializeComponent();

            serviceProcessInstaller = new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalSystem
            };

            serviceInstaller = new ServiceInstaller
            {
                ServiceName = "MonitoringFolderService",
                DisplayName = "MonitoringFolderService",
                Description = "Monitor any new files in the folder",
                StartType = ServiceStartMode.Automatic,
            };

            Installers.Add(serviceProcessInstaller);
            Installers.Add(serviceInstaller);
        }
    }
}
