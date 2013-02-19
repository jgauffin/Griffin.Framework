using System;
using System.Collections.Generic;
using System.Linq;

namespace Griffin.Framework.Authentication.Password.Validation
{
    /// <summary>
    /// Uses one or more of the other validators.
    /// </summary>
    public class CompositeValidation : ValidatorBase
    {
        private readonly IPasswordValidator[] _validators;

        public CompositeValidation(params IPasswordValidator[] validators)
        {
            if (validators == null) throw new ArgumentNullException("validators");
            _validators = validators;
        }
        
        /// <summary>
        /// Validate password
        /// </summary>
        /// <param name="password">Password</param>
        /// <returns>Empty list if successful.</returns>
        public override IEnumerable<IPasswordValidationError> TryValidate(string password)
        {
            if (password == null) throw new ArgumentNullException("password");
            return _validators.SelectMany(validator => validator.TryValidate(password));
        }
    }
}
