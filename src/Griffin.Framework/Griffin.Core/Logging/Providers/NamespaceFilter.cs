using System;
using System.Collections.Generic;
using System.Linq;

namespace Griffin.Logging.Providers
{
    /// <summary>
    ///     Allows user to filter on certain namespaces.
    /// </summary>
    public class NamespaceFilter : ILoggerFilter
    {
        private readonly List<NamespaceFltr> _allowedFilters = new List<NamespaceFltr>();
        private readonly List<NamespaceFltr> _rejectedFilters = new List<NamespaceFltr>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="NamespaceFilter" /> class.
        /// </summary>
        public NamespaceFilter()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NamespaceFilter" /> class.
        /// </summary>
        /// <param name="allowedIncludingChildNamespaces">A namespace to allow, <c>null</c> = do not add.</param>
        /// <param name="revokedIncludingChildNamespaces">A namespace to revoke, <c>null</c> = do not add.</param>
        /// <remarks>
        ///     Constructor can be used to get less bootstrapping code.
        /// </remarks>
        public NamespaceFilter(string allowedIncludingChildNamespaces = null,
            string revokedIncludingChildNamespaces = null)
        {
            if (allowedIncludingChildNamespaces != null)
                Allow(allowedIncludingChildNamespaces, true);
            if (revokedIncludingChildNamespaces != null)
                Revoke(revokedIncludingChildNamespaces, true);
        }

        /// <summary>
        ///     Checks if the specified logger may log to a certain logger.
        /// </summary>
        /// <param name="typeThatWantToLog">Type that want's to write to a log.</param>
        /// <returns>
        ///     <c>true</c> if the logging type is acceptable by this filter; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">typeThatWantToLog</exception>
        public bool IsSatisfiedBy(Type typeThatWantToLog)
        {
            if (typeThatWantToLog == null) throw new ArgumentNullException("typeThatWantToLog");

            if (_rejectedFilters.Any(filter => filter.IsSatisfiedBy(typeThatWantToLog)))
                return false;

            return _allowedFilters.Any(filter => filter.IsSatisfiedBy(typeThatWantToLog));
        }

        /// <summary>
        ///     Allows the specified the namespace.
        /// </summary>
        /// <param name="theNamespace">The namespace to allow.</param>
        /// <param name="allowChildNamespaces">if set to <c>true</c> all child namespaces is also allowed.</param>
        /// <exception cref="System.ArgumentNullException">theNamespace</exception>
        public void Allow(string theNamespace, bool allowChildNamespaces)
        {
            if (theNamespace == null) throw new ArgumentNullException("theNamespace");
            _allowedFilters.Add(new NamespaceFltr(theNamespace, allowChildNamespaces));
        }

        /// <summary>
        ///     Revokes the specified the namespace.
        /// </summary>
        /// <param name="theNamespace">The namespace.</param>
        /// <param name="includeChildNamespaces">if set to <c>true</c> all child namespaces is also revoked.</param>
        /// <exception cref="System.ArgumentNullException">theNamespace</exception>
        public void Revoke(string theNamespace, bool includeChildNamespaces)
        {
            if (theNamespace == null) throw new ArgumentNullException("theNamespace");
            _rejectedFilters.Add(new NamespaceFltr(theNamespace, includeChildNamespaces));
        }

        private class NamespaceFltr
        {
            public NamespaceFltr(string allowedNamespace, bool allowChildNamespaces)
            {
                AllowedNamespace = allowedNamespace;
                AllowChildNamespaces = allowChildNamespaces;
            }

            private string AllowedNamespace { get; set; }
            private bool AllowChildNamespaces { get; set; }

            public bool IsSatisfiedBy(Type typeThatWantToLog)
            {
                if (typeThatWantToLog.Namespace == null)
                    return false;
                if (AllowChildNamespaces)
                    return typeThatWantToLog.Namespace.StartsWith(AllowedNamespace);

                return typeThatWantToLog.Namespace.Equals(AllowedNamespace);
            }
        }
    }
}