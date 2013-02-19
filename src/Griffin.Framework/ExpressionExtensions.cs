using System;
using System.Linq.Expressions;

namespace Griffin.Framework
{
    /// <summary>
    /// Extension methods for labmda expressions
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
    }
}
