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
    class CustomImplementation : IApplicationService
    {
        public void Start()
        {
            Console.WriteLine("CustomImplementation Started");
        }

        public void Stop()
        {
            Console.WriteLine("CustomImplementation Stopped");
        }
    }
}
