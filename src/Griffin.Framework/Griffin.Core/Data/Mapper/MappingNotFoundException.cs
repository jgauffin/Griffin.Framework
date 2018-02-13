using System;
using System.Runtime.Serialization;

namespace Griffin.Data.Mapper
{
    /// <summary>
    /// We did not have a mapping configured for an entity type.
    /// </summary>
    
    public class MappingNotFoundException : MappingException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MappingNotFoundException"/> class.
        /// </summary>
        /// <param name="entityType">Entity that we did not find a mapping for.</param>
        public MappingNotFoundException(Type entityType)
            : base(entityType, "Failed to find mapper for entity '" + entityType + "'.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingNotFoundException"/> class.
        /// </summary>
        /// <param name="entityType">Entity that we did not find a mapping for.</param>
        /// <param name="inner">The inner exception.</param>
        public MappingNotFoundException(Type entityType, Exception inner)
            : base(entityType, "Failed to find mapper for entity '" + entityType + "'.", inner)
        {
        }
        
    }
}