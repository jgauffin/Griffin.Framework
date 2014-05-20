using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.ApplicationServices
{
    public class BackgroundJobRunner
    {
        private List<Type> _types = new List<Type>();

        public void Register<T>() where T : IBackgroundJob
        {
            
        }

        public void Register(Type type)
        {
            
        }

        public void Register(Assembly assembly)
        {
            
        }
    }
}
