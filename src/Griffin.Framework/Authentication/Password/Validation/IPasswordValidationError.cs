namespace Griffin.Framework.Authentication.Password.Validation
{
    /// <summary>
    /// A violation
    /// </summary>
    public interface IPasswordValidationError
    {
        /// <summary>
        /// Gets validator that generated the error
        /// </summary>
        IPasswordValidator Source { get; }

        /// <summary>
        /// Gets error
        /// </summary>
        string ErrorMessage { get; }
    }
}