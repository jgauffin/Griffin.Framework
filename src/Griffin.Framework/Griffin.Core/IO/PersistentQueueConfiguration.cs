using System;
using System.IO;
using Griffin.IO.Serializers;

namespace Griffin.IO
{
    /// <summary>
    /// Configuration options for <see cref="PersistentQueue{T}"/>.
    /// </summary>
    public class PersistentQueueConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersistentQueueConfiguration"/> class.
        /// </summary>
        /// <param name="queueName">Name of the queue.</param>
        /// <exception cref="System.ArgumentNullException">queueName</exception>
        public PersistentQueueConfiguration(string queueName)
        {
            if (queueName == null) throw new ArgumentNullException("queueName");
            QueueName = queueName;
            Serializer = new BinaryFormatterSerializer();
            MaxCount = 100;
            DataDirectory = Path.GetTempPath();
        }

        /// <summary>
        /// Max amount of items in the queue
        /// </summary>
        /// <remarks>
        /// <para>
        /// The implementation is using a circular index file where all records are pre-created. You therefore have to specify the maximum amount of items that can be in the queue. The index
        /// file is approximely 32 bytes * MaxCount bytes large. i.e. 1000 records means a 32 000 bytes large index file.
        /// </para>
        /// </remarks>
        /// <value>
        /// Default is 100.
        /// </value>
        public int MaxCount { get; set; }

        /// <summary>
        /// Name of the queue
        /// </summary>
        /// <remarks>
        /// <para>
        /// A sub directory will be created with this name and the index file will also be named as the specified name.
        /// </para>
        /// </remarks>
        public string QueueName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <example>
        /// <para>You can get a windows folder by doing like this:</para>
        /// <code>
        ///  var path = Environment.GetFolderPath(rootFolder);
        ///  path = Path.Combine(path, @"MyAppName\Queues");
        /// 
        ///  config.DataDirectory = path;
        /// </code>
        /// </example>
        /// <value>
        /// Default is <c>Path.GetTempPath()</c>
        /// </value>
        public string DataDirectory { get; set; }

        /// <summary>
        /// Kind of serializer to use.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Install the nuget package <c>Griffin.Framework.Json</c> if you would like to use JSON.NET.
        /// </para>
        /// </remarks>
        public ISerializer Serializer { get; set; }
    }
}