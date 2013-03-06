using System;

namespace Griffin.Framework.Data
{
    /// <summary>
    /// Unit of work definition
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Add all changes
        /// </summary>
        void SaveChanges();
    }
}
