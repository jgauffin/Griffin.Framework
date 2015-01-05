using System;

namespace Griffin.Container
{
    /// <summary>
    ///     Default implementation of <see cref="IScopedTaskInvoker" />
    /// </summary>
    public class ScopedTaskInvoker : IScopedTaskInvoker
    {
        private readonly IContainer _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScopedTaskInvoker"/> class.
        /// </summary>
        /// <param name="container">The container adapter. Use for instance the GriffinFramework.Autofac package or implement the interface yourself.</param>
        /// <exception cref="System.ArgumentNullException">container</exception>
        public ScopedTaskInvoker(IContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");
            _container = container;
        }

        /// <summary>
        ///     Run a task on a service
        /// </summary>
        /// <typeparam name="TService">Scoped Service to run a task on</typeparam>
        /// <param name="task">Task to execute</param>
        /// <example>
        ///     <code>
        /// <![CDATA[
        /// _invoker.Execute<IUserRepository>(repos => repos.Save(model.User));
        /// ]]>
        /// </code>
        /// </example>
        public void Execute<TService>(Action<TService> task)
        {
            using (var scope = _container.CreateScope())
            {
                var service = scope.Resolve<TService>();
                task(service);
                TaskExecuted(this, new ScopedTaskEventArgs(service, scope));
            }
        }

        /// <summary>
        ///     Run a task on a service
        /// </summary>
        /// <typeparam name="TService">Scoped Service to run a task on</typeparam>
        /// <param name="task">Task to execute</param>
        /// <param name="scopeTask">Scope that was created. Use it to commit an Unit Of Work etc.</param>
        /// <example>
        ///     <code>
        /// <![CDATA[
        /// _invoker.Execute<IUserRepository>(
        ///     repos => repos.Save(model.User),
        ///     scope => scope.Resolve<IUnitOfWork>().SaveChanges()
        /// );
        /// ]]>
        /// </code>
        /// </example>
        public void Execute<TService>(Action<TService> task, Action<IContainerScope> scopeTask)
        {
            using (var scope = _container.CreateScope())
            {
                var service = scope.Resolve<TService>();
                task(service);
                scopeTask(scope);
                TaskExecuted(this, new ScopedTaskEventArgs(service, scope));
            }
        }

        /// <summary>
        ///     Called in each scope before closing it (upon successful completion).
        /// </summary>
        public event EventHandler<ScopedTaskEventArgs> TaskExecuted = delegate { };
    }
}