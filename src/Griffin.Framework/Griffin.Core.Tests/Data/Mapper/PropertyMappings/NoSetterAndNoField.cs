using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.Core.Tests.Data.Mapper.PropertyMappings
{
    class NoSetterAndNoField
    {
        private string __iiid = "11";

        public string Id { get { return __iiid; } }
    }
}
