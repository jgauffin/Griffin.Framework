using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.Container
{
    public class DependencyMissingException : Exception
    {
        public DependencyMissingException(string message, Exception inner):base(message)
        {
            throw new NotImplementedException();
        }
    }
}
