using System;
using Griffin.Logging.Providers;

namespace Griffin.Logging
{
    /// <summary>
    ///     Logging facade
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Implement a <see cref="ILogProvider" /> and assign it using the property <see cref="Provider" />. You can also
    ///         use the default one named <see cref="LogProvider" />.
    ///     </para>
    ///     <para>The default behavior is to not log at all.</para>
    /// </remarks>
    public class LogManager
    {
        private static ILogProvider _current;
        private static readonly object SynLock = new object();

        /// <summary>
        ///     Get the current adapter
        /// </summary>
        public static ILogProvider Provider
        {
            get
            {
                if (_current == null)
                {
                    lock (SynLock)
                    {
                        if (_current == null)
                        {
                            //not a problem on x86 & x64
                            // ReSharper disable PossibleMultipleWriteAccessInDoubleCheckLocking
                            _current = new NullLogProvider();
                            // ReSharper restore PossibleMultipleWriteAccessInDoubleCheckLocking
                        }
                    }
                }

                return _current;
            }
            set { _current = value ?? new NullLogProvider(); }
        }

        /// <summary>
        ///     Get a logger
        /// </summary>
        /// <typeparam name="T">Type requesting a logger</typeparam>
        /// <returns>A logger</returns>
        public static ILogger GetLogger<T>() where T : class
        {
            return Provider.GetLogger(typeof(T));
        }

        /// <summary>
        ///     Get a logger
        /// </summary>
        /// <param name="typeThatWantsToLog">Type that will log messages</param>
        /// <returns>A logger</returns>
        public static ILogger GetLogger(Type typeThatWantsToLog)
        {
            return Provider.GetLogger(typeThatWantsToLog);
        }
    }
}