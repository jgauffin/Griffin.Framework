using System;
using System.Linq.Expressions;

namespace Griffin.Framework.Data
{
    /// <summary>
    /// Search query (i.e. supports paging and sorting)
    /// </summary>
    /// <typeparam name="TResult">Type of entity (or subset of an entity) which will be returdned</typeparam>
    public class SearchQuery<TResult> : IOrderedQuery<TResult>, IPagedQuery<TResult>, IOrderedQueryInfo, IPagedQueryInfo where TResult : class
    {
        private int _pageSize;
        private int _pageNumber;
        private string _orderProperty;
        private bool _orderAscending = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchQuery{T}"/> class.
        /// </summary>
        /// <remarks>Will per default return the first 50 items</remarks>
        public SearchQuery()
        {
            Page(1, 50);
        }

        /// <summary>
        /// Gets number of items per page (when paging is used)
        /// </summary>
        int IPagedQueryInfo.PageSize { get { return _pageSize; }}

        /// <summary>
        /// Gets page number (one based index)
        /// </summary>
        int IPagedQueryInfo.PageNumber { get { return _pageNumber; } }

        /// <summary>
        /// Gets the kind of sort order
        /// </summary>
        bool  IOrderedQueryInfo.OrderAscending { get { return _orderAscending; } }

        /// <summary>
        /// Gets property name for the property to sort by.
        /// </summary>
        string IOrderedQueryInfo.OrderByPropertyName{ get { return _orderProperty; } }


        /// <summary>
        /// Use paging
        /// </summary>
        /// <param name="pageNumber">Page to get (one based index).</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <returns>Current instance</returns>
        public void Page(int pageNumber, int pageSize)
        {
            if (pageNumber < 1 || pageNumber > 1000)
                throw new ArgumentOutOfRangeException("pageNumber", "Page number must be between 1 and 1000.");
            if (pageSize < 1 || pageSize > 1000)
                throw new ArgumentOutOfRangeException("pageSize", "Page size must be between 1 and 1000. ");

            _pageSize = pageSize;
            _pageNumber = pageNumber;
        }

        /// <summary>
        /// Sort ascending by a property
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>Current instance</returns>
        public void OrderBy(string propertyName)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName");
            ValidatePropertyName(propertyName);

            _orderAscending = true;
            _orderProperty = propertyName;
        }

        /// <summary>
        /// Sort descending by a property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>Current instance</returns>
        public void OrderByDescending(string propertyName)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName");
            ValidatePropertyName(propertyName);

            _orderAscending = false;
            _orderProperty = propertyName;
        }

        /// <summary>
        /// Property to sort by (ascending)
        /// </summary>
        /// <param name="property">The property.</param>
        public void OrderBy(Expression<Func<TResult, object>> property)
        {
            if (property == null) throw new ArgumentNullException("property");
            var expression = property.GetMemberInfo();
            var name = expression.Member.Name;
            OrderBy(name);
        }

        /// <summary>
        /// Property to sort by (descending)
        /// </summary>
        /// <param name="property">The property</param>
        public void OrderByDescending(Expression<Func<TResult, object>> property)
        {
            if (property == null) throw new ArgumentNullException("property");
            var expression = property.GetMemberInfo();
            var name = expression.Member.Name;
            OrderByDescending(name);
        }

        /// <summary>
        /// Make sure that the property exists in the model.
        /// </summary>
        /// <param name="name">The name.</param>
        protected virtual void ValidatePropertyName(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (typeof(TResult).GetProperty(name) == null)
            {
                throw new ArgumentOutOfRangeException("name", name, string.Format("'{0}' is not a public property of '{1}'.", name,
                                                          typeof(TResult).FullName));
            }
        }

        /// <summary>
        /// To the query info.
        /// </summary>
        /// <returns></returns>
        public IQueryInfo ToQueryInfo()
        {
            return this;
        }
    }
}