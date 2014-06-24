namespace Griffin.ApplicationServices
{
    /// <summary>
    ///     A background job which requires a lifetime scope (to access database connection/transaction etc).
    /// </summary>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// [ContainerService]
    /// public class CleanUpOldFriendRequests : IBackgroundJob
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
    ///     public void Execute()
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
    ///             cmd.ExecuteNonQuery();
    ///         }
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    public interface IBackgroundJob
    {
        /// <summary>
        ///     Execute job
        /// </summary>
        /// <remarks>
        ///     Any unhandled exceptions will be caught by the <see cref="BackgroundJobManager"/>.
        /// </remarks>
        void Execute();
    }
}