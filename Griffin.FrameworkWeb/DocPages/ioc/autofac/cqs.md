Griffin.Cqs adapter
===================

Griffin.Cqs is a library which implements the contracts defined in the 
nuget package `DotNetCqs`. `DotNetCqs` allows you to use use the Command/Query
pattern without coupling your code to a specific implementation.

This namespace enables you to easily register all your CQS handlers in autofac 
as shown in the example below.

```csharp
public class CompositionRoot
{
	private static AutofacAdapter _container;

	public static void Register()
	{
		var cb = new ContainerBuilder();

		// register all handlers
		cb.RegisterCqsHandlers(Assembly.GetExecutingAssembly());

		// register all different buses
		cb.RegisterType<ContainerQueryBus>().As<IQueryBus>().SingleInstance();
		cb.RegisterType<ContainerCommandBus>().As<ICommandBus>().SingleInstance();
		cb.RegisterType<ContainerEventBus>().As<IApplicationEventBus>().SingleInstance();
		cb.RegisterType<ContainerRequestReplyBus>().As<IRequestReplyBus>().SingleInstance();

		// Lazy load the container.
		cb.Register(x => _container).AsImplementedInterfaces().SingleInstance();

		// [.. your other registrations ..]

		var container = cb.Build();
		_container = new AutofacAdapter(container);
	}
}
```