using System;
using System.Data.Common;

namespace Griffin.Data
{
    /// <summary>
    ///     Expose <c>DbCommand</c> in the UoW.
    /// </summary>
    public static class UnitOfWorkExtensions
    {
        /// <summary>
        ///     Create a Dbcommand
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        /// <remarks>
        ///     <para>
        ///         The async methods in ADO.NET is only exposed in the ADO.NET base classes (i.e. <c>DbCommand</c>) and not in the
        ///         interfaces. We'll therefore
        ///         have to violate Liskovs Substitution Principle to be able to access them. You should however be fine if you
        ///         seperate data from business and
        ///         just do integration tests for your data layer.
        ///     </para>
        /// </remarks>
        public static DbCommand CreateDbCommand(this IAdoNetUnitOfWork unitOfWork)
        {
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");

            var cmd = unitOfWork.CreateCommand() as DbCommand;
            if (cmd == null)
                throw new NotSupportedException(cmd.GetType().FullName +
                                                " do not inherit DbCommand. You can therefore not cast it to DbCommand to be able to use the async methods.");

            return cmd;
        }
    }
}