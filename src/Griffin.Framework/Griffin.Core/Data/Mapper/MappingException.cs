using System;
using System.Runtime.Serialization;

namespace Griffin.Data.Mapper
{
    /// <summary>
    /// We did not have a mapping configured for an entity type.
    /// </summary>
    [Serializable]
    public class MappingException : Exception
    {
        public MappingException(Type entityType, string errorMessage)
            : base(errorMessage)
        {
            EntityTypeName = entityType.FullName;
        }

        public MappingException(Type entityType, string errorMessage, Exception inner)
            : base(entityType.FullName + ": " + errorMessage, inner)
        {
            EntityTypeName = entityType.FullName;
        }

        public MappingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            EntityTypeName = info.GetString("EntityTypeName");
        }

        protected MappingException()
        {}

        /// <summary>
        /// Full name of the entity type that we did not have a mapping for.
        /// </summary>
        public string EntityTypeName { get; private set; }


        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown. </param><param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination. </param><exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is a null reference (Nothing in Visual Basic). </exception><filterpriority>2</filterpriority><PermissionSet><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter"/></PermissionSet>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("EntityTypeName", EntityTypeName);
        }

    }
}