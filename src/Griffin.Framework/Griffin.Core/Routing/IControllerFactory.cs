using System;

namespace Griffin.Routing
{
    /// <summary>
    /// ControllerFactory interface
    /// </summary>
    /// <remarks>
    /// Instances of implementations are used to create all controllers for requests.
    /// Best entry point for IoC or DI.
    /// </remarks>
    public interface IControllerFactory
    {
        /// <summary>
        /// Creates the new controller of the specified type.
        /// </summary>
        /// <returns>The new controller.</returns>
        /// <param name="type">Type.</param>
        Controller CreateNew(Type type);
    }
}