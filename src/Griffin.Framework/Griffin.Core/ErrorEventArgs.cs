#if !NET45
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griffin
{
    /// <summary>
    /// Shim for corlib.
    /// </summary>
    public class ErrorEventArgs : EventArgs
    {
        private readonly Exception _exception;

        public ErrorEventArgs(Exception exception)
        {
            if (exception == null) throw new ArgumentNullException("exception");
            _exception = exception;
        }

        /// <summary>
        /// Returns exception
        /// </summary>
        /// <returns></returns>
        public virtual Exception GetException()
        {
            return _exception;
        }
    }
}
#endif