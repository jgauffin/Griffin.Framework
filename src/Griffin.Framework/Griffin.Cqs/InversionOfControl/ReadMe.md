# InversionOfControl implementation

To use these classes you need to have a adapter between Griffin.Framework and your favorite container. Read more in the [Griffin.Container](../../Griffin.Core/Container/) namespace.

There are for instance the [Griffin.Framework.autofac](../../Griffin.Core.Autofac/) nuget package which you can use.


## Description

There are two types of bus implementations for commands / events. 

One that will store the messages on disk and execute the at a later point on a new thread. It allows the invoker to continue with it's processing
and also means that the command/event will not be lost even if the application crashes. Those bus implementations are [QueuedIocCommandBus](QueuedIocCommandBus.cs)
and [QueuedIocEventBus](QueuedIocEventBus.cs).

The other implementations will execute handlers asynchronously on a new IoC container child scope, but will not return untill the handlers have successfully been invoked.

## Setup

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

However, if you would like to call `transaction.Commit()` or similar you have to subscribe on an event for every bus. Here is a sample using Griffin.Container:

```csharp
private void CreateContainer()
{
    var builder = new ContainerRegistrar();
    builder.RegisterService<ICommandBus>(CreateCommandBus, Lifetime.Singleton);
    builder.RegisterService<IEventBus>(CreateEventBus, Lifetime.Singleton);
    builder.RegisterService<IQueryBus>(CreateQueryBus, Lifetime.Singleton);
    builder.RegisterService<IRequestReplyBus>(CreateRequestReplyBus, Lifetime.Singleton);


    _container = builder.Build();
}

private IRequestReplyBus CreateRequestReplyBus(IServiceLocator arg)
{
    var bus = new IocRequestReplyBus(new GriffinContainerAdapter(_container));
    
    //commit transaction for Request/replies
    bus.RequestInvoked += (sender, args) => args.Scope.Resolve<IAdoNetUnitOfWork>().SaveChanges();
    return bus;
}

private IQueryBus CreateQueryBus(IServiceLocator arg)
{
    var bus = new IocQueryBus(new GriffinContainerAdapter(_container));
    
    //commit for queries
    bus.QueryExecuted += (sender, args) => args.Scope.Resolve<IAdoNetUnitOfWork>().SaveChanges();
    return bus;
}

private IEventBus CreateEventBus(IServiceLocator arg)
{
    var bus = new IocEventBus(new GriffinContainerAdapter(_container));
    
    //commit for events (done even if one or more handlers fails)
    // you can check the Successful property to only commit if all succeed.
    bus.EventPublished += (sender, args) => args.Scope.Resolve<IAdoNetUnitOfWork>().SaveChanges();
    
    // report all failures including the event and handler to http://OneTrueError.com
    bus.HandlerFailed += (sender, args) =>
        {
            foreach (var failure in args.Failures)
            {
                OneTrue.Report(failure.Exception, new {Event = args.ApplicationEvent, Handler = failure.Handler});
            }
        };
                    
    return bus;
}

private ICommandBus CreateCommandBus(IServiceLocator arg)
{
    var bus = new IocCommandBus(new GriffinContainerAdapter(_container));
    
    //commit for commands
    bus.CommandInvoked += (sender, args) => args.Scope.Resolve<IAdoNetUnitOfWork>().SaveChanges();
    return bus;
}

```

## Handlers

A handler looks like any other class and can take dependencies in the constructor. 

This example uses the [Data mapper](../../Griffin.Core/Data/Mapper) in Griffin Framework to store the item.

```csharp
public class AccountActivatedHandler : IApplicationEventSubscriber<AccountActivated>
{
    private readonly IAdoNetUnitOfWork _unitOfWork;

    public AccountActivatedHandler(IAdoNetUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(AccountActivated e)
    {
        var item = new EmailHistoryItem(RemindOrganizationsWithNoApplications.EmailTypeName, e.OrganizationId)
        {
            AccountId = e.AccountId,
            SentAtUtc = DateTime.Now.AddDays(1)
        };
        await _unitOfWork.InsertAsync(item);
    }
}
```
