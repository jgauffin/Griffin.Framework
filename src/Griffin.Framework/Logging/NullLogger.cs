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
    internal class NullLogger : ILogger
    {
        #region ILogger Members

        public void Debug(string message)
        {
        }

        public void Debug(string message, params object[] formatters)
        {
        }

        public void Debug(string message, Exception exception)
        {
        }

        public void Debug(string message, Exception exception, params object[] formatters)
        {
        }

        public void Info(string message)
        {
        }

        public void Info(string message, params object[] formatters)
        {
        }

        public void Info(string message, Exception exception)
        {
        }

        public void Info(string message, Exception exception, params object[] formatters)
        {
        }

        public void Warning(string message)
        {
        }

        public void Warning(string message, params object[] formatters)
        {
        }

        public void Warning(string message, Exception exception)
        {
        }

        public void Warning(string message, Exception exception, params object[] formatters)
        {
        }

        public void Error(string message)
        {
        }

        public void Error(string message, params object[] formatters)
        {
        }

        public void Error(string message, Exception exception)
        {
        }

        public void Error(string message, Exception exception, params object[] formatters)
        {
        }

        #endregion
    }
}