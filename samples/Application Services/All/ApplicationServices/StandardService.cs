using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Griffin.ApplicationServices;
using Griffin.Container;

namespace ServicesDemo.ApplicationServices
{
    [ContainerService]
    class StandardService : ApplicationServiceThread
    {
        protected override void Run(WaitHandle shutdownHandle)
        {
            Console.WriteLine("StandardService, Service started. Waiting for exit event");

            while (!shutdownHandle.WaitOne(100))
            {
                //runs something every 100ms
            }
            

            Console.WriteLine("StandardService, Exiting");
        }
    }
}
