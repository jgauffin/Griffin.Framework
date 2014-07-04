using System.Threading.Tasks;

namespace Griffin.ApplicationServices
{
    /// <summary>
    ///     A background job which requires a lifetime scope (to access database connection/transaction etc).
    /// </summary>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// [ContainerService]
    /// public class CleanUpOldFriendRequests : IBackgroundJobAsync
    /// {
    ///     private readonly IUnitOfWork _uow;
    ///     private static DateTime _lastExecutionTime;
    /// 
    ///     public CleanUpOldFriendRequests(IUnitOfWork uow)
    ///     {
    ///         if (uow == null) throw new ArgumentNullException("uow");
    /// 
    ///         _uow = uow;
    ///     }
    /// 
    ///     public async Task ExecuteAsync()
    ///     {
    ///         //run once a day
    ///         if (_lastExecutionTime.Date >= DateTime.Today)
    ///             return;
    ///         _lastExecutionTime = DateTime.Today;
    /// 
    ///         using (var cmd = _uow.CreateCommand())
    ///         {
    ///             cmd.CommandText = "DELETE FROM FriendRequests WHERE CreatedAtUtc < @datum";
    ///             cmd.AddParameter("datum", DateTime.Today.AddDays(-10));
    ///             await cmd.ExecuteNonQueryAsync();
    ///         }
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    public interface IBackgroundJobAsync
    {
        /// <summary>
        ///     Execute job
        /// </summary>
        /// <remarks>
        ///     Any unhandled exceptions will be caught by the <see cref="BackgroundJobManager"/>.
        /// </remarks>
        /// <returns>Completion task</returns>
        Task ExecuteAsync();
    }
}