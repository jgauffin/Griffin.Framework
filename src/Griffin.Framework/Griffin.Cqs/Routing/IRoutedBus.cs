namespace Griffin.Cqs.Routing
{
    /// <summary>
    /// Used to determine if the bus that implements this interface can handle the specified CQS object.
    /// </summary>
    public interface IRoutedBus
    {
        /// <summary>
        /// Match object against a rule
        /// </summary>
        /// <param name="cqsObject">The CQS object.</param>
        /// <returns><c>true</c> if this bus can handle the specified object; otherwise <c>false</c>. </returns>
        bool Match(object cqsObject);
    }
}