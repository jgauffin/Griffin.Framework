using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Griffin.ApplicationServices;
using Griffin.Container;

namespace ServicesDemo.BackgroundJobs
{
    [ContainerService]
    class SampleJob : IBackgroundJob
    {
        public void Execute()
        {
            Console.WriteLine("SampleJob: Executing... (being scoped)");
        }
    }
}
