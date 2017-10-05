using System;
using System.Runtime.Serialization;

namespace Griffin.Data.Mapper
{
    /// <summary>
    /// We did not have a mapping configured for an entity type.
    /// </summary>
    
    public class MappingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MappingException"/> class.
        /// </summary>
        /// <param name="entityType">Entity that mapping failed for.</param>
        /// <param name="errorMessage">The error message.</param>
        public MappingException(Type entityType, string errorMessage)
            : base(errorMessage)
        {
            EntityTypeName = entityType.FullName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingException"/> class.
        /// </summary>
        /// <param name="entityType">Entity that mapping failed for.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="inner">The inner.</param>
        public MappingException(Type entityType, string errorMessage, Exception inner)
            : base(entityType.FullName + ": " + errorMessage, inner)
        {
            EntityTypeName = entityType.FullName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingException"/> class.
        /// </summary>
        protected MappingException()
        {}

        /// <summary>
        /// Full name of the entity type that we did not have a mapping for.
        /// </summary>
        public string EntityTypeName { get; private set; }
    }
}