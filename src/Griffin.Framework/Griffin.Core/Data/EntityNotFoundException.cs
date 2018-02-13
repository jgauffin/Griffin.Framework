using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.Serialization;

namespace Griffin.Data
{
    /// <summary>
    ///     Thrown when a method which expects to find an entity doesn't.
    /// </summary>
    /// <remarks>
    /// <para>this error message will always include information to be able to identify the missing entity.</para>
    /// </remarks>
    public class EntityNotFoundException : DbException
    {
        private readonly string _commandText;
        private readonly string _parameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="command">The command that was executed to find an entity.</param>
        /// <exception cref="System.ArgumentNullException">command</exception>
        public EntityNotFoundException(string message, IDbCommand command)
            : base(message)
        {
            if (command == null) throw new ArgumentNullException("command");

            _commandText = command.CommandText;
            _parameters = string.Join(", ",
                command.Parameters.Cast<IDataParameter>()
                    .Select(x => x.ParameterName + "=" + x.Value));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <param name="command">The command.</param>
        /// <param name="inner">The inner.</param>
        /// <exception cref="System.ArgumentNullException">
        /// description
        /// or
        /// inner
        /// </exception>
        public EntityNotFoundException(string description, IDbCommand command, Exception inner)
            : base(description, inner)
        {
            if (description == null) throw new ArgumentNullException("description");
            if (inner == null) throw new ArgumentNullException("inner");

            _commandText = command.CommandText;
            _parameters = string.Join(", ",
                command.Parameters.Cast<IDataParameter>()
                    .Select(x => x.ParameterName + "=" + x.Value));
        }
        
        /// <summary>
        ///     Gets a message that describes the current exception.
        /// </summary>
        /// <returns>
        ///     The error message that explains the reason for the exception, or an empty string("").
        /// </returns>
        public override string Message
        {
            get
            {
                return string.Format("{0}\r\nCommand: {1}\r\nParameters: {2}", base.Message, _commandText, _parameters);
            }
        }

        /// <summary>
        /// Gets command that was executed
        /// </summary>
        public string CommandText { get { return _commandText; }}


        /// <summary>
        /// The command parameter collection joined as a string
        /// </summary>
        public string CommandParameters { get { return _parameters; } }
        
    }
}