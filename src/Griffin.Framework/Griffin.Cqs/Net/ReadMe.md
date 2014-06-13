Network bus/client
=======================

The following namespace contains an implementation which uses `Griffin.Net` to transport CQS objects over the network. 

# Exceptions

Exceptions are automatically transported back to the client which makes this library transparent (looks like the CQS object was executed locally).

Do note that all exceptions must be created with support for serialization, i.e. mark it with `[Serializable]` and include the serialization constructor:

```csharp
using System;
using System.Runtime.Serialization;

namespace Griffin.Cqs
{
    [Serializable]
    public class CqsHandlerMissingException : Exception
    {

        public CqsHandlerMissingException(Type type)
            : base("Missing a handler for '" + type.FullName + "'.")
        {
        }

        protected CqsHandlerMissingException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}
```

## Serializing properties

Here is the standard way of including properties:

```csharp
using System;
using System.Runtime.Serialization;

namespace Griffin.Cqs
{
    [Serializable]
    public class CqsHandlerMissingException : Exception
    {
        public CqsHandlerMissingException(Type type)
            : base(string.Format("Missing a handler for '{0}'.", type.FullName))
        {
            CqsType = type.FullName;
        }

        protected CqsHandlerMissingException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
            CqsType = info.GetString("CqsType");
        }

        public string CqsType { get; set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("CqsType", CqsType);
        }
    }
}
```

# Example

Here is a working example.

## Server


```csharp
using System.Net;
using Griffin.Core.Json;
using Griffin.Cqs.Net;
using Griffin.Net.Protocols.MicroMsg;
using Griffin.Net.Server;

namespace Griffin.Cqs.Demo.Server
{
    public class ServerDemo
    {
        private LiteServer _server;

        public int LocalPort
        {
            get { return _server.LocalPort; }
        }

        public void Setup()
        {
            var root = new CompositionRoot();
            root.Build();

            var module = new CqsModule
            {
                CommandBus = CqsBus.CmdBus,
                QueryBus = CqsBus.QueryBus,
                RequestReplyBus = CqsBus.RequestReplyBus,
                EventBus = CqsBus.EventBus
            };


            var config = new LiteServerConfiguration();
            config.DecoderFactory = () => new MicroMessageDecoder(new JsonMessageSerializer());
            config.EncoderFactory = () => new MicroMessageEncoder(new JsonMessageSerializer());

            config.Modules.AddAuthentication(new AuthenticationModule());
            config.Modules.AddHandler(module);

            _server = new LiteServer(config);
        }

        public void Start()
        {
            _server.Start(IPAddress.Any, 0);
        }
    }
}
```

## Example client

```csharp
using System;
using System.Net;
using System.Threading.Tasks;
using Griffin.Core.Json;
using Griffin.Cqs.Demo.Contracts.Cqs;
using Griffin.Cqs.Net;

namespace Griffin.Cqs.Demo.Client
{
    internal class ClientDemo
    {
        private readonly CqsClient _client;

        public ClientDemo()
        {
            _client = new CqsClient(() => new JsonMessageSerializer());
        }

        public async Task RunAsync(int port)
        {
            await _client.StartAsync(IPAddress.Loopback, port);

            Console.WriteLine("Client: Executing request/reply");
            var response = await _client.ExecuteAsync<LoginReply>(new Login("jonas", "mamma"));
            if (response.Success)
                Console.WriteLine("Client: Logged in successfully");
            else
            {
                Console.WriteLine("Client: Failed to login");
                return;
            }

            Console.WriteLine("Client: Executing command");
            await _client.ExecuteAsync(new IncreaseDiscount(20));


            Console.WriteLine("Client: Executing query");
            var discounts = await _client.QueryAsync(new GetDiscounts());
            Console.WriteLine("Client: First discount: " + discounts[0].Name);
        }
    }
}

```

## Dependencies

The following command and handler was used in the example:

```csharp
namespace Griffin.Cqs.Demo.Command
{
    public class IncreaseDiscount : DotNetCqs.Command
    {
        public IncreaseDiscount(int percent)
        {
            Percent = percent;
        }

        protected IncreaseDiscount()
        {
        }

        public int Percent { get; private set; }
    }

    public class IncreaseDiscountHandler : ICommandHandler<IncreaseDiscount>, IDisposable
    {
        public async Task ExecuteAsync(IncreaseDiscount command)
        {
            if (command.Percent == 1)
                throw new Exception("Must increase with at least two percent, cheap bastard!");

            Console.WriteLine("Being executed");
        }

        public void Dispose()
        {
            Console.WriteLine("Being disposed");
        }
    }
}
```


