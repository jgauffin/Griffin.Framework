using Griffin.Framework.Validation.Fluent;

namespace Griffin.Framework.Tests.Validation
{
    class Model3Validator : FluentValidator<Model3>
    {
        public Model3Validator()
        {
            Property("Skills").Required().Min(3);
            Property("Nickname").Required();
        }
    }
}
