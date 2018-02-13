using System;
using System.Data;
using System.Data.Common;
using System.Runtime.ExceptionServices;

namespace Griffin.Data
{
    /// <summary>
    /// Extensions for IDbCommand
    /// </summary>
    public static class CommandExtensions
    {
        /// <summary>
        ///     Add a parameter to a command
        /// </summary>
        /// <param name="command">Command to add parameter to</param>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value (will be changed to <c>DBNull.Value</c> automatically if it's <c>null</c>).</param>
        /// <returns>Created parameter</returns>
        /// <example>
        ///     <code>
        /// using (var command = connection.CreateCommand())
        /// {
        ///     cmd.CommandText = "SELECT avg(Age) FROM Users WHERE LastName Like @name";
        ///     cmd.AddParameter("name", "F%");
        ///     return (int)cmd.ExecuteScalar();
        /// }
        /// </code>
        /// </example>
        public static IDataParameter AddParameter(this IDbCommand command, string name, object value)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (name == null) throw new ArgumentNullException("name");

            var p = command.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            command.Parameters.Add(p);
            return p;
        }

#if NETSTANDARD1_6
        public class DataException : DbException
        {
            private string errorMessage;

            public DataException(string errorMessage):base(errorMessage)
            {
            }

            public DataException(string errorMessage, Exception inner):base(errorMessage, inner)
            {
            }
        }

#endif
        /// <summary>
        /// Creates a <c>DataException</c> using the command text and sql parameters.
        /// </summary>
        /// <param name="cmd">Command to build an exception from.</param>
        /// <param name="inner">Inner exception.</param>
        /// <returns>Created exception</returns>
        public static DataException CreateDataException(this IDbCommand cmd, Exception inner)
        {
            // Failed to generate a command. Throw original exception.
            if (string.IsNullOrEmpty(cmd.CommandText))
            {
                ExceptionDispatchInfo.Capture(inner).Throw();
            }

            var exceptionToAnalyze = inner;
            if (exceptionToAnalyze is AggregateException)
                exceptionToAnalyze = inner.InnerException;

            var includeSql = exceptionToAnalyze is DataException ||
                             exceptionToAnalyze is DbException ||
                             exceptionToAnalyze.GetType().Namespace.StartsWith("System.Data");

            if (exceptionToAnalyze.InnerException != null && !includeSql)
                includeSql = exceptionToAnalyze.InnerException is DataException ||
                             exceptionToAnalyze.InnerException is DbException ||
                             exceptionToAnalyze.InnerException.GetType().Namespace.StartsWith("System.Data");

            var pos = exceptionToAnalyze.Message.IndexOfAny(new[] { '\r', '\n' });
            var innerMsg = pos == -1 ? exceptionToAnalyze.Message : exceptionToAnalyze.Message.Substring(0, pos);
            if (includeSql)
            {
                var str = innerMsg + "\r\nQuery:" + cmd.CommandText;
                foreach (IDbDataParameter parameter in cmd.Parameters)
                {
                    str += "\r\n\t" + parameter.ParameterName + ": " + (parameter.Value ?? "NULL");
                }
                return new DataException(str, inner);
            }
            throw new DataException(innerMsg + " [see inner exception for details]", inner);
        }
    }
}
