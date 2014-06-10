using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetCqs;

namespace Griffin.Cqs
{
    /// <summary>
    /// Allows us to wrap the generic interfaces with a non generic interface to simplify the different implementations.
    /// </summary>
    public interface IQueryExecutor
    {
        /// <summary>
        /// Execute query and get response.
        /// </summary>
        /// <param name="query">Query to execute</param>
        /// <returns>Query result</returns>
        Task<object> ExecuteAsync(IQuery query);
    }
}
