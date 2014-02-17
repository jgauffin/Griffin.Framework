using System;

namespace Griffin
{
    /// <summary>
    ///     Produce GUIDs with are used internally in this library
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Can be used to use specialized guids as COMBs to increase performance of data source lookups.
    ///     </para>
    ///     <para>The default implementation uses <c>Guid.NewGuid()</c>.</para>
    /// </remarks>
    public class GuidFactory
    {
        private static GuidFactory _instance = new GuidFactory();

        /// <summary>
        ///     Assign a new factory
        /// </summary>
        /// <param name="factory">Factory to use</param>
        public static void Assign(GuidFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            _instance = factory;
        }

        /// <summary>
        ///     Generate a new GUID
        /// </summary>
        /// <returns>Guid of some sort.</returns>
        public static Guid Create()
        {
            return _instance.CreateInternal();
        }

        /// <summary>
        ///     Generate a new GUID using the assign implementation
        /// </summary>
        /// <returns>Guid of some sort</returns>
        protected virtual Guid CreateInternal()
        {
            return Guid.NewGuid();
        }
    }
}