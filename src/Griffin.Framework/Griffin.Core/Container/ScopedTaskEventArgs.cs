using System;

namespace Griffin.Container
{
    /// <summary>
    ///     Event args for <see cref="ScopedTaskInvoker.TaskExecuted" />.
    /// </summary>
    public class ScopedTaskEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ScopedTaskEventArgs" /> class.
        /// </summary>
        /// <param name="taskService">The task service.</param>
        /// <param name="scope">The scope.</param>
        /// <exception cref="System.ArgumentNullException">
        ///     taskService
        ///     or
        ///     scope
        /// </exception>
        public ScopedTaskEventArgs(object taskService, IContainerScope scope)
        {
            if (taskService == null) throw new ArgumentNullException("taskService");
            if (scope == null) throw new ArgumentNullException("scope");
            TaskService = taskService;
            Scope = scope;
        }

        /// <summary>
        ///     Service that the task was executed in.
        /// </summary>
        public object TaskService { get; private set; }

        /// <summary>
        ///     Scope used to service the service.
        /// </summary>
        public IContainerScope Scope { get; private set; }
    }
}