#if NETSTANDARD1_5
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.Data
{
    public class DataException : DbException
    {
        protected DataException()
        {
            
        }
        public DataException(string message) : base(message)
        {
            
        }

        public DataException(string message, Exception inner) : base(message, inner)
        {
            
        }
    }
}
#endif