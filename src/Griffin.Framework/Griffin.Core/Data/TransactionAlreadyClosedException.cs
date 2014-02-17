using System;
using System.Data;
using System.Runtime.Serialization;

namespace Griffin.Data
{
    /// <summary>
    ///     A transaction have already been closed.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Usually thrown if you have a messy data model where you do not have a full control over how a transaction is
    ///         created and/or commited.
    ///     </para>
    /// </remarks>
    public class TransactionAlreadyClosedException : DataException
    {
        public TransactionAlreadyClosedException(string errorMessage)
            : base(errorMessage)
        {
        }

        public TransactionAlreadyClosedException(string errorMessage, Exception inner)
            : base(errorMessage, inner)
        {
        }

        public TransactionAlreadyClosedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public TransactionAlreadyClosedException()
        {
        }


        /// <summary>
        ///     When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with
        ///     information about the exception.
        /// </summary>
        /// <param name="info">
        ///     The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object
        ///     data about the exception being thrown.
        /// </param>
        /// <param name="context">
        ///     The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual
        ///     information about the source or destination.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        ///     The <paramref name="info" /> parameter is a null reference (Nothing in
        ///     Visual Basic).
        /// </exception>
        /// <filterpriority>2</filterpriority>
        /// <PermissionSet>
        ///     <IPermission
        ///         class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
        ///         version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
        ///     <IPermission
        ///         class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
        ///         version="1" Flags="SerializationFormatter" />
        /// </PermissionSet>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}