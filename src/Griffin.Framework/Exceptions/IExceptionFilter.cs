namespace Griffin.Framework.Exceptions
{
    /// <summary>
    /// Used to log exceptions which has not been handled.
    /// </summary>
    public interface IExceptionFilter
    {
        /// <summary>
        /// Handle exception
        /// </summary>
        /// <param name="context">Context information</param>
        void Handle(ExceptionFilterContext context);
    }
}