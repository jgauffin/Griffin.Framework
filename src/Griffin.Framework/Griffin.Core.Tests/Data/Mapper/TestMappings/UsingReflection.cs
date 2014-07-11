using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Griffin.Data.Mapper;

namespace Griffin.Core.Tests.Data.Mapper.TestMappings
{
    public class UsingReflection
    {
        public string Id { get; set; }
    }

    public class UsingReflectionMapper : CrudEntityMapper<UsingReflection>
    {
        public UsingReflectionMapper()
            : base("UsingReflections")
        {
        }
    }
}
