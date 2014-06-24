using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Griffin.ApplicationServices;
using Griffin.Container;

namespace ServicesDemo.ApplicationServices
{
    [ContainerService]
    class CustomGuardedService  : IGuardedService
    {
        private Random _random = new Random();

        public bool IsRunning
        {
            get
            {
                var isRunning = _random.Next(0, 2) == 0;
                if (!isRunning)
                    Console.WriteLine("CustomGuardedService is reported to not be running");

                return isRunning;
            }
        }

        public event EventHandler<ApplicationServiceFailedEventArgs> Failed;

        public void Start()
        {
            Console.WriteLine("CustomGuardedService: Starting");
        }

        public void Stop()
        {
            Console.WriteLine("CustomGuardedService: Stopping");
        }
    }
}
