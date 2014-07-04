using System;

namespace Griffin.Data.Mapper.CommandBuilders
{
    /// <summary>
    /// Used to produce factories of the correct 
    /// </summary>
    public class CommandBuilderFactory
    {
        private static Func<IEntityMapper, ICommandBuilder> _commandBuilder = mapper => new SqlServerCommandBuilder(mapper);

        /// <summary>
        /// Assigns the specified command builder.
        /// </summary>
        /// <param name="commandBuilder">Assign your database engine specific command builder factory.</param>
        /// <exception cref="System.ArgumentNullException">commandBuilder</exception>
        public static void Assign(Func<IEntityMapper, ICommandBuilder> commandBuilder)
        {
            if (commandBuilder == null) throw new ArgumentNullException("commandBuilder");
            _commandBuilder = commandBuilder;
        }

        /// <summary>
        /// Create a command builder that uses your DB engine dialect.
        /// </summary>
        /// <param name="mapper">Mapper to get a builder for.</param>
        /// <returns>builder.</returns>
        public static ICommandBuilder Create(IEntityMapper mapper)
        {
            return _commandBuilder(mapper);
        }
    }

}
