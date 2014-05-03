using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.Container
{
    public class ServiceNotRegisteredException : Exception
    {
        public ServiceNotRegisteredException(Type serviceType, Exception inner)
        {
        throw new NotImplementedException();
        }
    }
}
