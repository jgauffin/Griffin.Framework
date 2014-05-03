using System;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistentQueue{T}"/> class.
        /// </summary>
        /// <param name="dataDirectory">The data directory.</param>
        /// <param name="serializer">The serializer.</param>
        /// <exception cref="System.ArgumentNullException">dataDirectory</exception>
        public PersistentQueue(string dataDirectory, ISerializer serializer)
        {
            if (dataDirectory == null) throw new ArgumentNullException("dataDirectory");
            _dataDirectory = dataDirectory;
            _serializer = serializer;
            CreateDirectoryIfNotExists();

            Encoding = Encoding.UTF8;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PersistentQueue{T}" /> class.
        /// </summary>
        /// <param name="rootFolder">The root folder.</param>
        /// <param name="relativeDirectory">
        ///     The relative directory from the <c>rootFolder</c> (for instance
        ///     <c>"myAppName\Queues\"</c>).
        /// </param>
        /// <param name="serializer">The serializer.</param>
        public PersistentQueue(Environment.SpecialFolder rootFolder, string relativeDirectory, ISerializer serializer)
        {
            _serializer = serializer;
            var path = Environment.GetFolderPath(rootFolder);
            path = Path.Combine(path, relativeDirectory, "Griffin");
            _dataDirectory = path;
            CreateDirectoryIfNotExists();
            Encoding = Encoding.UTF8;
        }

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
            Directory.CreateDirectory(directoryName, security);
        }


        public async Task<T> DequeueAsync()
        {
            var indexFile = _dataDirectory + typeof (T).Name + ".idx";

            var tempFile = Path.Combine(_dataDirectory, Path.GetFileNameWithoutExtension(indexFile) + "TMP.idx");

            // we failed during processing, copy back the original file.
            if (File.Exists(tempFile))
            {
                if (File.Exists(indexFile))
                    File.Delete(indexFile);
                File.Move(tempFile, indexFile);
            }

            File.Move(indexFile, tempFile);


            var targetFile = "";
            try
            {
                _lock.Wait();

                var buf = new char[65535];
                using (var reader = new StreamReader(File.OpenRead(tempFile)))
                using (var writer = new StreamWriter(File.OpenWrite(indexFile)))
                {
                    targetFile = await reader.ReadLineAsync();

                    var len = await reader.ReadAsync(buf, 0, buf.Length);
                    while (len > 0)
                    {
                        await writer.WriteAsync(buf, 0, len);
                        len = await reader.ReadAsync(buf, 0, buf.Length);
                    }
                }

                File.Delete(tempFile);
            }
            finally
            {
                _lock.Release();
            }


            targetFile = Path.Combine(_dataDirectory, typeof (T).Name, targetFile);
            T result;
            using (var file = File.OpenRead(targetFile))
            {
                result = (T) _serializer.Deserialize(file, typeof (T));
            }
            File.Delete(targetFile);
            return result;
        }

        public async Task EnqueueAsync<T>(T item)
        {
            var filename = typeof (T).Name + "_" + Guid.NewGuid().ToString("N") + ".json";
            if (!Directory.Exists(Path.Combine(_dataDirectory, typeof (T).Name)))
                CreateDirectory(Path.Combine(_dataDirectory, typeof (T).Name));

            using (var stream = File.OpenWrite(Path.Combine(_dataDirectory, typeof (T).Name, filename)))
            {
                _serializer.Serialize(item, stream, typeof(T));
            }

            try
            {
                _lock.Wait();
                var indexFile = _dataDirectory + typeof (T).Name + ".idx";
                using (var stream = new FileStream(indexFile, FileMode.Append, FileAccess.Write))
                {
                    var buf = Encoding.GetBytes(filename + "\r\n");
                    await stream.WriteAsync(buf, 0, buf.Length);
                }
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}