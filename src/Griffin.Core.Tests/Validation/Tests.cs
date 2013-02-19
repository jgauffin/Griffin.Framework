using System.Globalization;
using Griffin.Framework.Validation;
using Xunit;

namespace Griffin.Framework.Tests.Validation
{
    public class Tests
    {
    

        [Fact]
        public void TestFluent()
        {
            Validator.Add(new Fluent.FluentProvider());

            Model3 model = new Model3();
            model.Skills = 3;
            var errors = Validator.Validate(model);
        }
    }
}
