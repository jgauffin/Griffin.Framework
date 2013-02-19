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
    /// Used to write log entries to one or more log targets.
    /// </summary>
    /// <remarks>
    /// <para>
    /// It's very important that none of these methods throw exceptions in implementations. 
    /// </para><para>
    /// Here is our recommendation to how you should use each log level.
    /// <list type="table">
    /// <item>
    /// <term>Debug</term>
    /// <description>Debug entries are usually used only when debugging. They can be used to track
    /// variables or method contracts. There might be several debug entries per method.</description>
    /// </item>
    /// <item>
    /// <term>Info</term>
    /// <description>Informational messages are used to track state changes such as login, logout, record updates etc. 
    /// There are at most one entry per method.</description>
    /// </item>
    /// <item>
    /// <term>Warning</term>
    /// <description>
    /// Warnings are used when something unexpected happend but the application can handle it and continue as expected.
    /// </description>
    /// </item>
    /// <item>
    /// <term>Error</term>
    /// <description>
    /// Errors are when something unexpected happens and the application cannot deliver result as expected. It might or might not
    /// mean that the application has to be restarted.
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// public void MyClass
    /// {
    ///     public ILogger _logger = LogManager.GetLogger<MyClass>();
    /// 
    ///     public string CalculateQuestion(int answer)
    ///     {
    ///         if (answer != 42)
    ///         {
    ///             _logger.Error("User {0} failed to give the correct answer, specified {1} instead", 
    ///                 Thread.CurrentPrincipal.Identity.Name,
    ///                 answer);
    /// 
    ///             throw new InvalidOperationException("Wrong answer");
    ///         }
    /// 
    ///         return "So you want to know the real question, ehhh?";
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    public interface ILogger
    {
        /// <summary>
        /// Write a diagnostic message.
        /// </summary>
        /// <param accountName="message">Log message.</param>
        /// <param accountName="formatters">Formatters used in the log message</param>
        /// <remarks>
        /// Log entries which is helpful during debugging. The amount of debug entries can be vast,
        /// and it's therefore not recommended to have them turned on in production systems unless 
        /// it's really required.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// public void MyClass
        /// {
        ///     public ILogger _logger = LogManager.GetLogger<MyClass>();
        /// 
        ///     public string CalculateQuestion(int answer)
        ///     {
        ///         _logger.Debug("Answered {0}.", answer);
        ///         return "So you want to know the real question, ehhh?";
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        void Debug(string message, params object[] formatters);

        /// <summary>
        /// Write a diagnostic message.
        /// </summary>
        /// <param accountName="message">Log message.</param>
        /// <param accountName="exception">Exception thrown by code. All inner exceptions will automatically be logged.</param>
        /// <remarks>
        /// Log entries which is helpful during debugging. The amount of debug entries can be vast,
        /// and it's therefore not recommended to have them turned on in production systems unless 
        /// it's really required.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// public void MyClass
        /// {
        ///     public ILogger _logger = LogManager.GetLogger<MyClass>();
        /// 
        ///     public string CalculateQuestion(int answer)
        ///     {
        ///         try
        ///         {
        ///             return LoadFromCache();
        ///         }
        ///         catch (Exception err)
        ///         {
        ///             _logger.Debug("Nothing in the cache..", err);
        ///         }
        /// 
        ///         //do complex calculcation here.
        /// 
        ///         return "Hello";
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        void Debug(string message, Exception exception);

        /// <summary>
        /// Write an error message.
        /// </summary>
        /// <param accountName="message">Log message.</param>
        /// <param accountName="formatters">Formatters used in the log message</param>
        /// <remarks>
        /// Use this method to log errors in your code that prevent the application from continuing as expected,
        /// but the error is not severe enough to shut down the system.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// public void MyClass
        /// {
        ///     public ILogger _logger = LogManager.GetLogger<MyClass>();
        /// 
        ///     public string CalculateQuestion(int answer)
        ///     {
        ///         if (answer != 42)
        ///         {
        ///             _logger.Error("User {0} failed to give the correct answer, specified {1} instead", 
        ///                 Thread.CurrentPrincipal.Identity.Name,
        ///                 answer);
        /// 
        ///             throw new InvalidOperationException("Wrong answer");
        ///         }
        /// 
        ///         return "So you want to know the real question, ehhh?";
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        void Error(string message, params object[] formatters);

        /// <summary>
        /// Write an informational message.
        /// </summary>
        /// <param accountName="message">Log message.</param>
        /// <param accountName="exception">Exception thrown by code. All inner exceptions will automatically be logged.</param>
        /// <remarks>
        /// Use this method to log errors in your code that prevent the application from continuing as expected,
        /// but the error is not severe enough to shut down the system.
        /// </remarks>
        void Info(string message, Exception exception);

        /// <summary>
        /// Write an informational message.
        /// </summary>
        /// <param accountName="message">Log message.</param>
        /// <param accountName="formatters">Formatters used in the log message</param>
        /// <remarks>
        /// Informational messages are more important than Debug messages and are usually state changes like
        /// a user logs on, sends an email etc. There are typically at most one <c>Info</c> message per method.
        /// </remarks>
        void Info(string message, params object[] formatters);

        /// <summary>
        /// Write an warning message.
        /// </summary>
        /// <param accountName="message">Log message.</param>
        /// <remarks>
        /// Warnings should be written when something did not go as planned, but your application can recover from it
        /// and continue as expected.
        /// </remarks>
        void Warning(string message);

        /// <summary>
        /// Write an warning message.
        /// </summary>
        /// <param accountName="message">Log message.</param>
        /// <param accountName="exception">Exception thrown by code. All inner exceptions will automatically be logged.</param>
        /// <remarks>
        /// Warnings should be written when something did not go as planned, but your application can recover from it
        /// and continue as expected.
        /// </remarks>
        void Warning(string message, Exception exception);
    }
}