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
    class StandardTimer : ApplicationServiceTimer
    {
        public StandardTimer()
        {
            FirstInterval = TimeSpan.FromSeconds(1);
            Interval = TimeSpan.FromSeconds(10);
        }

        protected override void Execute()
        {
            Console.WriteLine("StandardTimer: Executing (may throw exceptions)");

        }
    }
}
