using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Griffin.Framework.Validation.Attributes;
using Xunit;

namespace Griffin.Framework.Tests.Validation.Attributes
{
    public class RequiredAttributeTests
    {
        [Fact]
        public void Test()
        {
            var attr = new RequiredAttribute();
            var rule = attr.CreateRule();
            rule.Validate(null);
        }
    }
}
