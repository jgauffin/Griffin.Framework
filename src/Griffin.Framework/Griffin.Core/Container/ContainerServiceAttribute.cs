using System;
using System.Collections;

namespace Griffin.Container
{
    /// <summary>
    ///     Used to simplify registration of services in an inversion of control container.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The purpose of this attribute is to allow services to be registered in containers without having to specify a <c>Register()</c> line for every class that should exist in the service. You
    /// can instead tag every class with this attribute and then let the container scan all assemblies after classes that have this attribute. We have built support for that in every container adaper that we've written. The extension methods
    /// are called <c>RegisterServices()</c>.
    ///     </para>
    ///     <para>
    ///         The methods that does registrations with the help of this attribute should register the class as all implemented interfaces except those that exist in the core .NET framework. If the class do not implement
    /// an interface it should be registered as itself.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <para>Example class that should be registered:</para>
    ///     <code>
    /// [ContainerService]
    /// public class UserRepository : IRepository
    /// {
    ///     IDbConnection _connection;
    /// 
    ///     public UserRepository(IDbConnection connection)
    ///     {
    ///         if (connection == null) throw new ArgumentNullException("connection");
    /// 
    ///         _connection = connection;
    ///     }
    /// }
    /// </code>
    /// <para>
    /// If we use the autofac package <c>Griffin.Framework.Autofac</c> we can register it as:
    /// <code>
    /// var builder = new ContainerBuilder();
    /// 
    /// // find all classes in the specified assembly
    /// builder.RegisterServices(Assembly.GetExecutingAssembly());
    /// 
    /// var container = builder.Build();
    /// </code>
    /// </para>
    /// </example>
    [AttributeUsage(AttributeTargets.Class)]
    public class ContainerServiceAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ContainerServiceAttribute" /> class.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Lifetime is per default <c>ContainerLifetime.Scoped</c>.
        /// </para>
        /// </remarks>
        public ContainerServiceAttribute()
        {
            Lifetime = ContainerLifetime.Scoped;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ContainerServiceAttribute" /> class.
        /// </summary>
        /// <param name="lifetime">how long the object should live in the container..</param>
        public ContainerServiceAttribute(ContainerLifetime lifetime)
        {
            Lifetime = lifetime;
        }

        /// <summary>
        ///     Gets specified lifetime
        /// </summary>
        /// <value>
        /// Lifetime is per default <c>ContainerLifetime.Scoped</c> unless otherwise specified in the constructor.
        /// </value>
        public ContainerLifetime Lifetime { get; private set; }
    }
}