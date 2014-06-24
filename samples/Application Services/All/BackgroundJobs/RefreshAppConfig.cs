using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Griffin.ApplicationServices;
using Griffin.Container;

namespace ServicesDemo.BackgroundJobs
{
    [ContainerService]
    class RefreshAppConfig : IBackgroundJob
    {
        public void Execute()
        {
            //required so that you can start/stop the services using the app.config during runtmie
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
