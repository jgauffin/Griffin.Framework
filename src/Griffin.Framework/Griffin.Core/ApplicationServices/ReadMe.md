# Application services

Sometimes you need to be able to execute things in the background or periodically. To do that you have Timers, Threads and Tasks in .NET.

They all require you do so some manual handling to start/restart and shutdown them when your application is started/stopped. This library
has two different alternatives.

Both alternatives have support for inversion of control containers. You either have to install one of our IoC adapter packages from nuget
or implement [two interfaces](../Container) for your favorite container.

# IApplicationService

This contract allows you to start background services when your application starts and stop them when the application ends. They are treated
as single instances and are also monitored by this library.

The [ApplicationServiceManager](ApplicationServiceManager.cs) also have the ability to restart services that crashes.

[Read more here](Docs/ApplicationServices.md).

# IBackgroundJob

Do you have jobs which need to be run in the background by still need short lived objects like transactions or database connections?

The background jobs are executed in isolated container life times which means that you can use transactions etc for them without affecting 
the rest of your application.

[Read more here](Docs/BackgroundJobs.md).