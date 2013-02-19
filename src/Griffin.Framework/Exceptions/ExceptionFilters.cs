using System;
using System.Collections.Generic;

namespace Griffin.Framework.Exceptions
{
    /// <summary>
    /// This class is used in all Griffin libraries to log unhandled exceptions.
    /// </summary>
    /// <remarks>The exceptions will by default be thrown, disable that by setting <see cref="ThrowExceptions"/> to false</remarks>
    public class ExceptionFilters
    {
        private static readonly List<IExceptionFilter> _filters = new List<IExceptionFilter>();

        /// <summary>
        /// Initializes the <see cref="ExceptionFilters" /> class.
        /// </summary>
        static ExceptionFilters()
        {
            ThrowExceptions = true;
        }

        /// <summary>
        /// Gets or sets if exceptions should be thrown.
        /// </summary>
        public static bool ThrowExceptions { get; set; }

        /// <summary>
        /// Register a new provider.
        /// </summary>
        /// <param name="filter">Filter which will handle exceptions</param>
        public static void Register(IExceptionFilter filter)
        {
            if (filter == null) throw new ArgumentNullException("filter");

            _filters.Add(filter);
        }

        /// <summary>
        /// Remove all registered filters
        /// </summary>
        public static void Clear()
        {
            _filters.Clear();
        }

        /// <summary>
        /// Will trigger all fitlers
        /// </summary>
        /// <param name="context"></param>
        public static void Trigger(ExceptionFilterContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            context.ThrowException = ThrowExceptions;
            foreach (var filter in _filters)
            {
                filter.Handle(context);
            }
        }
    }
}
