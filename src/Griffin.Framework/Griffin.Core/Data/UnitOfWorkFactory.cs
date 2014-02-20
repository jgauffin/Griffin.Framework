using System;

namespace Griffin.Data
{
    /// <summary>
    ///     Can be used to create unit of work instances in your application.
    /// </summary>
    /// <example>
    ///     <para>Assignment:</para>
    ///     <code>
    /// public IUnitOfWork Create()
    /// {
    ///     var conString = ConfigurationManager.ConnectionStrings("MyDb").ConnectionString;
    ///     var con = new SqlConnection(conString);
    ///     con.Open();
    ///     return new AdoNetUnitOfWork(con, true);
    /// }
    /// 
    /// UnitOfWorkFactory.SetFactoryMethod(() => Create());
    /// </code>
    ///     <para>Usage:</para>
    ///     <code>
    /// using (var uow = UnitOfWorkFactory.Create())
    /// {
    ///     var repos = new UserRepository(uow);
    ///     repos.Create("Jonas");
    /// 
    ///     uow.SaveChanges();
    /// }
    /// </code>
    /// </example>
    public class UnitOfWorkFactory
    {
        private static Func<IUnitOfWork> _factoryMethod;

        /// <summary>
        /// Create a new unit of work.
        /// </summary>
        /// <returns>Created UOW</returns>
        public static IUnitOfWork Create()
        {
            if (_factoryMethod == null)
                throw new InvalidOperationException("You must call SetFactoryMethod() first.");

            return _factoryMethod();
        }

        /// <summary>
        ///     Assign a method which will be used to create an unit of work every time <c>Create()</c> is being called.
        /// </summary>
        /// <param name="factoryMethod">Should return a fully functional UoW.</param>
        public static void SetFactoryMethod(Func<IUnitOfWork> factoryMethod)
        {
            if (factoryMethod == null) throw new ArgumentNullException("factoryMethod");
            _factoryMethod = factoryMethod;
        }
    }
}