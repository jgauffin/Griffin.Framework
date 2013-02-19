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
    /// Responsible of creating loggers for all types that requests one.
    /// </summary>
    /// <seealso cref="LogManager"/>.
    /// <remarks>
    /// The purpose of this class is to create a fascade between the logger creation 
    /// and the classes that requests a logger. It's up to each implementation to decide
    /// if the same logger should be used for each class or if more complex filters and 
    /// loggers are used.
    /// </remarks>
    public interface ILogManager
    {
        /// <summary>
        /// Get a logger for the specified type
        /// </summary>
        /// <param name="type">Type that requests a logger</param>
        /// <returns>A logger (always)</returns>
        /// <remarks>
        /// A logger should <c>always</c> be returned by this method. Simply use a empty
        /// logger if none can be found.
        /// </remarks>
        ILogger GetLogger(Type type);
    }
}