using System;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotNetCqs;
using Griffin.IO;

namespace Griffin.Cqs
{
    public class FileStorage : IFileStorage
    {
        private readonly string _dataDirectory;
        private PersistentQueue<Command> _commandQueue;
        private PersistentQueue<IQuery> _queryQueue;
        private PersistentQueue<IRequest> _requestQueue;
        private PersistentQueue<ApplicationEvent> _eventQueue;

        public FileStorage(string dataDirectory)
        {
            if (dataDirectory == null) throw new ArgumentNullException("dataDirectory");

            _settings = new JsonSerializerSettings()
            {
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                ObjectCreationHandling = ObjectCreationHandling.Auto,
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
            };

            _dataDirectory = dataDirectory;
            CreateDirectoryIfNotExists();

            Encoding = Encoding.UTF8;
        }

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
            security.AddAccessRule(new FileSystemAccessRule(sid, FileSystemRights.Modify | FileSystemRights.ReadAndExecute,
                AccessControlType.Allow));
            Directory.CreateDirectory(directoryName, security);
        }

        public FileStorage(Environment.SpecialFolder rootFolder, string applicationName)
        {
            _settings = new JsonSerializerSettings()
            {
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                ObjectCreationHandling = ObjectCreationHandling.Auto,
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
            };

            var path = Environment.GetFolderPath(rootFolder);
            path = Path.Combine(path, applicationName, "DotNetCqs");
            _dataDirectory = path;
            CreateDirectoryIfNotExists();
            Encoding = Encoding.UTF8;
        }

        public Encoding Encoding { get; set; }

        public async Task<Command> PopCommandAsync()
        {
            var data = await PopAsync("Command", _commandLock);
            return Deserialize<Command>(data);
        }

        public async Task<ApplicationEvent> PopEventAsync()
        {
            var data = await PopAsync("ApplicationEvent", _eventLock);
            return Deserialize<ApplicationEvent>(data);
        }

        public async Task<IQuery> PopQueryAsync()
        {
            var data = await PopAsync("Query", _queryLock);
            return Deserialize<IQuery>(data);
        }

        public async Task<IRequest> PopRequestAsync()
        {
            var data = await PopAsync("Request", _requestLock);
            return Deserialize<IRequest>(data);
        }

        public async Task PushAsync(Command command)
        {
            var data = Serialize<Command>(command);
            await PushAsync("Command", data, _commandLock);
        }

        public async Task PushAsync<T>(Request<T> request)
        {
            var data = Serialize<IRequest>(request);
            await PushAsync("Request", data, _requestLock);
        }


        public async Task PushAsync(ApplicationEvent appEvent)
        {
            var data = Serialize<ApplicationEvent>(appEvent);
            await PushAsync("ApplicationEvent", data, _eventLock);
        }


        public async Task PushAsync<T>(Query<T> query)
        {
            var data = Serialize<IQuery>(query);
            await PushAsync("Query", data, _queryLock);
        }

        private T Deserialize<T>(string data)
        {
            if (data == null) throw new ArgumentNullException("data");
            return JsonConvert.DeserializeObject<T>(data, _settings);
        }


        private async Task<string> PopAsync(string cqsType, SemaphoreSlim indexLock)
        {
            var indexFile = _dataDirectory + cqsType + ".idx";

            var tempFile = Path.Combine(_dataDirectory, Path.GetFileNameWithoutExtension(indexFile) + "TMP.idx");

            // we failed during processing, copy back the original file.
            if (File.Exists(tempFile))
            {
                if (File.Exists(indexFile))
                    File.Delete(indexFile);
                File.Move(tempFile, indexFile);
            }

            File.Move(indexFile, tempFile);


            var cqsFileName = "";
            try
            {
                indexLock.Wait();

                var buf = new char[65535];
                using (var reader = new StreamReader(File.OpenRead(tempFile)))
                using (var writer = new StreamWriter(File.OpenWrite(indexFile)))
                {
                    cqsFileName = await reader.ReadLineAsync();

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
                indexLock.Release();
            }


            cqsFileName = Path.Combine(_dataDirectory, cqsType, cqsFileName);
            var result = File.ReadAllText(cqsFileName);
            File.Delete(cqsFileName);
            return result;
        }

        private async Task PushAsync(string cqsType, string data, SemaphoreSlim indexLock)
        {
            var filename = cqsType + "_" + Guid.NewGuid().ToString("N") + ".json";
            if (!Directory.Exists(Path.Combine(_dataDirectory, cqsType)))
                CreateDirectory(Path.Combine(_dataDirectory, cqsType));

            using (var stream = File.OpenWrite(Path.Combine(_dataDirectory, cqsType, filename)))
            {
                var buf = Encoding.GetBytes(data);
                await stream.WriteAsync(buf, 0, buf.Length);
            }

            try
            {
                indexLock.Wait();
                var indexFile = _dataDirectory + cqsType + ".idx";
                using (var stream = new FileStream(indexFile, FileMode.Append, FileAccess.Write))
                {
                    var buf = Encoding.GetBytes(filename + "\r\n");
                    await stream.WriteAsync(buf, 0, buf.Length);
                }
            }
            finally
            {
                indexLock.Release();
            }
        }

        private string Serialize<T>(object cqsObject)
        {
            return JsonConvert.SerializeObject(cqsObject, typeof(T), _settings);
        }
    }
}