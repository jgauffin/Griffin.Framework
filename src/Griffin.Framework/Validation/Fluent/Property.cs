using Griffin.Framework.Validation.Rules;

namespace Griffin.Framework.Validation.Fluent
{
    /// <summary>
    /// Property class which contains all fluent rules.
    /// </summary>
    /// <remarks>You can use extension methods to exend with more rules</remarks>
    public class Property
    {
        private readonly string _name;
        private readonly ModelValidator _validator;

        public Property(string name, ModelValidator validator)
        {
            _name = name;
            _validator = validator;
        }

        public Property Max(int length)
        {
            _validator.Add(_name, new MaxRule(length));
            return this;
        }

        public Property Min(int length)
        {
            _validator.Add(_name, new MinRule(length));
            return this;
        }

        public Property Required()
        {
            _validator.Add(_name, new RequiredRule());
            return this;
        }
    }
}