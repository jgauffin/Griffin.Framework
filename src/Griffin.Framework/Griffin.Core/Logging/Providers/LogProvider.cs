using System;
using System.Collections.Generic;
using Griffin.Logging.Loggers;

namespace Griffin.Logging.Providers
{
    /// <summary>
    ///     Default implementation of a log factory.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The first matching logger is the one that will be returned when a logger is requested. So add the most
    ///         restrictive logger first (if the first logger is not using any filter it will be returned every time).
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// var factory = new LogFactory();
    /// factory.Add(new NamespaceFilter("Griffin", true), type => new ConsoleLogger(type));
    /// LogManager.Factory = factory;
    /// </code>
    /// </example>
    public class LogProvider : ILogProvider
    {
        private readonly List<Tuple<ILoggerFilter, ILogger>> _loggers = new List<Tuple<ILoggerFilter, ILogger>>();

        /// <summary>
        ///     Creates the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Logger to use (or <see cref="NullLogger.Instance" /> if none is found)</returns>
        /// <exception cref="System.ArgumentNullException">type</exception>
        public ILogger GetLogger(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            ILogger resultLogger = null;
            CompositeLogger compositeLogger = null;
            foreach (var logger in _loggers)
            {
                if (!logger.Item1.IsSatisfiedBy(type))
                    continue;

                if (resultLogger != null)
                {
                    if (compositeLogger == null)
                    {
                        compositeLogger = new CompositeLogger(type);
                        compositeLogger.Add(resultLogger);
                    }

                    compositeLogger.Add(logger.Item2);
                }
                else
                    resultLogger = logger.Item2;
            }

            return compositeLogger ?? resultLogger ?? NullLogger.Instance;
        }

        /// <summary>
        ///     Add a logger
        /// </summary>
        /// <param name="logger">Add a logger (will be used for all classes that want to log)</param>
        /// <remarks>
        ///     <para>
        ///         If you want to limit which classes a logger should be able to handle you need to use the other overload:
        ///         <see cref="Add(ILogger, ILoggerFilter)" />
        ///     </para>
        /// </remarks>
        public void Add(ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException("logger");

            _loggers.Add(new Tuple<ILoggerFilter, ILogger>(NoFilter.Instance, logger));
        }

        /// <summary>
        ///     Add a logger and limit which classes that can log to it.
        /// </summary>
        /// <param name="logger">Logger to use.</param>
        /// <param name="filter">Filter that logging types must pass.</param>
        public void Add(ILogger logger, ILoggerFilter filter)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            if (filter == null) throw new ArgumentNullException("filter");

            _loggers.Add(new Tuple<ILoggerFilter, ILogger>(filter, logger));
        }
    }
}