using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Griffin.Data.Mapper;

namespace Griffin.Core.Tests.Data.Mapper.TestMappings
{
    class OkMapping : CrudEntityMapper<Ok>
    {
        public OkMapping() : base("OkMappings")
        {
        }

    }
}
