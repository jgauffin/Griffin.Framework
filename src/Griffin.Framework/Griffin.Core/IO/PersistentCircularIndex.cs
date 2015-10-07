using System;
using System.IO;
using System.Text;

namespace Griffin.IO
{
    /// <summary>
    ///     A circular index.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Stores an index file on disk which is intended for a queue with a known size. The implementation works like a
    ///         circular queue where there are two pointers defined
    ///         in the file, one for the next read position and one for the next write position. In that way we can minimize
    ///         the number of reads required to read/write from the
    ///         file.
    ///     </para>
    /// </remarks>
    public class PersistentCircularIndex
    {
        private const int HeaderLength = sizeof (Int32)*5;
        private readonly string _fileName;
        private readonly int _maxDataSize;
        private readonly int _maxQueueSize;
        private readonly object _syncLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistentCircularIndex"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="maxDataSize">Maximum size of the data.</param>
        /// <param name="maxQueueSize">Maximum size of the queue.</param>
        public PersistentCircularIndex(string fileName, int maxDataSize, int maxQueueSize)
        {
            _fileName = fileName;
            _maxDataSize = maxDataSize;
            _maxQueueSize = maxQueueSize;
            if (!File.Exists(_fileName) || new FileInfo(fileName).Length == 0)
                CreateFile(_fileName, maxDataSize, maxQueueSize);
        }

        private void CreateFile(string fileName, int maxDataSize, int maxQueueSize)
        {
            using (var fs = File.Create(fileName))
            {
                var writer = new BinaryWriter(fs, Encoding.ASCII);
                writer.Write(0); //queue size
                writer.Write(0); //read record
                writer.Write(0); // write record
                writer.Write(maxDataSize);
                writer.Write(maxQueueSize);
                var data = new byte[maxDataSize];
                for (var i = 0; i < maxQueueSize; i++)
                {
                    writer.Write(data, 0, maxDataSize);
                }

                fs.Flush();
            }
        }

        /// <summary>
        /// Enqueue a new string
        /// </summary>
        /// <param name="data">The data.</param>
        /// <exception cref="System.ArgumentNullException">data</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">data;Record size is max  + recordSize +  bytes.</exception>
        /// <exception cref="QueueFullException"></exception>
        public void Enqueue(string data)
        {
            if (data == null) throw new ArgumentNullException("data");

            using (var fs = File.Open(_fileName, FileMode.Open))
            {
                var writer = new BinaryWriter(fs, Encoding.ASCII);
                var reader = new BinaryReader(fs, Encoding.ASCII);
                int writeRecord;
                int recordSize;
                byte[] buf;

                lock (_syncLock)
                {
                    var queueSize = reader.ReadInt32();
                    var readRecord = reader.ReadInt32();
                    writeRecord = reader.ReadInt32();
                    recordSize = reader.ReadInt32();
                    var maxQueueSize = reader.ReadInt32();
                    if (data.Length > recordSize)
                        throw new ArgumentOutOfRangeException("data", data.Length,
                            "Record size is max " + recordSize + " bytes.");

                    if (queueSize == maxQueueSize && queueSize != 0)
                        throw new QueueFullException(Path.GetFileNameWithoutExtension(_fileName));

                    buf = new byte[recordSize];
                    var len = Encoding.ASCII.GetBytes(data, 0, data.Length, buf, 0);

                    var nextWriteRecord = writeRecord + 1;
                    if (nextWriteRecord == maxQueueSize)
                    {
                        nextWriteRecord = 0;
                    }
                    writer.Seek(0, SeekOrigin.Begin);
                    writer.Write(queueSize + 1);
                    writer.Write(readRecord);
                    writer.Write(nextWriteRecord);
                }

                writer.Seek(writeRecord*recordSize + HeaderLength, SeekOrigin.Begin);
                writer.Write(buf, 0, buf.Length);
            }
        }

        /// <summary>
        /// Dequeue an item
        /// </summary>
        /// <returns></returns>
        public string Dequeue()
        {
            using (var fs = File.Open(_fileName, FileMode.Open))
            {
                var writer = new BinaryWriter(fs, Encoding.ASCII);
                var reader = new BinaryReader(fs, Encoding.ASCII);
                int recordSize;
                int readRecord;

                lock (_syncLock)
                {
                    var queueSize = reader.ReadInt32();
                    if (queueSize == 0)
                        return null;

                    readRecord = reader.ReadInt32();
                    var writeRecord = reader.ReadInt32();
                    recordSize = reader.ReadInt32();
                    var maxQueueSize = reader.ReadInt32();

                    var nextReadRecord = readRecord + 1;
                    if (nextReadRecord == maxQueueSize)
                    {
                        nextReadRecord = 0;
                    }
                    //move to the beginning again, to prevent disk scanning
                    if (queueSize - 1 == 0)
                    {
                        writeRecord = 0;
                        nextReadRecord = 0;
                    }

                    writer.Seek(0, SeekOrigin.Begin);
                    writer.Write(queueSize - 1);
                    writer.Write(nextReadRecord);
                    writer.Write(writeRecord);
                }


                reader.BaseStream.Position = (readRecord*recordSize) + HeaderLength;
                var buf = reader.ReadBytes(recordSize);
                return Encoding.ASCII.GetString(buf).TrimEnd('\0');
            }
        }
    }
}