# Background jobs

Do you have work which needs to be run occasionally? Do you use an inversion of control container?

Then the background jobs are for you. If not, look at [ApplicationServiceTimer](../ApplicationServiceTimer.cs) instead.

The jobs are run at certain intervals within an isolated child scope in a container.
That means that all other jobs will continue to work, even if one of your background jobs fail. It also means that
you can use a own transaction for each job.

# Running the jobs

The jobs are managed by the [BackgroundJobManager](../BackgroundJobManager.cs) class. It takes care of starting the jobs, running
them at defined intervals and finally report any errors if they fail.

To get started you need to create a new instance of the class and provide the IoC adapter that will be used to locate services.

```csharp
public class Service1 : ServiceBase
{
    BackgroundJobManager _jobInvoker;
    IContainer  _container;

    public Service1()
    {
        _serviceLocator = CreateContainer();

        _jobInvoker = new BackgroundJobManager(_container);
        _jobInvoker.ScopeClosing += OnScopeClosing;
    }

    public override OnStart(string[] argv)
    {
        _jobInvoker.Start();
    }

    public override OnStop()
    {
        _jobInvoker.Stop();
    }

    public void CreateContainer()
    {
        // create your favorite container (this example uses autofac)
        // and register your services in it.
        var builder = new Containerbuilder();
        builder.Register<DeleteOldMessages>().AsImplementedInterfaces().SingleInstance();

        // and then create the griffin adapter
        var autofac = builder.Build();
        _container = new AutofacAdapter(autofac);
    }

    // so that we can commit the transaction
    // event will not be invoked if something fails.
    public void OnScopeClosing(object sender, ScopeCreatedEventArgs e)
    {
        e.Scope.Resolve<IUnitOfWork>().SaveChanges();
    }
}
```

## Controlling intervals

In the `BackgroundJobManager` there are two properties which controls the lifetime

Name			| Description
---------------| ---------
StartInterval	| Amount of time before the jobs are executed for the first time (after `Start()` have been invoked).
ExecuteInterval | Interval between every job execution (for a single job). The interval is reseted when until the job returns.

## Logging errors

Sometimes job fails. You can log all errors by subscribing on an event:

```csharp
public class Service1 : ServiceBase
{
    BackgroundJobManager _jobInvoker;
    IContainer  _container;

    public Service1()
    {
        _serviceLocator = CreateContainer();

        _jobInvoker = new BackgroundJobManager(_container);
        _jobInvoker.JobFailed += OnBackgroundJobFailure;
    }

    private void OnBackgroundJobFailure(object sender, BackgroundJobFailedEventArgs e)
    {
        _logger.Error("Failed to execute " + e.Job, e.Exception);
    }

    // [....]
}
```

# Creating jobs

To create a background service you have to do the following:

1. Implement the [IBackgroundJob](../IBackgroundJob.cs) interface in your own class.
2. Register the class in your IoC container.

That's it.

Sample implementation:

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

The example also displays how you can run the job less frequently than what's configured in the `BackgroundJobManager`. Simply create a
static variable and check it in the `Execute()` method.