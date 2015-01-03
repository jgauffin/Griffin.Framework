# Application services

When you are running windows services or similar long running applications you 
typically have classes that need to run
as long as the application. We call them application services. 

The default implementation requires that the services are registered in your container as
SingleInstance services. If you need to invoke scoped services from them
you can use the [ScopedTaskInvoker](../IoC/ScopedTasks).

## Controlling services.

The classes are controlled by the `ApplicationServiceManager`. It can both
use your favorite IoC container (through the adapter contracts) or by using assembly scanning.

Using assembly scanning (searches for `ApplicationService` implementations)

```csharp
// find your application services
var scanner = new AssemblyScanner();
scanner.Scan(Assembly.GetExecutingAssembly());

// run the services
_serviceManager = new ApplicationServiceManager(scanner);
_serviceManager.Start();
```

Using autofac (requires the nuget package *griffin.framework.autofac*):

```csharp
// autofac
var builder = new ContainerBuilder();
builder.Register<YourServiceClass>()
       .AsImplementedInterfaces()
       .SingleInstance();
var autofac = builder.Build();

// griffin library
var adapter = new AutofacAdapter(autofac);
_serviceManager = new ApplicationServiceManager(adapter);
_serviceManager.Start();


// .. when the application is shut down..
_serviceManager.Stop();
```

By using a container you can also use dependency injection in your services.

### Detecting failures

The ApplicationServiceManager will restart service which have failed (as long as they implement `IGuardedService`). But you
probably want to be able to log all failures.
 This can be done with the help of the `ServiceStartFailed` and `ServiceFailed` events:

```csharp
public class Program
{
    public static void Main(string[] argv)
    {
        // [.. other initializations ..]]
        _appServiceManager.ServiceStartFailed += OnServiceCouldNotBeStarted;
        _appServiceManager.ServiceFailed += OnServiceFailed;

    }


   public static void OnServiceFailed(object sender, ApplicationServiceFailedEventArgs e)
   {
       _logger.Error("Service " + e.ApplicationService.GetType().Name + " failed", e.Exception);
   }

   public static void OnServiceCouldNotBeStarted(object sender, ApplicationServiceFailedEventArgs e)
   {
       _logger.Error("Service " + e.ApplicationService.GetType().Name + " failed", e.Exception);
   }

}
```

### Starting/stopping services.

The services can for instance be started/stopped using the application
config. To activate a service you just set the appSetting `<add key="YourClass.Enabled" value="true" />` and
to disable it you set the same key to `false`. This can also be done during runtime if your services implement 
`IGuardedService`.


## Creating services

To get started with the library you need to create a class that either implement 
`IApplicationService`
or  `IGuardedService`. They contain methods which are used to start/stop the services. 

However, if you implement the interfaces directly you need to make sure that your classes do not crash. If you want 
this library to take care of everything you need to inherit one of the following classes:

### ApplicationServiceThread

`ApplicationServiceThread` is running as a background thread and is great for everything that requires to run
as long as your application. Any uncaught exception will temporarily shutdown the service (until the 
service manager restarts it).

That means that you do not have to be afraid of that a bug in a service will consume all the CPU
or not be started again.

**Example service**

```csharp
public class YourService : ApplicationserviceThread
{
    protected void Run(WaitHandle shutdownHandle)
    {
        //100ms break until the service is signalled to stop
        while (!shutdownHandle.Wait(100))
        {
            try
            {
                // do actual logic here.
            } 
            catch (DataException ex)
            {
                // no need for try/catch, but we only want the service
                // to get automatically restarted when DataException is thrown
                throw;
            }
            catch (Exception ex)
            {
                _log.Error("Opps", ex);
            }
        }
    }
}
```

### ApplicationServiceTimer

**ApplicationServiceTimer** allows you to execute jobs in the background without
having to deal with unhandled exceptions.

Do note that instances of this class are also treated as single instances.

```csharp
public class DeleteOldMessages : ApplicationServiceTimer
{
    public override void Execute()
    {
        using (var connection = new SqlConnection("..."))
        {
            using (var transaction = connection.BeginTransaction())
            {
                //delete query.
            }
        }
    }
}
```

However, if you are using timers that requires a container scope  
(lifetime scope) you might want to use
`IBackgroundJob` or `ScopedTaskInvoker`. 

Read the [documentation](Backgroundjobs) for more information.
