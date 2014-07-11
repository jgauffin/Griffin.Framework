using System;

namespace Griffin.Data.Mapper
{
    /// <summary>
    ///     Will point on the entity that a mapping is for if the <see cref="ICrudEntityMapper" /> interface is used instead of the
    ///     generic one.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MappingForAttribute : Attribute
    {
        /// <summary>
        /// </summary>
        /// <param name="entityType"></param>
        public MappingForAttribute(Type entityType)
        {
            if (entityType == null) throw new ArgumentNullException("entityType");
            EntityType = entityType;
        }

        /// <summary>
        ///     Type of entity that the decorated entity mapper is for.
        /// </summary>
        public Type EntityType { get; private set; }
    }
}