using System;
using System.Runtime.Serialization;

namespace Griffin.Data.Mapper
{
    /// <summary>
    /// We did not have a mapping configured for an entity type.
    /// </summary>
    [Serializable]
    public class MappingNotFoundException : MappingException
    {
        public MappingNotFoundException(Type entityType)
            : base(entityType, "Failed to find mapper for entity '" + entityType + "'.")
        {
        }

        public MappingNotFoundException(Type entityType, Exception inner)
            : base(entityType, "Failed to find mapper for entity '" + entityType + "'.", inner)
        {
        }

        public MappingNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}