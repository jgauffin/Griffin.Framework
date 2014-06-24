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
    class FailingStandardService : ApplicationServiceThread
    {
        protected override void Run(WaitHandle shutdownHandle)
        {
            Console.WriteLine("FailingStandardService: Failing.");
            throw new InvalidOperationException();
        }
    }
}
