using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Griffin.ApplicationServices;
using Griffin.ApplicationServices.AppDomains;

namespace AppDomainServiceHostDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = new ApplicationManagerSettings
            {
                AppDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Versions"),
                ApplicationName = "DemoApp",
                CompanyName = "Griffin",
                PickupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pickup")
            };
            var appManager = new ApplicationManager<MyApplicationInit>(settings);
            appManager.Start();


            Console.ReadLine();
            appManager.Stop();
        }
    }

    public class MyApplicationInit : IApplicationInitialize
    {
        public void Start(string[] argv)
        {
            Console.WriteLine("Started");
        }

        public void Stop()
        {
            Console.WriteLine("Stopped");
        }

        public bool SupportsPause { get { return false; }}
        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Resume()
        {
            throw new NotImplementedException();
        }

        public void OnUpdateAvailable(IUpdateInformation information)
        {
            
        }

        public bool RequestRestartPermission()
        {
            return false;
        }
    }
}
