using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Griffin
{
    /// <summary>
    ///     Credits http://rogeralsing.com/2008/02/28/linq-expressions-creating-objects/
    /// </summary>
    public static class ConstructorExtensions
    {
        /// <summary>
        ///     Creates a delegate which allocates a new object faster than  <see cref="Activator.CreateInstance()" />.
        /// </summary>
        /// <param name="ctor">The ctor.</param>
        /// <returns>The activator</returns>
        /// <remarks>
        ///     The method uses an expression tree to build
        ///     a delegate for the specified constructor
        /// </remarks>
        public static InstanceFactory CreateFactory(this ConstructorInfo ctor)
        {
            if (ctor == null) throw new ArgumentNullException("ctor");

            var paramsInfo = ctor.GetParameters();
            var param = Expression.Parameter(typeof(object[]), "args");

            var argsExp = new Expression[paramsInfo.Length];

            //pick each arg from the params array 
            //and create a typed expression of them
            for (var i = 0; i < paramsInfo.Length; i++)
            {
                Expression index = Expression.Constant(i);
                var paramType = paramsInfo[i].ParameterType;
                Expression paramAccessorExp = Expression.ArrayIndex(param, index);
                Expression paramCastExp = Expression.Convert(paramAccessorExp, paramType);
                argsExp[i] = paramCastExp;
            }

            //make a NewExpression that calls the
            //ctor with the args we just created
            var newExp = Expression.New(ctor, argsExp);

            //create a lambda with the New
            //Expression as body and our param object[] as arg
            var lambda = Expression.Lambda(typeof(InstanceFactory), newExp, param);

            //compile it
            return (InstanceFactory)lambda.Compile();
        }
    }
}
