using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.ApplicationServices.AppDomains
{
    /// <summary>
    /// A writable configuration source
    /// </summary>
    public interface IConfigAdapter
    {
        string this[string name] { get; set; }
    }
}
