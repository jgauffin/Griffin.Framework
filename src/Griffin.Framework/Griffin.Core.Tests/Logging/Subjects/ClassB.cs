using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Griffin.Logging;

namespace Griffin.Core.Tests.Logging.Subjects
{
    class ClassB
    {
        ILogger _logger = LogManager.GetLogger<ClassB>();

        public void DoSomething()
        {
            _logger.Trace("ClassB is doing something.");
        }
    }
}
