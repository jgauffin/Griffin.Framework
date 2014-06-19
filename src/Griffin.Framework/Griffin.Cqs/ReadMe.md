Griffin.Framework implementation of the DotNetCqs specification
===================

DotNetCqs is a specification library for Command/Query seperation implementations in .NET.

This library includes three different implementations:

* InversionOfControl - Use an Inversion Of Control container to execute the CQS objects.
* Net - Execute the CQS objects remotely
* Simple - Get started easily to try the CQS pattern

# Creating a server

```csharp

// enables CQS in the server
var module = new CqsModule();

// to activate IoC in the server (comment out if you do not want it)
// requires a Griffin Framework [adapter](../Griffin.Core/InversionOfControl)
module.CommandBus = new IocCommandBus(_yourContainerAdapter);
module.RequestreplyBus = new IocRequestReplyBus(_yourContainerAdapter);
module.EventBus = new IocEventBus(_yourContainerAdapter);
module.QueryBus = new IocQueryBus(_yourContainerAdapter);

var config = new LiteServerConfiguration();

//optional, using our own [transport protocol](../Griffin.Core/Net/Protocols/MicroMsg) with JSON as message serializer
config.EncoderFactory = () => new MicroMessageEncoder(new JsonMessageSerializer());
config.DecoderFactory = () => new MicroMessageDecoder(new JsonMessageSerializer());

//required
config.Modules.AddHandler();

//setup the [server](../Griffin.Core/Net/Server)
var server = new LiteServer();
server.Start();
```

# Creating a client


## sample project

Can be found [here](https://github.com/jgauffin/Griffin.Framework/tree/master/samples/Griffin.Cqs.Demo)