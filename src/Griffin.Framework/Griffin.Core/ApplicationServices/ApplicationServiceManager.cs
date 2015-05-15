using System;
using System.Collections.Concurrent;
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
    ///         Otherwise they will only be started/stopped when <c>Start()</c>/<c>Stop()</c> is invoked on this manager.
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
    /// // Discover all classes that implement IApplicationservice
    /// var serviceLocator = new AssemblyScanner();
    /// serviceLocator.Scan(Assembly.GetExecutingAssembly());
    /// 
    /// //run the services.
    /// _serviceManager = new ApplicationServiceManager(serviceLocator);
    /// _serviceManager.Start();
    /// 
    /// // [...]
    /// 
    /// // .. when the application is shut down..
    /// _serviceManager.Stop();
    /// </code>
    /// </example>
    public class ApplicationServiceManager
    {
        private readonly ConcurrentDictionary<string, bool> _configOverrides = new ConcurrentDictionary<string, bool>();
        private readonly ILogger _logger = LogManager.GetLogger<ApplicationServiceManager>();
        private readonly IAppServiceLocator _serviceLocator;
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

            _serviceLocator = new IocAppServiceLocator(container);
            CheckInterval = TimeSpan.FromSeconds(30);
            StartInterval = TimeSpan.FromSeconds(5);
            _timer = new Timer(OnCheckServices, null, Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ApplicationServiceManager" /> class.
        /// </summary>
        /// <param name="serviceLocator">Implementation used to find all registered services.</param>
        /// <exception cref="System.ArgumentNullException">container</exception>
        public ApplicationServiceManager(IAppServiceLocator serviceLocator)
        {
            if (serviceLocator == null) throw new ArgumentNullException("serviceLocator");

            _serviceLocator = serviceLocator;
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
        ///     How frequently we should check if there are any services that should be started/stopped.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <value>Default is every 30 seconds.</value>
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
        ///     Service failed to execute properly (an unhandled exception was caught).
        /// </summary>
        public event EventHandler<ApplicationServiceFailedEventArgs> ServiceFailed = delegate { };


        /// <summary>
        ///     Disable service (so it will be shut down during the next service check).
        /// </summary>
        /// <param name="className">Name of class (with or without namespace)</param>
        /// <remarks>
        ///     <para>
        ///         Requires that the service implements <see cref="IGuardedService" />.
        ///     </para>
        ///     <para>
        ///         This will override the setting in the configuration file.
        ///     </para>
        /// </remarks>
        /// <example>
        ///     <code>
        /// serviceManager.Settings.Disable("YourClassName");
        /// </code>
        /// </example>
        public void Disable(string className)
        {
            _configOverrides[className] = false;
        }

        /// <summary>
        ///     Disable service (so it will be shut down during the next service check)
        /// </summary>
        /// <param name="type">Type to disable</param>
        /// <remarks>
        ///     <para>
        ///         Requires that the service implements <see cref="IGuardedService" />.
        ///     </para>
        ///     <para>
        ///         This will override the setting in the configuration file.
        ///     </para>
        /// </remarks>
        public void Disable(Type type)
        {
            _configOverrides[type.FullName] = false;
        }

        /// <summary>
        ///     Enable a service (so that it will be started during the next service check)
        /// </summary>
        /// <param name="className">Name of class (with or without namespace)</param>
        /// <remarks>
        ///     <para>
        ///         Requires that the service implements <see cref="IGuardedService" />.
        ///     </para>
        ///     <para>
        ///         This will override the setting in the configuration file.
        ///     </para>
        /// </remarks>
        /// <example>
        ///     <code>
        /// serviceManager.Settings.Disable("YourClassName");
        /// </code>
        /// </example>
        public void Enable(string className)
        {
            _configOverrides[className] = true;
        }

        /// <summary>
        ///     Enable service (so it will be started during the next service check)
        /// </summary>
        /// <param name="type">Type to disable</param>
        /// <remarks>
        ///     <para>
        ///         Requires that the service implements <see cref="IGuardedService" />.
        ///     </para>
        ///     <para>
        ///         This will override the setting in the configuration file.
        ///     </para>
        /// </remarks>
        public void Enable(Type type)
        {
            _configOverrides[type.FullName] = true;
        }

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
            var services = _serviceLocator.GetServices();
            foreach (var service in services)
            {
                var guarded = service as IGuardedService;
                if (guarded != null)
                    guarded.Failed += OnServiceFailed;

                if (!IsEnabled(service.GetType()))
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

        private void OnServiceFailed(object sender, ApplicationServiceFailedEventArgs e)
        {
            ServiceFailed(this, e);
        }

        /// <summary>
        ///     Will shut down all services and stop checking their health.
        /// </summary>
        /// <exception cref="System.AggregateException">Contains a list of errors for all services that could not be stopped.</exception>
        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);

            var exceptions = new List<Exception>();
            var services = _serviceLocator.GetServices();
            foreach (var service in services)
            {
                var guarded = service as IGuardedService;
                if (guarded != null)
                    guarded.Failed -= OnServiceFailed;
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

        /// <summary>
        ///     check services wether they should be started/stopped.
        /// </summary>
        /// <returns></returns>
        internal void CheckServices()
        {
            var services = _serviceLocator.GetServices();
            foreach (var service in services)
            {
                var guarded = service as IGuardedService;
                if (guarded == null)
                    continue;

                try
                {
                    if (!IsEnabled(service.GetType()))
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

        private bool IsEnabled(Type type)
        {
            var enabled = false;
            if (_configOverrides.TryGetValue(type.FullName, out enabled))
                return enabled;
            if (_configOverrides.TryGetValue(type.Name, out enabled))
                return enabled;

            return _settings.IsEnabled(type);
        }

        private void OnCheckServices(object state)
        {
            try
            {
                CheckServices();
            }
            catch (Exception exception)
            {
                _logger.Warning("Failed to start one or more services.", exception);
            }
        }
    }
}