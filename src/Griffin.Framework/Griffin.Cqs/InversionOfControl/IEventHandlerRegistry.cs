using System;
using System.Collections.Generic;

namespace Griffin.Cqs.InversionOfControl
{
    /// <summary>
    ///     Used to resolve all concrete event handlers for an application event.
    /// </summary>
    public interface IEventHandlerRegistry
    {
        /// <summary>
        ///     Identify all handlers for an event.
        /// </summary>
        /// <param name="type">Event type</param>
        /// <returns>List of types (concrete event handlers); empty list if there are no event handlers.</returns>
        IEnumerable<Type> Lookup(Type type);
    }
}