Executing scoped services from SingleInstance services
===============

It can be a hassle to invoke scoped services from single instance services when
using an IoC container. You typically have to get a reference to 
the container to be able to create a scope and then having to resolve
the correct service (i.e. Service Location).

This small class allows you to abstract that away.

# Setup

Start by registering `ScopedTaskInvoker` in your container.

## Autofac example

```csharp
var builder = new ContainerBuilder();

builder.RegisterType<ScopedTaskInvoker>()
       .AsImplementedInterfaces()
       .SingleInstance();

// required so that ScopedTaskInvoker
// can use your container.
builder.Register(x => new AutofacAdapter(x))
       .AsImplementedInterfaces();
```

# Usage

Simply take the `IScopedTaskInvoker` interface as a dependency 
in your single instance service.

## Example

```csharp
public class CleanDatabaseThread : ApplicationThread
{
    IScopedTaskInvoker _invoker;

    public CleanDatabaseThread(IScopedTaskInvoker invoker)
    {
        if (invoker == null) throw new ArgumentNullException("invoker");

        _invoker = invoker;
    }

    protected void Run(WaitHandle shutdownHandle)
    {
        //simplistic example
        //unhandled exceptions are caught by Griffin.Framework.
        // application services are automatically restarted 

        _invoker.Execute<ICleanDb>(x => x.DeleteOldMessages());
    }
}
```

# Commiting unit of work

If your scoped tasks typically uses an Unit of Work you might
want to commit them once the task have been completed successfully.

That can be achieved by subscribing on an event.

```csharp
public class Program
{
    private static AutofacAdapter _adapter;

    public static void Main(string[] argv)
    {
        var builder = new ContainerBuilder();
        builder.Register(CreateUnitOfWork)
               .AsImplementedInterfaces()
               .SingleInstance();

        // required so that ScopedTaskInvoker
        // can use your container.
        builder.Register(x => new AutofacAdapter(x))
               .AsImplementedInterfaces();

        _adapter = new AutofacAdapter(builder.Build());
    }

    public static object CreateUnitOfWork()
    {
        var task = new ScopedTaskInvoker(adapter);
        task.TaskExecuted += OnClosingScope;
        return task;
    }

    public static void OnClosingScope(object source, ScopedTaskEventArgs e)
    {
        e.Scope.Resolve<IUnitOfWork>().SaveChanges();
    }
}
```
