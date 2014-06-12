Unity support for Griffin.Framework
==========

This library enables Griffin.Framework to use Unity for its features which require service location (to enable you to use dependency injection).

Install it using nuget:

    install-package griffin.framework.unity


# Registrations

This library includes a easier way to manage which classes should be registered in the container. It's done with the help of the `[ContainerService]` attribute.

To get a class automatically registered in the container simply tag it with the attribute:

```csharp
[ContainerService]
public class UserRepository : IUserRepository
{
    // [...]
}
```

and then register it:

```csharp
public class CompositionRoot
{
	private static UnityContainerAdapter _container;

	public static void Register()
	{
		var container = new UnityContainer();

		// register all classes with the [ContainerService] attribute from the current assembly.
		container.RegisterServices(Assembly.GetExecutingAssembly());

		// [.. your other registrations ..]

		_container = new UnityContainerAdapter(container);
	}
}
```

You can also specify the lifetime:

```csharp
[ContainerService(Lifetime.SingleInstance)]
public class MessageCache
{
}
```

The registration will register the class as all implemented interfaces (exception core .NET interfaces).

