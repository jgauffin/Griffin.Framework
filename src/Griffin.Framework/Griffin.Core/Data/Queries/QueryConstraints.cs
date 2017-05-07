using System;
using System.Linq.Expressions;
using Griffin.Reflection;

namespace Griffin.Data.Queries
{
    /// <summary>
    ///     Typed constraints
    /// </summary>
    /// <typeparam name="T">Model to query</typeparam>
    public class QueryConstraints<T> : IQueryConstraints<T> where T : class
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="QueryConstraints{T}" /> class.
        /// </summary>
        /// <remarks>Will per default return the first 50 items</remarks>
        public QueryConstraints()
        {
            ModelType = typeof (T);
            PageSize = 50;
            PageNumber = -1;
        }

        /// <summary>
        ///     Gets start record (in the data source)
        /// </summary>
        /// <remarks>Calculated with the help of PageNumber and PageSize.</remarks>
        public int StartRecord
        {
            get
            {
                if (PageNumber <= 1)
                    return 0;

                return (PageNumber - 1)*PageSize;
            }
        }

        /// <summary>
        ///     Gets model which will be queried.
        /// </summary>
        protected Type ModelType { get; set; }

        /// <summary>
        ///     Make sure that the property exists in the model.
        /// </summary>
        /// <param name="name">The name.</param>
        protected virtual void ValidatePropertyName(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (ModelType.GetProperty(name) == null)
            {
                throw new ArgumentException(string.Format("'{0}' is not a public property of '{1}'.", name,
                    ModelType.FullName));
            }
        }

        #region IQueryConstraints<T> Members

        /// <summary>
        ///     Gets number of items per page (when paging is used)
        /// </summary>
        public int PageSize { get; private set; }

        /// <summary>
        ///     Gets page number (one based index)
        /// </summary>
        public int PageNumber { get; private set; }

        /// <summary>
        ///     Gets the kind of sort order
        /// </summary>
        public SortOrder SortOrder { get; private set; }

        /// <summary>
        ///     Gets property name for the property to sort by.
        /// </summary>
        public string SortPropertyName { get; private set; }


        /// <summary>
        ///     Use paging
        /// </summary>
        /// <param name="pageNumber">Page to get (one based index).</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <returns>Current instance</returns>
        public IQueryConstraints<T> Page(int pageNumber, int pageSize)
        {
            if (pageNumber < 1 || pageNumber > 1000)
                throw new ArgumentOutOfRangeException("pageNumber", "Page number must be between 1 and 1000.");
            if (pageSize < 1 || pageSize > 1000)
                throw new ArgumentOutOfRangeException("pageSize", "Page size must be between 1 and 1000.");

            PageSize = pageSize;
            PageNumber = pageNumber;
            return this;
        }

        /// <summary>
        ///     Sort ascending by a property
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>Current instance</returns>
        public IQueryConstraints<T> SortBy(string propertyName)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName");
            ValidatePropertyName(propertyName);

            SortOrder = SortOrder.Ascending;
            SortPropertyName = propertyName;
            return this;
        }

        /// <summary>
        ///     Sort descending by a property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>Current instance</returns>
        public IQueryConstraints<T> SortByDescending(string propertyName)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName");
            ValidatePropertyName(propertyName);

            SortOrder = SortOrder.Descending;
            SortPropertyName = propertyName;
            return this;
        }

        /// <summary>
        ///     Property to sort by (ascending)
        /// </summary>
        /// <param name="property">The property.</param>
        public IQueryConstraints<T> SortBy(Expression<Func<T, object>> property)
        {
            if (property == null) throw new ArgumentNullException("property");
            var expression = property.GetMemberInfo();
            var name = expression.Member.Name;
            SortBy(name);
            return this;
        }

        /// <summary>
        ///     Property to sort by (descending)
        /// </summary>
        /// <param name="property">The property</param>
        public IQueryConstraints<T> SortByDescending(Expression<Func<T, object>> property)
        {
            if (property == null) throw new ArgumentNullException("property");
            var expression = property.GetMemberInfo();
            var name = expression.Member.Name;
            SortByDescending(name);
            return this;
        }

        #endregion
    }
}
