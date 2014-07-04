using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Griffin.ApplicationServices.AppDomains.Controller;
using Griffin.ApplicationServices.AppDomains.Host;
using Griffin.IO;
using UnhandledExceptionEventArgs = Griffin.ApplicationServices.AppDomains.UnhandledExceptionEventArgs;

namespace Griffin.ApplicationServices.AppDomains
{
    /// <summary>
    /// Takes care of all app domains that have been created due to new files.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Launches the application in a shadow coopeid
    /// </para>
    /// </remarks>
    public class ApplicationManager<T> where T : class, IApplicationInitialize
    {
        private readonly ApplicationManagerSettings _settings;
        private DateTime _detectedChange;
        private DateTime _lastRunningDomainCreatedAt;
        private HostedAppDomain _runningDomain;
        private HostedAppDomain _pendingDomain;
        private string _latestWorkingVersion;
        private IConfigAdapter _config;
        private NewVersionDetector _versionDetector;
        private NamedPipeServer _server;

        public ApplicationManager(ApplicationManagerSettings settings)
        {
            _settings = settings;

            _config = new RegistryConfigAdapter(settings.CompanyName, settings.ApplicationName);
            _server = new NamedPipeServer();
            _server.ClientDisconnected += OnClientConnectionDisconnected;
            _server.ReceivedCommand += OnClientConnectionCommand;
            _server.ClientConnectionFailure += OnClientConnectionFailure;
        }

        private void OnClientConnectionFailure(object sender, UnhandledExceptionEventArgs e)
        {
            //TODO: Is the domain going down?
        }

        private void OnClientConnectionCommand(object sender, ClientReceivedCommandEventArgs e)
        {
            if (e.ClientId == _runningDomain.Id)
                _runningDomain.ProcessRemoteCommand(e.Command, e.Argv);
            else if (_pendingDomain != null && _pendingDomain.Id == e.ClientId)
                _pendingDomain.ProcessRemoteCommand(e.Command, e.Argv);
            else
            {
                //TODO: LOG
            }
        }

        private void OnClientConnectionDisconnected(object sender, EventArgs e)
        {
            //TODO: 
        }

        private void OnNewVersion(object sender, VersionFoundEventArgs e)
        {
            try
            {
                if (_runningDomain != null)
                {
                    _runningDomain.Stop();
                    _runningDomain.PanicRequested -= OnClientDomainPanic;
                    _runningDomain.AppDomainException -= OnClientDomainException;
                }

            }
            catch (Exception exception)
            {
                
            }

            try
            {
                var id = Path.GetFileName(e.VersionPath);
                var domain = new HostedAppDomain(typeof(T));
                domain.Configure(id, e.VersionPath);
                domain.Start();
                domain.AppDomainException += OnClientDomainException;
                domain.PanicRequested += OnClientDomainPanic;
                _runningDomain = domain;
                _config["RunningVersion"] = id;

            }
            catch (Exception exception)
            {

            }
        }

        private void OnClientDomainPanic(object sender, EventArgs e)
        {
            
        }


        private void StartAppDomain(string versionDirectory)
        {
          
        }

        private void StopAppDomain()
        {
            
        }

        private void OnClientDomainException(object sender, UnhandledExceptionStringEventArgs e)
        {
            
        }


        public void Start()
        {
            _settings.Validate();

            if (_versionDetector == null)
            {
                _versionDetector = new NewVersionDetector(_settings.PickupPath, _settings.AppDirectory);
                _versionDetector.VersionFound += OnNewVersion;
            }
            _versionDetector.Start();
            _server.Start();
        }

        public void Stop()
        {
            _versionDetector.Stop();
            _server.Stop();
        }
    }
}
