namespace Griffin.Framework.Validation.Fluent
{
    /// <summary>
    /// Validate a model.
    /// </summary>
    /// <typeparam name="T">Type of model</typeparam>
    /// <remarks>This class will automatically discovered as long as it's in the same assembly as the model to validate.
    /// Otherwise you'll have to register it in the <see cref="FluentProvider"/>.</remarks>
    public class FluentValidator<T> : FluentValidator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FluentValidator{T}" /> class.
        /// </summary>
        public FluentValidator()
            : base(typeof (T))
        {
        }
    }
}