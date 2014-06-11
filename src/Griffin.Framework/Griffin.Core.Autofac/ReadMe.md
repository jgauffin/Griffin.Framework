Autofac support for Griffin.Framework
==========

This library enables Griffin.Framework to use Autofac for its features which require service location (to enable you to use dependency injection).

# Griffin.Cqs

Griffin.Cqs is a library which enables you to use the Command/Query pattern over application boundries (or within the same application).

To register your commands queries you can do the following:

```csharp
var cb = new ContainerBuilder();
cb.RegisterCqsHandlers(Assembly.GetExecutingAssembly());
cb.RegisterType<ContainerQueryBus>().As<IQueryBus>().SingleInstance();
cb.RegisterType<ContainerCommandBus>().As<ICommandBus>().SingleInstance();
cb.RegisterType<ContainerEventBus>().As<IApplicationEventBus>().SingleInstance();
cb.RegisterType<ContainerRequestReplyBus>().As<IRequestReplyBus>().SingleInstance();
cb.RegisterInstance(_griffinContainer).AsImplementedInterfaces();
// .. your other registrations ..

var container = cb.Build();

// store as a class member variable
_griffinContainer = new AutofacContainer(container);

```