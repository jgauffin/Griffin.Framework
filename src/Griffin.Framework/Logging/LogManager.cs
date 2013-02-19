/*
 * Copyright (c) 2011, Jonas Gauffin. All rights reserved.
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston,
 * MA 02110-1301 USA
 */

using System;

namespace Griffin.Framework.Logging
{
    /// <summary>
    /// Logging framework fascade.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A custom logger is configured by default writing all log entries to void. 
    /// </para>
    /// <para>
    /// Takes care of resolving which loggers a requesting class should get. A <see cref="ILogger"/> implementation
    /// can either log to one targets or multiple ones depending on the configuration.
    /// </para>
    /// <para>
    /// This is a singleton facade. The motivation to use the pattern is that it's not always feasible to use
    /// an inversion of control container to build loggers. And an IoC container might not be used for all projects
    /// that use the logging framework. Using a singleton facade instead of a pure singleton makes it easier to
    /// create custom implementations without exposing them to the user.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// public class YourClass
    /// {
    ///     private ILogger _logger = LogManager.GetLogger<YourClass>();
    /// 
    ///     public void Start()
    ///     {
    ///         _logger.Info("Hello world!");
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    public class LogManager
    {
        private static ILogManager _logManager = new NullLogManager();

        /// <summary>
        /// Assigns the specified log manager.
        /// </summary>
        /// <param name="logManager">The log manager.</param>
        /// <remarks>
        /// Assigns a log manager which will be used to generate loggers for
        /// each class that requests one.
        /// </remarks>
        public static void Assign(ILogManager logManager)
        {
            if (logManager == null) throw new ArgumentNullException("logManager");

            _logManager = logManager;
        }

        /// <summary>
        /// Get logger for the specified type
        /// </summary>
        /// <param name="type">Type to get logger for</param>
        /// <returns>A logger implementation.</returns>
        public static ILogger GetLogger(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            return _logManager.GetLogger(type);
        }

        /// <summary>
        /// Get logger for the specified type
        /// </summary>
        /// <returns>A logger implementation.</returns>
        public static ILogger GetLogger<T>()
        {
            return _logManager.GetLogger(typeof(T));
        }
    }
}