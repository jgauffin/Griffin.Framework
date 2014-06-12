# InversionOfControl implementation

To use these classes you need to have a adapter between Griffin.Framework and your favorite container. Read more in the [Griffin.Container](../../Griffin.Core/Container/) namespace.

There are for instance the [Griffin.Framework.autofac](../../Griffin.Core.Autofac/) nuget package which you can use.


## Description

There are two types of bus implementations for commands / events. 

One that will store the messages on disk and execute the at a later point on a new thread. It allows the invoker to continue with it's processing
and also means that the command/event will not be lost even if the application crashes. Those bus implementations are [QueuedIocCommandBus](QueuedIocCommandBus.cs)
and [QueuedIocEventBus](QueuedIocEventBus.cs).

The other implementations will execute handlers asynchronously on a new IoC container child scope, but will not return untill the handlers have successfully been invoked.

## Usage

The usage depends on the inversion of control container. You can for instance look at the [Autofac integration package](../../Griffin.Core.Autofac/).

What might not look straight forward is that the bus implementations requires a container to be able to work. But how can they access a container
when they are being registered in the container being built?

It's possible thanks to lazy loading. You can do something like this for Unity:

```csharp
public class CompositionRoot
{
	private static UnityContainerWrapper _container;

	public static void Register()
	{
		var container = new UnityContainer()

		container.RegisterType<IQueryBus, ContainerQueryBus>(new ContainerControlledLifetimeManager());
		container.RegisterType<ICommandBus, ContainerCommandBus>(new ContainerControlledLifetimeManager());
		container.RegisterType<IApplicationEventBus, ContainerEventBus>(new ContainerControlledLifetimeManager());
		container.RegisterType<IRequestReplyBus, ContainerRequestReplyBus>(new ContainerControlledLifetimeManager());

		// Lazy load the container.
		container.RegisterType<IContainer>(new InjectionFactory(c => _container));

		// [.. your other registrations ..]

		_container = new UnityContainerWrapper(container);
	}
}
```




