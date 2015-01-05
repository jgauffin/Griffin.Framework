using System;
using System.IO;
using System.Text;

namespace Griffin.ApplicationServices.AppDomains.Host
{
    /// <summary>
    ///     Represents a running appdomain instance.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         this class is run in the main app domain and communicates with the application AppDomain using
    ///         <see cref="AppInitializeRunner" />.
    ///     </para>
    /// </remarks>
    public class HostedAppDomain
    {
        private readonly Type _startType;
        private AppDomain _appDomain;
        private AppDomainSetup _appDomainSetup;
        private string _appName;
        private AppInitializeRunner _domainManager;
        private string _shadowFolder;


        /// <summary>
        /// </summary>
        /// <param name="startType">
        ///     Class which starts the application, can be compared with global.asax in ASP.NET or the service
        ///     class for Windows services.
        /// </param>
        public HostedAppDomain(Type startType)
        {
            _startType = startType;
        }

        /// <summary>
        /// When this domain was started (UTC)
        /// </summary>
        public DateTime RunningSince { get; private set; }

        /// <summary>
        ///     Generated id for this specific app domain
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Configure this domain
        /// </summary>
        /// <param name="id">AppDomain identifier</param>
        /// <param name="appDirectory">Directory where new versions are stored (in sub folders)</param>
        public void Configure(string id, string appDirectory)
        {
            Id = id;
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _appName = Path.GetFileName(AppDomain.CurrentDomain.BaseDirectory);
            _shadowFolder = Path.Combine(appData, _appName, id);

            _appDomainSetup = new AppDomainSetup
            {
                ApplicationBase = appDirectory,
                ShadowCopyFiles = "true",
                CachePath = _shadowFolder,
                ShadowCopyDirectories = null,
                //null = copy all: http://blogs.msdn.com/b/junfeng/archive/2004/02/09/69919.aspx
            };
        }

        /// <summary>
        ///     Launch app domain and start the services in it.
        /// </summary>
        public void Start()
        {
            if (Id == null)
                throw new InvalidOperationException("Run Configure() first.");

            RunningSince = DateTime.UtcNow;
            _appDomain = AppDomain.CreateDomain(_appName, null, _appDomainSetup);
            _domainManager =
                (AppInitializeRunner)
                    _appDomain.CreateInstanceAndUnwrap("Griffin.Core", typeof (AppInitializeRunner).FullName);
            _domainManager.Start(Id, _startType.AssemblyQualifiedName);
        }

        /// <summary>
        ///     A command received from the <see cref="AppInitializeRunner" /> through the named pipe.
        /// </summary>
        /// <param name="command">command to execute</param>
        /// <param name="argv">arguments for the command</param>
        public void ProcessRemoteCommand(string command, string[] argv)
        {
            switch (command)
            {
                case "AppDomainException":
                    TriggerAppDomainException(argv[0]);
                    break;
                case "PANIC":
                    PanicRequested(this, EventArgs.Empty);
                    break;
            }
        }

        private void TriggerAppDomainException(string base64)
        {
            var bytes = Convert.FromBase64String(base64);
            var exception = Encoding.UTF8.GetString(bytes);
            AppDomainException(this, new UnhandledExceptionStringEventArgs(exception));
        }

        /// <summary>
        ///     An exception was caught in the hosted app domain
        /// </summary>
        public event EventHandler<UnhandledExceptionStringEventArgs> AppDomainException = delegate { };

        /// <summary>
        ///     Failed to launch or run this version. Roll back to previous version.
        /// </summary>
        public event EventHandler PanicRequested = delegate { };


        /// <summary>
        ///     Stop this app domain
        /// </summary>
        public void Stop()
        {
            _domainManager.StopService();
        }
    }
}