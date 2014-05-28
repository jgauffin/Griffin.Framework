using System;

namespace Griffin.Net.Buffers
{
    /// <summary>
    ///     A object pool (or similar) have no more items to give out.
    /// </summary>
    /// <remarks>This exception typically occurrs if the pool/stack is too small (too many concurrent operations) or if some code fail to return the item to the pool when done.</remarks>
    public class PoolEmptyException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PoolEmptyException"/> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        public PoolEmptyException(string errorMessage)
            : base(errorMessage)
        {
        }
    }
}