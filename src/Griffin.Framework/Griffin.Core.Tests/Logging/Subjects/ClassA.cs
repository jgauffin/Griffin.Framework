using Griffin.Logging;

namespace Griffin.Core.Tests.Logging.Subjects
{
    internal class ClassA
    {
        ILogger _logger = LogManager.GetLogger<ClassA>();

        public void DoSomething()
        {
            _logger.Trace("ClassA is doing something.");
        }
    }
}