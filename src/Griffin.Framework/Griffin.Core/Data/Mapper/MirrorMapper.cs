using System;
using System.Data;

namespace Griffin.Data.Mapper
{
    /// <summary>
    /// Generates a mapping where property names and column names are identical
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MirrorMapper<T> : IEntityMapper<T>, IEntityMapper
    {
        private readonly Type _type = typeof(T);

        public void Map(IDataRecord source, T destination)
        {
            Map(source, (object) destination);
        }

        public object Create(IDataRecord record)
        {
            return Activator.CreateInstance<T>();
        }

        public void Map(IDataRecord source, object destination)
        {
            for (var i = 0; i < source.FieldCount; i++)
            {
                var name = source.GetName(i);
                var pi = _type.GetProperty(name);
                if (pi == null)
                    throw new MappingException(_type, "Field '" + name + "' cannot be mapped to a property.");

                var value = source.GetValue(i);
                if (!pi.PropertyType.IsInstanceOfType(value))
                {
                    try
                    {
                        value = Convert.ChangeType(value, pi.PropertyType);
                    }
                    catch (Exception)
                    {
                        throw new MappingException(_type,
                            string.Format("Value '{0}' [{1}] cannot be converted to {2}", value, value.GetType(),
                                pi.PropertyType));
                    }
                }

                pi.SetValue(destination, value);
            }
        }
    }
}