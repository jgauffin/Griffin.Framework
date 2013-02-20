using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Griffin.Framework.Validation
{
    public class FastProperty
    {
        public Func<object, object> GetDelegate;
        public Action<object, object> SetDelegate;

        public FastProperty(PropertyInfo property)
        {
            Property = property;
            InitializeGet();
            InitializeSet();
        }

        public PropertyInfo Property { get; private set; }

        public object Get(object instance)
        {
            return GetDelegate(instance);
        }

        private void InitializeGet()
        {
            var instance = Expression.Parameter(typeof (object), "instance");
            var instanceCast = (!Property.DeclaringType.IsValueType)
                                   ? Expression.TypeAs(instance, Property.DeclaringType)
                                   : Expression.Convert(instance, Property.DeclaringType);
            GetDelegate =
                Expression.Lambda<Func<object, object>>(
                    Expression.TypeAs(Expression.Call(instanceCast, Property.GetGetMethod()), typeof (object)), instance)
                          .Compile();
        }

        private void InitializeSet()
        {
            var instance = Expression.Parameter(typeof (object), "instance");
            var value = Expression.Parameter(typeof (object), "value");

            // value as T is slightly faster than (T)value, so if it's not a value type, use that
            var instanceCast = (!Property.DeclaringType.IsValueType)
                                   ? Expression.TypeAs(instance, Property.DeclaringType)
                                   : Expression.Convert(instance, Property.DeclaringType);
            var valueCast = (!Property.PropertyType.IsValueType)
                                ? Expression.TypeAs(value, Property.PropertyType)
                                : Expression.Convert(value, Property.PropertyType);
            SetDelegate =
                Expression.Lambda<Action<object, object>>(
                    Expression.Call(instanceCast, Property.GetSetMethod(), valueCast), new[] {instance, value})
                          .Compile();
        }

        public void Set(object instance, object value)
        {
            SetDelegate(instance, value);
        }
    }
}