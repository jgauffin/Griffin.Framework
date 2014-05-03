using System;
using System.Collections.Generic;

namespace Griffin.Container
{
    /// <summary>
    /// 
    /// </summary>
    public interface IContainerScope : IDisposable
    {
        TService Resolve<TService>();
        object Resolve(Type service);
        IEnumerable<TService> ResolveAll<TService>();
        IEnumerable<object> ResolveAll(Type service);
    }
}