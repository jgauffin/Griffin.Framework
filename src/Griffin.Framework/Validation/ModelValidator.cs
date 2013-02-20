using System;
using System.Collections.Generic;
using Griffin.Framework.Text;
using Griffin.Framework.Validation.Rules;

namespace Griffin.Framework.Validation
{
    /// <summary>
    /// Validates a specific model.
    /// </summary>
    public class ModelValidator
    {
        private readonly Type _modelType;
        private readonly Dictionary<string, List<IRule>> _rules = new Dictionary<string, List<IRule>>();
        private List<FastProperty> _properties = new List<FastProperty>();

        public ModelValidator(Type modelType)
        {
            _modelType = modelType;
            _properties = new List<FastProperty>();
            foreach (var propertyInfo in _modelType.GetProperties())
            {
                if (!propertyInfo.CanWrite || !propertyInfo.CanRead || propertyInfo.GetIndexParameters().Length > 0)
                    continue;

                _properties.Add(new FastProperty(propertyInfo));
            }
        }
        /// <summary>
        /// Add a rule for a property.
        /// </summary>
        /// <param name="name">Property name</param>
        /// <param name="rule">Rule to validate property against</param>
        public void Add(string name, IRule rule)
        {
            List<IRule> rules;
            if (!_rules.TryGetValue(name, out rules))
            {
                rules = new List<IRule>();
                _rules.Add(name, rules);
            }

            rules.Add(rule);
        }

        /// <summary>
        /// Validates a model.
        /// </summary>
        /// <param name="model">Model to validate</param>
        /// <returns>On or more errors if any rule failed; otherwise an empty collection.</returns>
        public ValidationErrors Validate(object model)
        {
            if (model == null) throw new ArgumentNullException("model");

            var errors = new ValidationErrors(model.GetType());

            foreach (var prop in _properties)
            {
                List<IRule> rules;
                if (!_rules.TryGetValue(prop.Property.Name, out rules))
                    continue;

                var value = prop.Get(model);
                foreach (var rule in rules)
                {
                    var context = new ValidationContext(model, prop.Property.Name, value);
                    if (rule.Validate(context)) 
                        continue;

                    var field = Localize.A.Type(model.GetType(), prop.Property.Name);
                    var msg = rule.Format(field, context);
                    errors.Add(prop.Property.Name, rule, msg);
                }
            }

            return errors;
        }
    }
}