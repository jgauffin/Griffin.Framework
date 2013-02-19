using System;

namespace Griffin.Framework.InversionOfControl
{
    /// <summary>
    /// Child container, holds all scoped services.
    /// </summary>
    /// <remarks>All scoped services are disposed when the child container is disposed.</remarks>
    public interface IChildContainer : IServiceLocator, IDisposable
    {
    }
}
