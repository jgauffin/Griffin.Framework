using Griffin.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpRouting
{
    class ActivatorControllerFactory : IControllerFactory
    {
        public Controller CreateNew(Type type)
        {
            return (Controller)Activator.CreateInstance(type);
        }
    }
}
