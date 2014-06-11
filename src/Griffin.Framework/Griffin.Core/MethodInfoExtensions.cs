using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Griffin
{
    /// <summary>
    ///     Extension methods for <see cref="MethodInfo" />.
    /// </summary>
    public static class MethodInfoExtensions
    {
        /// <summary>
        ///     Delegate for <see cref="MethodInfoExtensions.ToFastDelegate" />.
        /// </summary>
        /// <param name="instance">instance to invoke the method on</param>
        /// <param name="arguments">method arguments</param>
        /// <returns>Result (if any)</returns>
        public delegate object LateBoundMethod(object instance, object[] arguments);

        /// <summary>
        ///     Create a fast delegete from the method info which also takes care of casting.
        /// </summary>
        /// <param name="method">Method to convert</param>
        /// <returns>Delegate to invoke</returns>
        public static LateBoundMethod ToFastDelegate(this MethodInfo method)
        {
            var instanceParameter = Expression.Parameter(typeof (object), "target");
            var argumentsParameter = Expression.Parameter(typeof (object[]), "arguments");

            var call = Expression.Call(
                Expression.Convert(instanceParameter, method.DeclaringType),
                method,
                CreateParameterExpressions(method, argumentsParameter));

            var lambda = Expression.Lambda<LateBoundMethod>(
                Expression.Convert(call, typeof (object)),
                instanceParameter,
                argumentsParameter);

            return lambda.Compile();
        }

        private static Expression[] CreateParameterExpressions(MethodInfo method, Expression argumentsParameter)
        {
            return method.GetParameters().Select((parameter, index) =>
                Expression.Convert(
                    Expression.ArrayIndex(argumentsParameter, Expression.Constant(index)), parameter.ParameterType))
                .ToArray();
        }
    }
}