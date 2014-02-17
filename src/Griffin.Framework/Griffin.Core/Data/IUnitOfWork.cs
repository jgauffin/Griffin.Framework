using System;

namespace Griffin.Data
{
    /// <summary>
    ///     Unit of work contract
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Disposing without <c>SaveChanges()</c> being called means that the transaction should be aborted.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// using (var uow = UnitOfWorkFactory.Create())
    /// {
    ///     var repos = new UserRepository(uow);
    ///     repos.Create(user);
    /// 
    ///     uow.SaveChanges();
    /// }
    /// </code>
    /// </example>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        ///     Commit changes.
        /// </summary>
        /// <exception cref="TransactionAlreadyClosedException">UoW have already been saved.</exception>
        void SaveChanges();
    }
}