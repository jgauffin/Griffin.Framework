using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Griffin.Logging;
using Griffin.Logging.Loggers;

namespace ServicesDemo.Logs
{
    class LogManagerProvider : ILogProvider
    {
        public ILogger GetLogger(Type typeThatWantToLog)
        {
            return new ConsoleLogger(typeThatWantToLog);
        }
    }
}
