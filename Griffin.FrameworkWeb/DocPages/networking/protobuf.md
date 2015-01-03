Protobuf
=========

ProtoBuf is Googles open serialization format which can be used to 
serialize objects in a standardized way. With it, different platforms can 
communicate with a format that is much more efficient than XML. 

Combine the most popular implementation if it, protobuf-net, with Griffin.Framework 
and you get an easy and fast way of sending information between processes.

# Installation

Install the nuget package `griffin.framework.protobuf`.

# Example

Here is an example

## Messages

```csharp
[ProtoContract]
public class Authenticate
{
    [ProtoMember(1)]
    public string UserName { get; set; }
 
    [ProtoMember(2)]
    public string Password { get; set; }
}
 
[ProtoContract]
public class AuthenticateReply
{
    [ProtoMember(1)]
    public bool Success { get; set; }
 
    [ProtoMember(2)]
    public string Decision { get; set; }
}
```

## Server

As a server you typically just create a ChannelTcpListener:

```csharp
var settings = new ChannelTcpListenerConfiguration(
    () => new MicroMessageDecoder(new ProtoBufSerializer()),
    () => new MicroMessageEncoder(new ProtoBufSerializer())
    );
 
var server = new MicroMessageTcpListener(settings);
server.MessageReceived = OnServerMessage;
 
server.Start(IPAddress.Any, 1234); 
```

That’s it, all you need now is a callback to receive messages from all clients:

```csharp
private static void OnServerMessage(ITcpChannel channel, object message)
{
    var auth = (Authenticate) message;
    channel.Send(new AuthenticateReply() {Success = true});
}
```

How much code/configuration would you need in WCF to achieve the same thing?

## The client

In the client you do about the same, but you use async/await instead of a callback.

```csharp
private static async Task RunClient()
{
    var client = new ChannelTcpClient<object>(
        new MicroMessageEncoder(new ProtoBufSerializer()),
        new MicroMessageDecoder(new ProtoBufSerializer())
        );
 
    await client.ConnectAsync(IPAddress.Parse("192.168.1.3"), 1234);
 
    for (int i = 0; i < 20000; i++)
    {
        await client.SendAsync(new Authenticate { UserName = "jonas", Password = "king123" });
        var reply = (AuthenticateReply)await client.ReceiveAsync();
 
        if (reply.Success)
        {
            //Console.WriteLine("Client: Yay, we are logged in.");
        }
        else
        {
            //Console.WriteLine("Client: " + reply.Decision);
        }
    }
 
    await client.CloseAsync();
}
```
