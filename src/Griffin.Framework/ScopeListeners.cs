using System;
using System.Collections.Generic;
using System.Linq;

namespace Griffin.Framework
{
    /// <summary>
    /// Scopes defines operations in some way.
    /// </summary>
    /// <remarks>
    /// <para>For instance web applications uses scoping to capture work during a HTTP request, while Griffin.Decoupled uses scoping to wrap each command.
    /// </para>
    /// <para>This class is invoked by different parts of the framework to handle those scopes and therefore allow you to perform additional operations for those scopes.</para>
    /// </remarks>
    public class ScopeListeners : IScopePublisher
    {
        private static List<IScopeListener> _listeners = new List<IScopeListener>();
        private static List<IScopePublisher> _publishers = new List<IScopePublisher>();
        private static ScopeListeners _instance = new ScopeListeners();

        /// <summary>
        /// Add a new subscriber
        /// </summary>
        /// <param name="listener"></param>
        public static void Subscribe(IScopeListener listener)
        {
            var listeners = _listeners.ToList();
            listeners.Add(listener);
            _listeners = listeners;
        }

        /// <summary>
        /// Register a new publisher
        /// </summary>
        /// <param name="name">publisher</param>
        /// <returns></returns>
        public static IScopePublisher Register(string name)
        {
            return _instance;
        }

        /// <summary>
        /// Start a scope
        /// </summary>
        /// <param name="identifier"></param>
        public void TriggerStarting(object identifier)
        {
            Iterate(x => x.ScopeStarting(identifier));
        }

        /// <summary>
        /// Scope started
        /// </summary>
        /// <param name="identifier"></param>
        public void TriggerStarted(object identifier)
        {
            Iterate(x => x.ScopeStarted(identifier));
        }

        /// <summary>
        /// Scope is completed but not yet disposed.
        /// </summary>
        /// <param name="identifier"></param>
        public void TriggerEnding(object identifier)
        {
            Iterate(x => x.ScopeEnding(identifier));
        }

        /// <summary>
        /// Scope has been disposed.
        /// </summary>
        /// <param name="identifier"></param>
        public void TriggerEnded(object identifier)
        {
            Iterate(x=>x.ScopeEnded(identifier));
        }

        /// <summary>
        /// Remove all listeners.
        /// </summary>
        public static void Clear()
        {
            _listeners = new List<IScopeListener>();
        }

        private void Iterate(Action<IScopeListener> action)
        {
            var listeners = _listeners;
            foreach (var listener in listeners)
            {
                action(listener);
            }
        }

    }
}
