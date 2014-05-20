using System;
using System.Runtime.Serialization;

namespace Griffin
{
    /// <summary>
    ///     Queue is full and no more items may be enqueued.
    /// </summary>
    [Serializable]
    public class QueueFullException : Exception
    {
        private readonly string _queueName;

        /// <summary>
        ///     Initializes a new instance of the <see cref="QueueFullException" /> class.
        /// </summary>
        /// <param name="queueName">Name of the queue.</param>
        public QueueFullException(string queueName)
            : base("Queue '" + queueName + "' is full.")
        {
            _queueName = queueName;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="QueueFullException" /> class.
        /// </summary>
        /// <param name="queueName">Name of the queue.</param>
        /// <param name="inner">The inner.</param>
        public QueueFullException(string queueName, Exception inner)
            : base("Queue '" + queueName + "' is full.", inner)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="QueueFullException" /> class.
        /// </summary>
        /// <param name="info">
        ///     The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object
        ///     data about the exception being thrown.
        /// </param>
        /// <param name="context">
        ///     The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual
        ///     information about the source or destination.
        /// </param>
        protected QueueFullException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
            _queueName = info.GetString("QueueName");
        }

        /// <summary>
        ///     Name of the queue that is full.
        /// </summary>
        public string QueueName
        {
            get { return _queueName; }
        }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter" />
        ///   </PermissionSet>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("QueueName", _queueName);
            base.GetObjectData(info, context);
        }
    }
}