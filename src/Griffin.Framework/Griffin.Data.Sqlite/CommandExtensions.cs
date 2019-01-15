using System;
using System.Data;
using Griffin.Data.Mapper;

namespace Griffin.Data.Sqlite
{
    /// <summary>
    ///     SQLite specific extensions
    /// </summary>
    public static class CommandExtensions
    {
        /// <summary>
        ///     Paging of a command
        /// </summary>
        /// <typeparam name="TEntity">Database entity</typeparam>
        /// <param name="cmd">Command to apply paging to</param>
        /// <param name="pageNumber">One based index.</param>
        /// <param name="itemsPerPage">Number of items in each page.</param>
        public static void Paging<TEntity>(this IDbCommand cmd, int pageNumber, int itemsPerPage)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");
            if (pageNumber <= 0) throw new ArgumentOutOfRangeException("pageNumber");
            if (itemsPerPage <= 0) throw new ArgumentOutOfRangeException("itemsPerPage");

            var mapper = EntityMappingProvider.GetCrudMapper<TEntity>();
            var db = new SqliteCommandBuilder(mapper);
            db.Paging(cmd, pageNumber, itemsPerPage);
        }

        /// <summary>
        ///     Paging of a command
        /// </summary>
        /// <param name="cmd">Command to apply paging to</param>
        /// <param name="pageNumber">One based index.</param>
        /// <param name="itemsPerPage">Number of items in each page.</param>
        public static void Paging(this IDbCommand cmd, int pageNumber, int itemsPerPage)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");
            if (pageNumber <= 0) throw new ArgumentOutOfRangeException("pageNumber");
            if (itemsPerPage <= 0) throw new ArgumentOutOfRangeException("itemsPerPage");

            var offset = (pageNumber - 1) * itemsPerPage;
            cmd.CommandText += string.Format(" LIMIT {1} OFFSET {0}",
                offset, itemsPerPage);
        }
    }
}