namespace Griffin.Cqs.Authorization
{
    /// <summary>
    ///     Used to authorize inbound messages before they are executed by one of the bus's.
    /// </summary>
    public interface IAuthorizationFilter
    {
        /// <summary>
        ///     Authorize context.
        /// </summary>
        /// <param name="context">Contains information about the object being executed.</param>
        void Authorize(AuthorizationFilterContext context);
    }
}