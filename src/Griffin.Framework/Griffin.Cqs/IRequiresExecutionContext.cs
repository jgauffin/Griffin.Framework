namespace Griffin.Cqs
{
    /// <summary>
    ///     Tells that a handler requires an execution context
    /// </summary>
    /// <remarks>
    ///     Include this interface on your command/query/event handler to specify that you need to get context information
    ///     about the message that will be executed next.
    /// </remarks>
    public interface IRequiresExecutionContext
    {
        /// <summary>
        ///     Information specified by the bus that will invoke the handler.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The context is typically derived by the bus implementation to provide information about the message and it's
        ///         source. The other mean
        ///         is to include information in the <see cref="Cqs.ExecutionContext.Parameters" /> dictionary.
        ///     </para>
        /// </remarks>
        ExecutionContext ExecutionContext { get; set; }
    }
}