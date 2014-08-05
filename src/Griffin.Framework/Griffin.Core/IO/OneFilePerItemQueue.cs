using System;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Griffin.IO.Serializers;

namespace Griffin.IO
{
    /// <summary>
    ///     Stores every queued item in a separate file.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Creates a new directory under the <see cref="DataDirectory" /> which is named as <see cref="QueueName" />. That
    ///         directory will
    ///         be sued to store all files using a number counter and <c>.data</c> as file extension.
    ///     </para>
    ///     <para>
    ///         This library will create the data directory and the queue directory automatically if they do not exist. By
    ///         default EVERYONE will
    ///         receive read/write permissions to that directory. If you want to reconfigure that you have to use the
    ///         <see cref="DataDirectorySecurity" /> property.
    ///     </para>
    /// </remarks>
    /// <typeparam name="T">Type of queued item</typeparam>
    public class OneFilePerItemQueue<T> : IQueue<T>
    {
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1);
        private string _fullPath;
        private int _nextReadNumber;
        private int _nextWriteNumber;
        private ISerializer _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="OneFilePerItemQueue{T}"/> class.
        /// </summary>
        /// <param name="queueName">Name of the queue.</param>
        /// <exception cref="System.ArgumentNullException">queueName</exception>
        public OneFilePerItemQueue(string queueName)
        {
            if (queueName == null) throw new ArgumentNullException("queueName");
            QueueName = queueName;
            Serializer = new BinaryFormatterSerializer();
            DataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "GriffinFramework");
        }

        /// <summary>
        ///     Used to serialize items.
        /// </summary>
        /// <value>
        ///     Default is <see cref="BinaryFormatterSerializer" />.
        /// </value>
        public ISerializer Serializer
        {
            get { return _serializer; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                _serializer = value;
            }
        }

        /// <summary>
        ///     Directory to store items in.
        /// </summary>
        /// <remarks>
        ///     This name is combined with the queue name. i.e. if the data directory is "C:\Queues" and the queueName is
        ///     "Commands" the directory to use
        ///     will be "C:\Queues\Commands".
        /// </remarks>
        /// <value>
        ///     Default is
        ///     <c>Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GriffinFramework")</c>
        /// </value>
        public string DataDirectory { get; set; }

        /// <summary>
        ///     Used when creating directories automatically.
        /// </summary>
        public DirectorySecurity DataDirectorySecurity { get; set; }

        /// <summary>
        ///     Name of the current queue
        /// </summary>
        public string QueueName { get; private set; }

        /// <summary>
        ///     Dequeue an item from our queue.
        /// </summary>
        /// <returns>Dequeued item; <c>default(T)</c> if there are no more items in the queue.</returns>
        public async Task<T> DequeueAsync()
        {
            if (_fullPath == null)
                throw new InvalidOperationException("Invoke Start() first");

            try
            {
                await _lock.WaitAsync();
                var filename= GetReadFileName();
                if (filename == null)
                    return default(T);

                try
                {
                    using (var stream = File.OpenRead(filename))
                    {
                        return (T) Serializer.Deserialize(stream, typeof (T));
                    }
                }
                finally 
                {
                    File.Delete(filename);
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        private string GetReadFileName()
        {
            while (true)
            {
                if (_nextReadNumber == _nextWriteNumber)
                    return null;


                var filename = GetFileName(_nextReadNumber++);
                if (_nextReadNumber == int.MaxValue)
                    _nextReadNumber = 1;

                if (File.Exists(filename))
                    return filename;

                Debug.Assert(false, "Should have got a filename");
            }
            
        }

        /// <summary>
        ///     Enqueue item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task EnqueueAsync(T item)
        {
            if (_fullPath == null)
                throw new InvalidOperationException("Invoke Start() first");

            try
            {
                await _lock.WaitAsync();
                var filename = GetFileName(_nextWriteNumber++);
                if (_nextWriteNumber == int.MaxValue)
                    _nextWriteNumber = 1;

                if (File.Exists(filename))
                    throw new FileNotFoundException("Expected next queue item to NOT exist: " + filename, filename);

                try
                {
                    using (var stream = File.OpenWrite(filename))
                    {
                        Serializer.Serialize(item, stream, typeof (T));
                    }
                }
                catch
                {
                    try
                    {
                        File.Delete(filename);
                    }
                    catch (Exception exception)
                    {
                        Debug.Fail("Failed delete file " + filename, exception.ToString());
                    }
                    throw;
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        public void Start()
        {
            _fullPath = Path.Combine(DataDirectory, QueueName);

            if (Directory.Exists(DataDirectory))
                CreateDirectory(DataDirectory);

            if (!Directory.Exists(_fullPath))
                CreateDirectory(_fullPath);

            FindIndexes();
        }

        private void CreateDirectory(string directoryName)
        {
            var security = DataDirectorySecurity;
            if (security == null)
            {
                var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                security = new DirectorySecurity();
                security.AddAccessRule(new FileSystemAccessRule(sid,
                    FileSystemRights.Modify | FileSystemRights.ReadAndExecute,
                    AccessControlType.Allow));
            }
            Directory.CreateDirectory(directoryName, security);
        }

        private void FindIndexes()
        {
            _nextReadNumber = int.MaxValue;
            _nextWriteNumber = int.MinValue;
            var files = Directory.EnumerateFiles(_fullPath, "*.data");
            foreach (var file in files)
            {
                var numberStr = Path.GetFileNameWithoutExtension(file);
                int number;
                if (!int.TryParse(numberStr, out number))
                    continue;

                if (number < _nextReadNumber)
                    _nextReadNumber = number;
                if (number > _nextWriteNumber)
                    _nextWriteNumber = number;
            }

            if (_nextWriteNumber != int.MinValue)
                _nextWriteNumber++; // have to write AFTER the existing file.

            // queue is empty
            if (_nextWriteNumber == int.MinValue)
                _nextWriteNumber = 1;
            if (_nextReadNumber == int.MaxValue)
                _nextReadNumber = 1;

            // reached maximum, start over
            if (_nextReadNumber == int.MaxValue)
                _nextReadNumber = 1;
            if (_nextWriteNumber == int.MaxValue)
                _nextWriteNumber = 1;
        }

        private string GetFileName(int index)
        {
            return Path.Combine(_fullPath, index + ".data");
        }
    }
}