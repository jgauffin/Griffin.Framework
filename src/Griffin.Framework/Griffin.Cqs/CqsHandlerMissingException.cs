using System;
using System.Runtime.Serialization;
using DotNetCqs;

namespace Griffin.Cqs
{
    /// <summary>
    /// Did not find a handler for a specific CQS object (i.e. a subclass of <see cref="Command"/>, <see cref="Query{TResult}"/>, <see cref="ApplicationEvent"/> or <see cref="Request{TReply}"/>).
    /// </summary>
    [Serializable]
    public class CqsHandlerMissingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CqsHandlerMissingException"/> class.
        /// </summary>
        /// <param name="type">message that a handler was not found for.</param>
        public CqsHandlerMissingException(Type type)
            : base(string.Format("Missing a handler for '{0}'.", type.FullName))
        {
            CqsType = type.FullName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CqsHandlerMissingException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected CqsHandlerMissingException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
            CqsType = info.GetString("CqsType");
        }

        /// <summary>
        /// Full name of the type that we are missing a handler for.
        /// </summary>
        public string CqsType { get; set; }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown. </param><param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination. </param><exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is a null reference (Nothing in Visual Basic). </exception><filterpriority>2</filterpriority><PermissionSet><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter"/></PermissionSet>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("CqsType", CqsType);
        }
    }
}