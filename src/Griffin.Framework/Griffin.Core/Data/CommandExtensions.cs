using System;
using System.Data;

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

        /// <summary>
        /// Creates a <c>DataException</c> using the command text and sql parameters.
        /// </summary>
        /// <param name="cmd">Command to build an exception from.</param>
        /// <param name="inner">Inner exception.</param>
        /// <returns>Created exception</returns>
        public static DataException CreateDataException(this IDbCommand cmd, Exception inner)
        {
            var str = "Failed to execute sql.\r\nQuery:" + cmd.CommandText;
            foreach (IDbDataParameter parameter in cmd.Parameters)
            {
                str += "\r\n\t" + parameter.ParameterName + ": " + (parameter.Value ?? "NULL");
            }
            return new DataException(str, inner);
        }

    }
}
