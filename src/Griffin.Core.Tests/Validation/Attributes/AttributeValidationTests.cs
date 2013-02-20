using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Griffin.Framework.Validation;
using Xunit;

namespace Griffin.Framework.Tests.Validation.Attributes
{
    public class AttributeValidationTests
    {
        [Fact]
        public void TestEmbedded()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(1053);
            Validator.Add(new AttributeProvider());

            var model = new AttributeModel();
            model.Age = 3;
            var errors = Validator.Validate(model);

        }
    }
}
