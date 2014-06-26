using System;
using System.Collections.Generic;
using Griffin.Container;

namespace Griffin.ApplicationServices
{
    /// <summary>
    ///     Uses your inversion of control container to locate services.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         No need to use this class directly, simply pass the <see cref="IContainer" /> (i.e. your IoC adapter) to the
    ///         <see cref="ApplicationServiceManager" /> constructor.
    ///     </para>
    /// </remarks>
    public class IocAppServiceLocator : IAppServiceLocator
    {
        private readonly IContainer _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="IocAppServiceLocator"/> class.
        /// </summary>
        /// <param name="container">Used to resolve <see cref="IApplicationService"/>.</param>
        /// <exception cref="System.ArgumentNullException">container</exception>
        public IocAppServiceLocator(IContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");
            _container = container;
        }

        /// <summary>
        ///     Discover all services for <see cref="ApplicationServiceManager" />.
        /// </summary>
        /// <returns>Returned services are considered to be single instances, i.e. live as long as the application.</returns>
        public IEnumerable<IApplicationService> GetServices()
        {
            return _container.ResolveAll<IApplicationService>();
        }
    }
}