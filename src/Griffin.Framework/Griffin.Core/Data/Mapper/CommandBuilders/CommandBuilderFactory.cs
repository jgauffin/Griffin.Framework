using System;

namespace Griffin.Data.Mapper.CommandBuilders
{
    /// <summary>
    ///     Used to produce factories of the correct
    /// </summary>
    public class CommandBuilderFactory
    {
        private static Func<IEntityMapper, ICommandBuilder> _commandBuilder =
            mapper => new SqlServerCommandBuilder(mapper);

        public static void Assign(Func<IEntityMapper, ICommandBuilder> commandBuilder)
        {
            if (commandBuilder == null) throw new ArgumentNullException("commandBuilder");
            _commandBuilder = commandBuilder;
        }

        public static ICommandBuilder Create(IEntityMapper mapper)
        {
            return _commandBuilder(mapper);
        }
    }
}