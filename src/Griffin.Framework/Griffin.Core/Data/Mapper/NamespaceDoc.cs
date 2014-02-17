using System.Runtime.CompilerServices;

namespace Griffin.Data.Mapper
{
    /// <summary>
    ///     Micro data layer making it easier to work with SQL queries.
    /// </summary>
    /// <remarks>
    ///     <para></para>
    ///     The command extension uses mapper classes
    ///     to map the database recordset to your entity classes. Hence you need to start by creating mappings as shown below.
    ///     <para>
    ///         The mappings are retrieve by using the Assem
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <para>
    ///         First you have to define one mapping file per entity that you want to fetch. The following mapping
    ///         works when the columns are named same as the class properties and they are of the same type:
    ///     </para>
    ///     <code>
    /// <![CDATA[
    /// public class UserMapping : ReflectionBasedEntityMapper<User>
    /// {
    /// }
    /// ]]>
    /// </code>
    ///     <para>
    ///         You can however customize it to specify a different column name or use an adapter for the column value. Read
    ///         more in the <see cref="ReflectionEntityMapper{TEntity}" /> documentation.
    ///     </para>
    /// </example>
    [CompilerGenerated]
    internal class NamespaceDoc
    {
    }
}