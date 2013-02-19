using System.Collections.Generic;
using Griffin.Framework.Validation.Rules;

namespace Griffin.Framework.Validation
{
    /// <summary>
    /// Validates a specific model.
    /// </summary>
    public class ModelValidator
    {
        private readonly Dictionary<string, List<IRule>> _rules = new Dictionary<string, List<IRule>>();

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
            var errors = new ValidationErrors(model.GetType());

            foreach (var propertyInfo in model.GetType().GetProperties())
            {
                if (!propertyInfo.CanWrite && propertyInfo.GetIndexParameters().Length > 0)
                    continue;

                List<IRule> rules;
                if (!_rules.TryGetValue(propertyInfo.Name, out rules))
                    continue;

                var value = propertyInfo.GetValue(model, null);
                foreach (var rule in rules)
                {
                    var context = new ValidationContext(model, propertyInfo.Name, value);
                    if (!rule.Validate(context))
                        errors.Add(propertyInfo.Name, rule);
                }
            }

            return errors;
        }
    }
}