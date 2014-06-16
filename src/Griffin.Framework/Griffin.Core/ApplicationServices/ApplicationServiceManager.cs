using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Griffin.Container;
using Griffin.Logging;

namespace Griffin.ApplicationServices
{
    /// <summary>
    ///     Used to manage services that runs as long as the application (i.e. single instance classes)
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Services that have been enabled/disabled during runtime will be started/stopped automatically by this library
    ///         if they
    ///         implement <see cref="IGuardedService" />. Same goes for services which have crashed.
    /// Otherwise they will only be started/stopped when <c>Start()</c>/<c>Stop()</c> is invoked on this manager.
    ///     </para>
    ///     <para>
    ///         You can also start/stop services at runtime using your config file (app/web.config) if you use the
    ///         <see cref="AppConfigServiceSettings" /> class as configuration source (or your own implementation).
    ///     </para>
    ///     <para>
    ///         If you are using a inversion of control container your classes should be registered as "Single Instance" in it
    ///         for this class to work properly.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <para>Start by creating a class:</para>
    ///     <code>
    /// public class MyQueueReader : IApplicationService
    /// {
    ///     ServiceBusReader _reader;
    /// 
    ///     public MyQueueReader(/* can use dependency injection */)
    ///     {
    ///         _reader = new ServiceBusReader(blabla);
    ///     }
    /// 
    ///     public bool IsRunning  { get { return _reader.IsRunning; } }
    /// 
    ///     public void Start()
    ///     {
    ///         _reader.Start();
    ///     }
    /// 
    ///     public void Stop()
    ///     {
    ///         _reader.Stop();
    ///     }
    /// }
    /// </code>
    ///     <para>To enable the service in your app.config:</para>
    ///     <code>
    /// <![CDATA[
    /// <configuration>
    ///  <appSettings>
    ///    <add key="MyQueueReader.Enabled" value="true" />
    ///  <appSettings>
    /// </configuration>
    /// ]]>
    /// </code>
    ///     <para>
    ///         Finally you have to create the service manager:
    ///     </para>
    ///     <code>
    /// // Example using Autofac
    /// var builder = new ContainerBuilder();
    /// 
    /// // register using the [ContainerService] attribut in this library, see the "Container" namespace.
    /// builder.RegisterServices(Assembly.GetExecutingAssembly());
    /// 
    /// var container = new AutofacServiceLocator(container);
    /// 
    /// // Create manager and start all services.
    /// _serviceManager = new ApplicationServiceManager(container);
    /// _serviceManager.Start();
    /// 
    /// 
    /// // .. when the application is shut down..
    /// _serviceManager.Stop();
    /// </code>
    /// </example>
    public class ApplicationServiceManager
    {
        private readonly IContainer _container;
        private readonly ILogger _logger = LogManager.GetLogger<ApplicationServiceManager>();
        private readonly Timer _timer;
        private TimeSpan _checkInterval;
        private ISettingsRepository _settings = new AppConfigServiceSettings();

        /// <summary>
        ///     Initializes a new instance of the <see cref="ApplicationServiceManager" /> class.
        /// </summary>
        /// <param name="container">Inversion of control container (griffin adapter to support all containers).</param>
        /// <exception cref="System.ArgumentNullException">container</exception>
        public ApplicationServiceManager(IContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");

            _container = container;
            CheckInterval = TimeSpan.FromSeconds(30);
            StartInterval = TimeSpan.FromSeconds(5);
            _timer = new Timer(OnCheckServices, null, Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        ///     Used to be able to determine which services should be running
        /// </summary>
        public ISettingsRepository Settings
        {
            get { return _settings; }
            set { _settings = value ?? new AppConfigServiceSettings(); }
        }

        /// <summary>
        ///     Hur ofta som vi ska kontrollera om tjänsterna är uppe och rullar
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <value>Default är var 30:e sekund.</value>
        public TimeSpan CheckInterval
        {
            get { return _checkInterval; }
            set
            {
                _checkInterval = value;
                if (_timer != null)
                    _timer.Change(_checkInterval, _checkInterval);
            }
        }

        /// <summary>
        ///     Amount of time before starting services for the first time.
        /// </summary>
        /// <value>
        ///     Default is 5 seconds.
        /// </value>
        public TimeSpan StartInterval { get; set; }

        /// <summary>
        ///     Failed to start a service (no matter if it's for the first time or later on)
        /// </summary>
        public event EventHandler<ApplicationServiceFailedEventArgs> ServiceStartFailed = delegate { };

        /// <summary>
        ///     Start all services and start monitoring them.
        /// </summary>
        /// <exception cref="System.AggregateException">
        ///     Contains one or more <see cref="StartServiceException" /> with information about the services that could not be
        ///     started.
        /// </exception>
        public void Start()
        {
            var exceptions = new List<Exception>();
            var services = _container.ResolveAll<IApplicationService>();
            foreach (var service in services)
            {
                if (!Settings.IsEnabled(service.GetType()))
                {
                    _logger.Debug("Service is disabled '" + service.GetType().FullName + "'.");
                    continue;
                }

                try
                {
                    _logger.Info("Starting service '" + service.GetType().FullName + "'.");
                    service.Start();
                }
                catch (Exception exception)
                {
                    exceptions.Add(new StartServiceException(service, exception));
                }
            }

            _timer.Change(StartInterval, CheckInterval);
            if (exceptions.Any())
                throw new AggregateException(exceptions);
        }

        /// <summary>
        ///     Will shut down all services and stop checking their health.
        /// </summary>
        /// <exception cref="System.AggregateException">Contains a list of errors for all services that could not be stopped.</exception>
        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);

            var exceptions = new List<Exception>();
            var services = _container.ResolveAll<IApplicationService>();
            foreach (var service in services)
            {
                var guarded = service as IGuardedService;
                if (guarded != null && !guarded.IsRunning)
                    continue;

                try
                {
                    _logger.Info("Stopping service '" + service.GetType().FullName + "'.");
                    service.Stop();
                }
                catch (Exception exception)
                {
                    exceptions.Add(new TargetInvocationException(
                        "Failed to stop '" + service.GetType().FullName + "'.", exception));
                }
            }

            if (exceptions.Any())
                throw new AggregateException(exceptions);
        }

        private void OnCheckServices(object state)
        {
            try
            {
                var services = _container.ResolveAll<IApplicationService>();
                foreach (var service in services)
                {
                    var guarded = service as IGuardedService;
                    if (guarded == null)
                        continue;

                    try
                    {
                        if (!Settings.IsEnabled(service.GetType()))
                        {
                            if (guarded.IsRunning)
                            {
                                _logger.Info("Stopping service that have been disabled '" + service.GetType().FullName +
                                             "'.");
                                service.Stop();
                            }
                            continue;
                        }

                        if (guarded.IsRunning)
                            continue;

                        _logger.Info("Starting service that should be running '" + service.GetType().FullName + "'.");
                        service.Start();
                    }
                    catch (Exception exception)
                    {
                        var args = new ApplicationServiceFailedEventArgs(service, exception);
                        ServiceStartFailed(this, args);
                        if (!args.CanContinue)
                            return;
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.Warning("Failed to start one or more services.", exception);
            }
        }
    }
}