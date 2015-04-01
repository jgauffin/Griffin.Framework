namespace Griffin.Cqs.Routing
{
    /// <summary>
    /// rule used to validate the CQS object
    /// </summary>
    public interface IRoutingRule
    {
        /// <summary>
        /// Match object against a rule
        /// </summary>
        /// <param name="cqsObject">The CQS object.</param>
        /// <returns><c>true</c> if this rule accepts the object; otherwise <c>false</c>. </returns>
        bool Match(object cqsObject);
    }
}