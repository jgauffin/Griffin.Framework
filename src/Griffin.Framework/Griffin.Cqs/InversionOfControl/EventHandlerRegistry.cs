using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotNetCqs;

namespace Griffin.Cqs.InversionOfControl
{
    /// <summary>
    ///     Keeps track of all event handler implementations. Typically to allow <see cref="SeparateScopesIocEventBus" /> to
    ///     use one scope
    ///     per handler without additional lookups.
    /// </summary>
    public class EventHandlerRegistry : IEventHandlerRegistry
    {
        private readonly Dictionary<Type, List<Type>> _eventHandlers = new Dictionary<Type, List<Type>>();

        /// <summary>
        ///     Identify all handlers for an event.
        /// </summary>
        /// <param name="type">Event type</param>
        /// <returns>List of types (concrete event handlers); empty list if there are no event handlers.</returns>
        public IEnumerable<Type> Lookup(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (!typeof (ApplicationEvent).IsAssignableFrom(type))
                throw new ArgumentException("'" + type.FullName + "' is not an Message.");

            List<Type> handlers;
            if (!_eventHandlers.TryGetValue(type, out handlers))
                return new Type[0];
            return handlers;
        }

        /// <summary>
        ///     Map a handler to an event.
        /// </summary>
        /// <typeparam name="TApplicationEvent">Type of event being handled</typeparam>
        /// <param name="concreateEventSubscriber">Concrete handler</param>
        /// <exception cref="ArgumentException">
        ///     'concreateEventSubscriber' do not implement IApplicationEventSubscriber<![CDATA[<TApplicationEvent>]]>.
        /// </exception>
        public void Map<TApplicationEvent>(Type concreateEventSubscriber) where TApplicationEvent : ApplicationEvent
        {
            var itf = concreateEventSubscriber.GetInterfaces().FirstOrDefault(IsEventHandler);
            if (itf == null)
                throw new ArgumentException(string.Format("'{0}' do not implement IApplicationEventSubscriber<{1}>.",
                    concreateEventSubscriber.FullName, typeof (TApplicationEvent).Name));

            var eventType = typeof (TApplicationEvent);
            List<Type> handlers;
            if (!_eventHandlers.TryGetValue(eventType, out handlers))
            {
                handlers = new List<Type>();
                _eventHandlers[eventType] = handlers;
            }
            handlers.Add(concreateEventSubscriber);
        }

        /// <summary>
        ///     Scan assembly after handlers.
        /// </summary>
        /// <param name="assembly">
        ///     Assembly to scan for implementations of
        ///     <c><![CDATA[IApplicationEventSubscriber<TEventType>]]></c>
        /// </param>
        public void ScanAssembly(Assembly assembly)
        {
            var types = from type in assembly.GetTypes()
                let intfs = type.GetInterfaces()
                where intfs.Any(IsEventHandler)
                select type;
            foreach (var type in types)
            {
                var eventType = type.GetInterfaces().First(IsEventHandler).GetGenericArguments()[0];
                List<Type> handlers;
                if (!_eventHandlers.TryGetValue(eventType, out handlers))
                {
                    handlers = new List<Type>();
                    _eventHandlers[eventType] = handlers;
                }
                handlers.Add(type);
            }
        }


        private static bool IsEventHandler(Type x)
        {
            return x.IsGenericType && x.GetGenericTypeDefinition() == typeof (IApplicationEventSubscriber<>);
        }
    }
}