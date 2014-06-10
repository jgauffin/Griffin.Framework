Autofac support for Griffin.Framework
==========

This library enables Griffin.Framework to use Autofac for its features which require service location (to enable you to use dependency injection).

# Griffin.Cqs

Griffin.Cqs is a library which enables you to use the Command/Query pattern over application boundries (or within the same application).

To register your commands queries you can do the following:

```csharp
var cb = new ContainerBuilder();
cb.RegisterCqsHandlers(Assembly.GetExecutingAssembly());
cb.Register(x => new ContainerQueryBus(_griffinContainer)).As<IQueryBus>().SingleInstance();
cb.Register(x => new ContainerCommandBus(_griffinContainer)).As<ICommandBus>().SingleInstance();
cb.Register(x => new ContainerEventBus(_griffinContainer)).As<IApplicationEventBus>().SingleInstance();
cb.Register(x => new ContainerRequestReplyBus(_griffinContainer)).As<IRequestReplyBus>().SingleInstance();
cb.RegisterType<IQueryBus>(x => new ContainerQueryBus(_griffinContainer));
// .. your other registrations ..

var container = cb.Build();
_griffinContainer = new AutofacContainer(container);

```