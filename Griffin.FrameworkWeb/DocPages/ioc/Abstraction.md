Abstraction
===============

Implement `IContainer` which wraps your main container.

## Example

```csharp
public class AutofacAdapter : IContainer
{
    private readonly global::Autofac.IContainer _container;

    public AutofacAdapter(global::Autofac.IContainer container)
    {
        if (container == null) throw new ArgumentNullException("container");
        _container = container;
    }

    public IContainerScope CreateScope()
    {
        return new AutofacScopeAdapter(_container.BeginLifetimeScope());
    }

    public TService Resolve<TService>()
    {
        try
        {
            return ResolutionExtensions.Resolve<TService>((IComponentContext) _container);
        }
        catch (ComponentNotRegisteredException ex)
        {
            throw new ServiceNotRegisteredException(typeof (TService), ex);
        }
        catch (DependencyResolutionException ex)
        {
            throw new DependencyMissingException(ex.Message, ex);
        }
    }

    public object Resolve(Type service)
    {
        if (service == null) throw new ArgumentNullException("service");
        try
        {
            return _container.Resolve(service);
        }
        catch (ComponentNotRegisteredException ex)
        {
            throw new ServiceNotRegisteredException(service, ex);
        }
        catch (DependencyResolutionException ex)
        {
            throw new DependencyMissingException(ex.Message, ex);
        }
    }

    public IEnumerable<TService> ResolveAll<TService>()
    {
        try
        {
            return _container.Resolve<IEnumerable<TService>>();
        }
        catch (ComponentNotRegisteredException ex)
        {
            throw new ServiceNotRegisteredException(typeof (TService), ex);
        }
        catch (DependencyResolutionException ex)
        {
            throw new DependencyMissingException(ex.Message, ex);
        }
    }

    public IEnumerable<object> ResolveAll(Type service)
    {
        if (service == null) throw new ArgumentNullException("service");
        try
        {
            var type = typeof (IEnumerable<>).MakeGenericType(service);
            return (IEnumerable<object>) _container.Resolve(type);
        }
        catch (ComponentNotRegisteredException ex)
        {
            throw new ServiceNotRegisteredException(service, ex);
        }
        catch (DependencyResolutionException ex)
        {
            throw new DependencyMissingException(ex.Message, ex);
        }
    }
}

```

# IContainerScope

IContainerScope represents a scoped life time container.

## Example

```csharp
public class AutofacScopeAdapter : IContainerScope
{
    private readonly ILifetimeScope _scope;

    public AutofacScopeAdapter(ILifetimeScope scope)
    {
        if (scope == null) throw new ArgumentNullException("scope");
        _scope = scope;
    }

    public void Dispose()
    {
        _scope.Dispose();
    }

    public TService Resolve<TService>()
    {
        try
        {
            return _scope.Resolve<TService>();
        }
        catch (ComponentNotRegisteredException ex)
        {
            throw new ServiceNotRegisteredException(typeof (TService), ex);
        }
        catch (DependencyResolutionException ex)
        {
            throw new DependencyMissingException(ex.Message, ex);
        }
    }

    public object Resolve(Type service)
    {
        try
        {
            return _scope.Resolve(service);
        }
        catch (ComponentNotRegisteredException ex)
        {
            throw new ServiceNotRegisteredException(service, ex);
        }
        catch (DependencyResolutionException ex)
        {
            throw new DependencyMissingException(ex.Message, ex);
        }
    }

    public IEnumerable<TService> ResolveAll<TService>()
    {
        try
        {
            return _scope.Resolve<IEnumerable<TService>>();
        }
        catch (ComponentNotRegisteredException ex)
        {
            throw new ServiceNotRegisteredException(typeof (TService), ex);
        }
        catch (DependencyResolutionException ex)
        {
            throw new DependencyMissingException(ex.Message, ex);
        }
    }

    public IEnumerable<object> ResolveAll(Type service)
    {
        try
        {
            var type = typeof(IEnumerable<>).MakeGenericType(service);
            return (IEnumerable<object>)_scope.Resolve(type);
        }
        catch (ComponentNotRegisteredException ex)
        {
            throw new ServiceNotRegisteredException(service, ex);
        }
        catch (DependencyResolutionException ex)
        {
            throw new DependencyMissingException(ex.Message, ex);
        }
    }
}
```

Look at the Autofac project in our github for a reference.
