using System;

namespace Griffin.Container
{
    /// <summary>
    ///     Run an IoC registered service that requires a scope.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         When you run singleton services you might need to be able to invoke scoped tasks. This
    ///         contract abstracts away the creation/deletion of custom scopes, thus a hard dependency towards
    ///         the container.
    ///     </para>
    /// </remarks>
    public interface IScopedTaskInvoker
    {
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
        void Execute<TService>(Action<TService> task);


        /// <summary>
        ///     Run a task on a service
        /// </summary>
        /// <typeparam name="TService">Scoped Service to run a task on</typeparam>
        /// <param name="task">Task to execute</param>
        /// <param name="scope">Scope that was create it. Use it to commit an Unit Of Work etc.</param>
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
        void Execute<TService>(Action<TService> task, Action<IContainerScope> scope);
    }
}