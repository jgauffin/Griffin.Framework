using System;
using System.Linq;
using System.Linq.Expressions;

namespace Griffin.Framework.Data
{
    /// <summary>
    /// Extension methods used to apply query paging/sorting to LINQ queries.
    /// </summary>
    public static class SearchQueryToLinqExtensions
    {
        /// <summary>
        /// Apply the query information to a LINQ statement 
        /// </summary>
        /// <typeparam name="T">Model type</typeparam>
        /// <param name="instance">contraints instance</param>
        /// <param name="query">LINQ queryable</param>
        /// <returns>Modified query</returns>
        public static IQueryable<T> ApplyTo<T>(this IQuery<T> instance, IQueryable<T> query) where T : class
        {
            if (instance == null) throw new ArgumentNullException("instance");
            if (query == null) throw new ArgumentNullException("query");

            var info = instance.ToQueryInfo();
            var sortInfo = info as IOrderedQueryInfo;
            if (sortInfo != null && !string.IsNullOrEmpty(sortInfo.OrderByPropertyName))
            {
                query = sortInfo.OrderAscending
                            ? query.OrderBy(sortInfo.OrderByPropertyName)
                            : query.OrderByDescending(sortInfo.OrderByPropertyName);
            }


            var pageInfo = info as IPagedQueryInfo;
            if (pageInfo != null && pageInfo.PageNumber >= 1)
            {
                query = query.Skip((pageInfo.PageNumber - 1)*pageInfo.PageSize).Take(pageInfo.PageSize);
            }

            return query;
        }

        /// <summary>
        /// Apply ordering to a LINQ query
        /// </summary>
        /// <typeparam name="T">Model type</typeparam>
        /// <param name="source">Linq query</param>
        /// <param name="propertyName">Property to sort by</param>
        /// <param name="values">DUNNO?</param>
        /// <returns>Ordered query</returns>
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string propertyName, params object[] values)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (propertyName == null) throw new ArgumentNullException("propertyName");

            var type = typeof (T);
            var property = type.GetProperty(propertyName);
            var parameter = Expression.Parameter(type, "p");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExp = Expression.Lambda(propertyAccess, parameter);
            var resultExp = Expression.Call(typeof (Queryable), "OrderBy", new[] {type, property.PropertyType},
                                            source.Expression, Expression.Quote(orderByExp));
            return source.Provider.CreateQuery<T>(resultExp);
        }

        /// <summary>
        /// Apply ordering to a LINQ query
        /// </summary>
        /// <typeparam name="T">Model type</typeparam>
        /// <param name="source">Linq query</param>
        /// <param name="propertyName">Property to sort by</param>
        /// <param name="values">DUNNO?</param>
        /// <returns>Ordered query</returns>
        public static IQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string propertyName,
                                                         params object[] values)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (propertyName == null) throw new ArgumentNullException("propertyName");

            var type = typeof (T);
            var property = type.GetProperty(propertyName);
            var parameter = Expression.Parameter(type, "p");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExp = Expression.Lambda(propertyAccess, parameter);
            var resultExp = Expression.Call(typeof (Queryable), "OrderByDescending", new[] {type, property.PropertyType},
                                            source.Expression, Expression.Quote(orderByExp));
            return source.Provider.CreateQuery<T>(resultExp);
        }

        /// <summary>
        /// Execute LINQ query and fill a result.
        /// </summary>
        /// <typeparam name="T">Model type</typeparam>
        /// <param name="instanz">The instanz.</param>
        /// <param name="query">Query to execute. Do note that you have to have applied the query yourself first (this will only apply sorting and paging).</param>
        /// <returns>Search reuslt</returns>
        public static PagedResult<T> ToPagedResult<T>(this IQueryable<T> instanz, IQuery<T> query)
            where T : class
        {
            if (instanz == null) throw new ArgumentNullException("instanz");
            if (query == null) throw new ArgumentNullException("query");

            var totalCount = instanz.Count();
            var limitedQuery = query.ApplyTo(instanz);
            return new PagedResult<T>(limitedQuery, totalCount);
        }

        /// <summary>
        /// Execute LINQ query and fill a result.
        /// </summary>
        /// <typeparam name="TFrom">Database Model type</typeparam>
        /// <typeparam name="TTo">Query result item type</typeparam>
        /// <param name="instanz">The instanz.</param>
        /// <param name="query">Query to execute. Do note that you have to have applied the query yourself first (this will only apply sorting and paging).</param>
        /// <param name="converter">Method used to convert the result </param>
        /// <returns>Search reuslt</returns>
        public static PagedResult<TTo> ToPagedResult<TFrom, TTo>(this IQueryable<TFrom> instanz,
                                                                 IQuery<TFrom> query,
                                                                 Func<TFrom, TTo> converter)
            where TFrom : class
            where TTo : class
        {
            if (instanz == null) throw new ArgumentNullException("instanz");
            if (query == null) throw new ArgumentNullException("query");

            var totalCount = instanz.Count();
            var limitedQuery = query.ApplyTo(instanz);
            return new PagedResult<TTo>(limitedQuery.Select(converter), totalCount);
        }
    }
}