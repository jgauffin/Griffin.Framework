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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Griffin.Core.Json;
using Griffin.Cqs.Demo.Command;
using Griffin.Cqs.Net;
using Griffin.Cqs.Simple;

namespace Griffin.Cqs.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = CreateServer();
            server.Start(IPAddress.Any, 0);

			// Wait for enter before shutting down the server
            Console.ReadLine();

			server.Stop();
        }

        private static CqsServer CreateServer()
        {
            var cmdBus = new SimpleCommandBus();
            cmdBus.Register(typeof (Program).Assembly);

            var queryBus = new SimpleQueryBus();
            queryBus.Register(typeof (Program).Assembly);

            var requestReplyBus = new SimpleRequestReplyBus();
            requestReplyBus.Register(typeof (Program).Assembly);

            var eventBus = new SimpleEventBus();
            eventBus.Register(typeof (Program).Assembly);

            var server = new CqsServer(cmdBus, queryBus, eventBus, requestReplyBus);
            server.SerializerFactory = () => new JsonMessageSerializer();
            
            return server;
        }
    }
}
```

## Example client

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Griffin.Core.Json;
using Griffin.Cqs.Demo.Command;
using Griffin.Cqs.Net;
using Griffin.Cqs.Simple;

namespace Griffin.Cqs.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new CqsClient(() => new JsonMessageSerializer());
            client.StartAsync(IPAddress.Loopback, server.LocalPort).Wait();

            // Invoking a command
            client.ExecuteAsync(new IncreaseDiscount(1)).Wait();
            
            Console.ReadLine();

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
        /// <summary>
        /// Execute a command asynchronously.
        /// </summary>
        /// <param name="command">Command to execute.</param>
        /// <returns>
        /// Task which will be completed once the command has been executed.
        /// </returns>
        public async Task ExecuteAsync(IncreaseDiscount command)
        {
            if (command.Percent == 1)
                throw new Exception("Must increase with at least two percent, cheap bastard!");

            Console.WriteLine("Being executed");
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Console.WriteLine("Being disposed");
        }
    }
}
```


