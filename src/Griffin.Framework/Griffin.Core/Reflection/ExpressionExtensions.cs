using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Griffin.Reflection
{
    /// <summary>
    ///     Small helpers for expressions
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        /// Gets the member info.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns></returns>
        /// <remarks>Used to get property information</remarks>
        public static MemberExpression GetMemberInfo(this Expression method)
        {
            var lambda = method as LambdaExpression;
            if (lambda == null)
                throw new ArgumentNullException("method");

            MemberExpression memberExpr = null;
            switch (lambda.Body.NodeType)
            {
                case ExpressionType.Convert:
                    memberExpr =
                        ((UnaryExpression)lambda.Body).Operand as MemberExpression;
                    break;
                case ExpressionType.MemberAccess:
                    memberExpr = lambda.Body as MemberExpression;
                    break;
            }

            if (memberExpr == null)
                throw new ArgumentException("method");

            return memberExpr;
        }

        /// <summary>
        /// Get property information.
        /// </summary>
        /// <typeparam name="TSource">Entity type.</typeparam>
        /// <typeparam name="TProperty">Expression pointing at the property.</typeparam>
        /// <param name="propertyLambda">The property lambda.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">
        /// </exception>
        public static PropertyInfo GetPropertyInfo<TSource, TProperty>(this Expression<Func<TSource, TProperty>> propertyLambda)
        {
            var type = typeof(TSource);

            var member = propertyLambda.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    propertyLambda));

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    propertyLambda));

            if (type != propInfo.ReflectedType &&
                !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a property that is not from type {1}.",
                    propertyLambda,
                    type));

            return propInfo;
        }
    }
}