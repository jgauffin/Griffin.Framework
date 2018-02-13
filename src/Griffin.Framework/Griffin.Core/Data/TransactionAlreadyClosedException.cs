using System;
using System.Data;
using System.Runtime.Serialization;
#if NETSTANDARD1_6
using static Griffin.Data.CommandExtensions;
#endif

namespace Griffin.Data
{
    /// <summary>
    ///     A transaction have already been closed.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Usually thrown if you have a messy data model where you do not have a full control over how a transaction is
    ///         created and/or committed.
    ///     </para>
    /// </remarks>
    public class TransactionAlreadyClosedException : DataException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionAlreadyClosedException"/> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        public TransactionAlreadyClosedException(string errorMessage)
            : base(errorMessage)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionAlreadyClosedException"/> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="inner">The inner exception.</param>
        public TransactionAlreadyClosedException(string errorMessage, Exception inner)
            : base(errorMessage, inner)
        {
        }
        
    }
}