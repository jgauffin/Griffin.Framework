using System;
using System.IO;
using System.Security.AccessControl;
using System.Threading;
using Griffin.IO;

namespace Griffin.ApplicationServices.AppDomains.Controller
{
    /// <summary>
    ///     The purpose is to monitor the pickup path for changes and then copy the new version to a new version folder.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The class waits 30 seconds from the last change before initiating a copy. The wait is done so that we are sure
    ///         that no new changes
    ///         have been added.
    ///     </para>
    ///     <para>
    ///         Version folders are just named as the DateTime (<c>date.ToString("yyyy-MM-dd_HHmmss")</c>) for the newest file
    ///         in the pickup directory.
    ///     </para>
    ///     <para>
    ///         You can specify your own directory security for the version folders by setting the <see cref="Security" />
    ///         property.
    ///     </para>
    /// </remarks>
    public class NewVersionDetector : IDisposable
    {
        private readonly string _appPath;
        private readonly string _pickupPath;
        private readonly Timer _updateTimer;
        private DateTime _changeDetectedAt;
        private readonly FileSystemWatcher _fileSystemWatcher;
        private DateTime _lastRunningDomainCreatedAt;
        private bool _started;


        /// <summary>
        /// Initializes a new instance of the <see cref="NewVersionDetector"/> class.
        /// </summary>
        /// <param name="pickupPath">Folder to monitor</param>
        /// <param name="appPath">Folder to create version folders under.</param>
        /// <exception cref="System.ArgumentNullException">
        /// pickupPath
        /// or
        /// appPath
        /// </exception>
        public NewVersionDetector(string pickupPath, string appPath)
        {
            if (pickupPath == null) throw new ArgumentNullException("pickupPath");
            if (appPath == null) throw new ArgumentNullException("appPath");

            _pickupPath = pickupPath;
            _appPath = appPath;

            _fileSystemWatcher = new FileSystemWatcher(_pickupPath);
            _fileSystemWatcher.IncludeSubdirectories = true;
            _fileSystemWatcher.Changed += OnFilesChanges;
            _fileSystemWatcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite;

            _updateTimer = new Timer(OnCheckForReload, null, Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        ///     Custom security settings for the new (application) folders.
        /// </summary>
        public DirectorySecurity Security { get; set; }

        private void OnFilesChanges(object sender, FileSystemEventArgs e)
        {
            if (!_started)
                return;

            _changeDetectedAt = DateTime.UtcNow;
        }

        private void OnCheckForReload(object state)
        {
            if (_changeDetectedAt == DateTime.MinValue)
                return;
            if (_changeDetectedAt.Subtract(_lastRunningDomainCreatedAt).TotalSeconds < 30)
                return;

            CopyAppDomain();
        }

        private void CopyAppDomain()
        {
            var newestDate = DateTime.MinValue;
            foreach (var file in Directory.GetFiles(_pickupPath, "*.*", SearchOption.AllDirectories))
            {
                if (File.GetLastWriteTimeUtc(file) > newestDate)
                    newestDate = File.GetLastWriteTimeUtc(file);
            }


            var newDomainPath = Path.Combine(_appPath, newestDate.ToString("yyyy-MM-dd_HHmmss"));
            if (Security != null)
                DirectoryUtils.Copy(_pickupPath, newDomainPath, true, Security);
            else
            {
                DirectoryUtils.Copy(_pickupPath, newDomainPath, true);
            }
            _lastRunningDomainCreatedAt = DateTime.UtcNow;
            VersionFound(this, new VersionFoundEventArgs(newDomainPath));
        }

        /// <summary>
        ///     A new version was found.
        /// </summary>
        public event EventHandler<VersionFoundEventArgs> VersionFound = delegate { };


        /// <summary>
        ///     Start monitoring for new releases in the <see cref="ApplicationManagerSettings.PickupPath" />,
        /// </summary>
        public void Start()
        {
            _updateTimer.Change(1000, 1000);
            _started = true;
        }

        /// <summary>
        ///     Stop monitoring for new releases.
        /// </summary>
        public void Stop()
        {
            _started = false;
            _updateTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _fileSystemWatcher.Dispose();
        }
    }
}