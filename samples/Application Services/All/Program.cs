using System;
using System.Reflection;
using Autofac;
using Griffin.ApplicationServices;
using Griffin.Core.Autofac;
using Griffin.Logging;
using ServicesDemo.Logs;
using IContainer = Griffin.Container.IContainer;

namespace ServicesDemo
{
    internal class Program
    {
        private BackgroundJobManager _jobManager;
        private ApplicationServiceManager _serviceManager;

        private static void Main(string[] args)
        {
            LogManager.Provider = new LogManagerProvider();

            new Program().RunDemo();
        }

        private void RunDemo()
        {
            
            var adapter = CreateContainer();
            _serviceManager = new ApplicationServiceManager(adapter);
            _serviceManager.ServiceFailed += OnApplicationFailure;
            _serviceManager.Start();

            _jobManager = new BackgroundJobManager(adapter);
            _jobManager.JobFailed += OnJobFailed;
            _jobManager.Start();


            //Press enter to shut down
            Console.ReadLine();

            _jobManager.Stop();
            _serviceManager.Stop();

            Console.WriteLine("Done, press enter to exit");
            Console.ReadLine();
        }

        private void OnJobFailed(object sender, BackgroundJobFailedEventArgs e)
        {
            Console.WriteLine(e.Job + " failed: " + e.Exception);
        }

        private void OnApplicationFailure(object sender, ApplicationServiceFailedEventArgs e)
        {
            Console.WriteLine(e.ApplicationService + " failed: " + e.Exception);
        }

        private IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterServices(Assembly.GetExecutingAssembly());

            var container = builder.Build();
            return new AutofacAdapter(container);
        }
    }
}