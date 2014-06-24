# Application services

Sometimes you need to be able to execute things in the background or periodically. To do that you have Timers, Threads and Tasks in .NET.

They all require you do so some manual handling to start/restart and shutdown them when your application is started/stopped. This library
has two different alternatives.

Both alternatives have support for inversion of control containers. You either have to install one of our IoC adapter packages from nuget
or implement [two interfaces](../Container) for your favorite container.

# IApplicationService

This contract allows you to start background services when your application starts and stop them when the application ends. They are treated
as single instances and are also monitored by this library. Services that fail (unhandled exceptions) are automatically restarted by the library.

Using one of the base classes in the library:

```csharp
public class YourService : ApplicationserviceThread
{
    protected void Run(WaitHandle shutdownHandle)
    {
        while (true)
        {
            try
            {
                // pause 100ms between each loop iteration.
                // you can specify 0 too
                if (shutdownHandle.WaitOne(100))
                    break;

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

[Read more here](Docs/ApplicationServices.md).

# IBackgroundJob

Do you have jobs which need to be run in the background by still need short lived objects like transactions or database connections?

The background jobs are executed in isolated container life times which means that you can use transactions etc for them without affecting 
the rest of your application.

```csharp
public class CleanUpOldFriendRequests : IBackgroundJob
{
    private readonly IUnitOfWork _uow;
    private static DateTime _lastExecutionTime;

    public CleanUpOldFriendRequests(IUnitOfWork uow)
    {
        if (uow == null) throw new ArgumentNullException("uow");

        _uow = uow;
    }

    public void Execute()
    {
        //run once a day
        if (_lastExecutionTime.Date >= DateTime.Today)
            return;
        _lastExecutionTime = DateTime.Today;

        using (var cmd = _uow.CreateCommand())
        {
            cmd.CommandText = "DELETE FROM FriendRequests WHERE CreatedAtUtc < @datum";
            cmd.AddParameter("datum", DateTime.Today.AddDays(-10));
            cmd.ExecuteNonQuery();
        }
    }
}
```

[Read more here](Docs/Backgroundjobs.md).