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
    /// Simple wrapper to be able to provide logging objects even if no logging framework have been specified.
    /// </summary>
    internal class NullLogManager : ILogManager
    {
        private static readonly NullLogger Logger = new NullLogger();

        #region ILogManager Members

        /// <summary>
        /// Get a logger for the specified type
        /// </summary>
        /// <param name="type">Type that requests a logger</param>
        /// <returns>
        /// A logger (always)
        /// </returns>
        /// <remarks>
        /// A logger should <c>always</c> be returned by this method. Simply use a empty
        /// logger if none can be found.
        /// </remarks>
        public ILogger GetLogger(Type type)
        {
            return Logger;
        }


        #endregion
    }
}