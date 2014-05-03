using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.IO
{
    public class PersistentQueue
    {

    }


    /// <summary>
    /// A circular index.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used for the persistant queue to keep track of all files and 
    /// </para>
    /// </remarks>
    public class PersistentCircularIndex
    {
        private readonly string _fileName;
        private readonly int _maxDataSize;
        private readonly int _maxQueueSize;
        private const int HeaderLength = sizeof(Int32) * 5;
        private object _syncLock = new object();

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
                for (int i = 0; i < maxQueueSize; i++)
                {
                    writer.Write(data, 0, maxDataSize);
                }

                fs.Flush();
            }
        }

        public void Enqueue(string data)
        {
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

                    if (queueSize == maxQueueSize)
                        throw new InvalidOperationException("Queue is full.");

                    buf = new byte[recordSize];
                    int len = Encoding.ASCII.GetBytes(data, 0, data.Length, buf, 0);
                    if (len > recordSize)
                        throw new ArgumentOutOfRangeException("data", data, "Record size is max " + recordSize + " bytes.");

                    var nextWriteRecord = writeRecord + 1;
                    if (nextWriteRecord == maxQueueSize)
                    {
                        nextWriteRecord = 0;
                    }
                    writer.Seek(0, SeekOrigin.Begin);
                    writer.Write(queueSize+1);
                    writer.Write(readRecord);
                    writer.Write(nextWriteRecord);
                }

                writer.Seek(writeRecord * recordSize + HeaderLength, SeekOrigin.Begin);
                writer.Write(buf, 0, buf.Length);
            }
        }

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
                    readRecord = reader.ReadInt32();
                    var writeRecord = reader.ReadInt32();
                    recordSize = reader.ReadInt32();
                    var maxQueueSize = reader.ReadInt32();

                    // same record, we've read everything.
                    if (readRecord == writeRecord)
                        return null;

                    var nextReadRecord = readRecord + 1;
                    if (nextReadRecord == maxQueueSize)
                    {
                        nextReadRecord = 0;
                    }
                    //move to the beginning again, to prevent disk scanning
                    if (writeRecord == readRecord)
                    {
                        writeRecord = 0;
                        readRecord = 0;
                    }

                    writer.Seek(0, SeekOrigin.Begin);
                    writer.Write(queueSize - 1);
                    writer.Write(nextReadRecord);
                    writer.Write(writeRecord);
                }
                

                reader.BaseStream.Position = (readRecord *recordSize) + HeaderLength;
                var buf = reader.ReadBytes(recordSize);
                return Encoding.ASCII.GetString(buf).TrimEnd('\0');
            }
        }
    }
}
