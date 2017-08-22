using System;
using System.Collections;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Griffin.IO
{
    /// <summary>
    /// Used to store items on disk
    /// </summary>
    /// <typeparam name="T">Type of item to store, may be a base type.</typeparam>
    public class PersistentQueue<T> : IQueue<T>
    {
        private readonly string _dataDirectory;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1);
        private readonly ISerializer _serializer;
        private readonly PersistentCircularIndex _index;
        private const int RecordSize = 32;//Guid.NewGuid().ToString("N").Length;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistentQueue{T}"/> class.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">dataDirectory</exception>
        public PersistentQueue(PersistentQueueConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            _dataDirectory = Path.Combine(configuration.DataDirectory, configuration.QueueName);
            _serializer = configuration.Serializer;
            CreateDirectoryIfNotExists();

            var indexFileName = Path.Combine(configuration.DataDirectory, configuration.QueueName + ".idx");
            _index = new PersistentCircularIndex(indexFileName, RecordSize, configuration.MaxCount);

            Encoding = Encoding.UTF8;
        }

        /// <summary>
        /// Encoding used during serialization
        /// </summary>
        public Encoding Encoding { get; set; }

        private void CreateDirectoryIfNotExists()
        {
            if (!Directory.Exists(_dataDirectory))
            {
                CreateDirectory(_dataDirectory);
            }
        }

        private void CreateDirectory(string directoryName)
        {
            var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            var security = new DirectorySecurity();
            security.AddAccessRule(new FileSystemAccessRule(sid,
                FileSystemRights.Modify | FileSystemRights.ReadAndExecute,
                AccessControlType.Allow));
            Directory.CreateDirectory(directoryName);
            new DirectoryInfo(directoryName).SetAccessControl(security);
        }


        /// <summary>
        /// Dequeue an item
        /// </summary>
        /// <returns>Task that will complete once an item have been successfully read from disk</returns>
        public async Task<T> DequeueAsync()
        {
            //var tempFile = Path.Combine(_dataDirectory, _indexFileName.Remove(_indexFileName.Length - 4, 4) + "TMP.idx");

            //// we failed during processing, copy back the original file.
            //if (File.Exists(tempFile))
            //{
            //    if (File.Exists(_indexFileName))
            //        File.Delete(_indexFileName);
            //    File.Move(tempFile, _indexFileName);
            //}

            //File.Move(_indexFileName, tempFile);


            var targetFile = "";
            try
            {
                await _lock.WaitAsync();
                var filename = _index.Dequeue();
                if (filename == null)
                    return default(T);

                targetFile = Path.Combine(_dataDirectory, filename);


                //File.Delete(tempFile);
            }
            finally
            {
                _lock.Release();
            }


            targetFile = Path.Combine(_dataDirectory, targetFile + ".json");
            T result;
            using (var file = File.OpenRead(targetFile))
            {
                result = (T)_serializer.Deserialize(file, typeof(T));
            }
            File.Delete(targetFile);
            return result;
        }

        /// <summary>
        /// enqueue an item
        /// </summary>
        /// <param name="item">Item to enqueue</param>
        /// <returns>Task that completes once the item have been successfully written to disk.</returns>
        public async Task EnqueueAsync(T item)
        {
            var filename = Guid.NewGuid().ToString("N") + ".json";

            using (var stream = File.OpenWrite(Path.Combine(_dataDirectory, filename)))
            {
                _serializer.Serialize(item, stream, typeof(T));
            }

            try
            {
                await _lock.WaitAsync();
               _index.Enqueue(Path.GetFileNameWithoutExtension(filename));
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}