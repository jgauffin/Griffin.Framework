Networking
===================

here are two extension points in the new library.

The first point is the ITcpChannelFactory which are used to create the channels that are used for communication. In that way you can get a fine grained control over what goes on in the IO operations. There are two built in types of channels. TcpChannel which is used for regular communication and SecureTcpChannel which uses TLS/SSL as transport security.

You can also customize which channels the library should use by changing the ChannelFactory property:

server.ChannelFactory = new SecureTcpChannelFactory(new ServerSideSslStreamBuilder(myCertificate));
The second extension point is that the library uses encoders and decoders on the incoming data. There are some built in encoders. For instance for the STOMP messaging protocol and of course for HTTP. Each time your create a client or a server you specify which encoder/decoder to use.

# Performance

A benchmark have been made with the MicroMsg protocol for 
transport and protobuf-net as the serializer. 

Griffin Framework was able to send 400 000 .NET objects 
(or rather 800 000 as it was request/reply) in 11 seconds. 

That’s 0,0279675 millisecond per message. 

# Fully asynchronous

The library is fully asynchronous both in the server side and the client side. 
The server do however not support TPL yet 
as there is no built in support in the .NET socket API, 
async/await would therefore have to be a layer on top of the socket handling. 

Currently you have to use callbacks.

## Example

```csharp
private void OnMessage(ITcpChannel channel, object message)
{
    BeginDoSomething(message, OnEndDoSomething, channel);
}
 
public void OnEndDoSomething(IAsyncResult ar)
{
    var channel = (ITcpChannel) ar.AsyncState;
    channel.Send("Sometrhing");
}
```

The server doesn’t act before you invoke `channel.Send()`
and you can do that at any time (as long as the channel is connected). 
Do note however that the server will continue to read from the channel and
 invoke the
`OnMessage` delegate for every new message, even if you have not replied to
 the first.

# Building a server

Typically you do not need to create a new class every time you want to
 build a server. Instead you pick one of the encoder/decoder classes and 
pass them to the `ChannelTcpListener` class.

Finally you just hook into the listener callbacks to receive messages. 
Do note that the receive callback will give you constructed .NET objects
 and not a byte array or similar.

```csharp
public class Server
{
    private readonly ChannelTcpListener _server;
 
    public Server()
    {
        _server = new ChannelTcpListener();
        _server.MessageReceived += OnMessage;
        _server.ClientConnected += OnClientConnected;
        _server.ClientDisconnected += OnClientDisconnected;
    }
 
    public int LocalPort
    {
        get { return _server.LocalPort; }
    }
 
    public void Start()
    {
        _server.Start(IPAddress.Any, 0);
    }
 
    private void OnClientConnected(object sender, ClientConnectedEventArgs e)
    {
        Console.WriteLine("Got connection from client with ip " + e.channel.RemoteEndPoint);
    }
 
    private void OnClientDisconnected(object sender, ClientDisconnectedEventArgs e)
    {
        Console.WriteLine("Disconnected: " + e.Channel.RemoteEndpoint);
    }
 
    private void OnMessage(ITcpChannel channel, object message)
    {
        Console.WriteLine("Server received: " + message);
    }
}
```

The last step is to send information back to the server in your recieve callback:

```csharp
private void OnMessage(ITcpChannel channel, object message)
{
    // we can send any kind of object, all is serialized per default using DataContractSerializer
    channel.Send("Well, hello you too");
}
```

The serialization is taken care of by the Library thanks of the specified message encoder class. That means that you can at any time switch format. You could start with our MicroMessage encoder and later on switch to HTTP as transport without having to change the rest of your server.

Per default, the ChannelTcpListener uses MicroMsg. The transport protocol basically adds a content length and type information into a header. The great thing with MicroMsg is that it supports streams out of the box. 
You can for instance send a `FileStream`:

```csharp
private void OnMessage(ITcpChannel channel, object message)
{
    channel.Send(new FileStream(@"C:\VeryLargeFile.big"));
}
```

It doesn’t matter if the file is 10GB or just a few KB. The library will transfer it without even increasing the memory footprint (the library switches between a MemoryStream or FileStream depending on the stream size).

# Channel storage

In more complex solution you might want the ability to store information specific for a channel (i.e. client connection). I support that by the
`channel.Data´ property which basically wraps a `ConcurrentDictionary`.

## Example

```csharp
private void OnMessage(ITcpChannel channel, object message)
{
    channel.Data["CurrentMessage"] = message;
 
    BeginDoSomething(message, OnEndDoSomething, channel);
}
 
public void OnEndDoSomething(IAsyncResult ar)
{
    var channel = (ITcpChannel) ar.AsyncState;
 
    var msg = channel.Data["CurrentMessage"];
    channel.Send("Something");
}
```

# Clients

There is a built in client in the library. It’s fully asynchronous using TPL 
and async/await. It uses per default the MicroMsg protocol.

Here is a client example:

```csharp
// will transport any type of object
var client = new ChannelTcpClient<object>();
 
await client.ConnectAsync(IPAddress.Loopback, serverPort);
 
// send a string, but could have been your own DTO
await client.SendAsync("Hello world");
 
// receive a response from the server.
var response = await client.ReceiveAsync();
 
// close client
await client.CloseAsync();
```

To talk with a HTTP server you can construct a client like this:

```csharp
var client = new ChannelTcpClient<IHttpResponse>(new HttpRequestEncoder(), new HttpResponseDecoder());
await client.ConnectAsync(SomeIp, 80);
await client.SendAsync(new HttpRequest("GET", "/", "HTTP/1.1"));
var response = await client.ReceiveAsync();
Console.WriteLine(response.StatusCode);
```

Note that’s it’s just a demonstration of how you can construct server/clients just as long as there are encoders/decoders. Basically building encoders/decoders are the only thing you need to do to support new protocols, no need to even touch the networking code.

# Custom protocol

Sometimes it’s not enough to use HTTP, STOMP or MicroMsg. 
Instead you have to use another format.

Here is a custom protocol which only adds a length header and a 
string body (JSON). Remember that TCP is stream based and we therefore 
have to make sure that everything is sent/received.

```csharp
public class MyProtocolEncoder : IMessageEncoder
{
    private readonly MemoryStream _memoryStream = new MemoryStream(65535);
    private readonly JsonSerializer _serializer = new JsonSerializer();
    private int _bytesLeft;
    private int _offset;
 
    public void Prepare(object message)
    {
        // Clear the memory stream
        _memoryStream.SetLength(0);
 
        // start at position 4 so that we can insert the length header.
        _memoryStream.Position = 4;
        var writer = new StreamWriter(_memoryStream);
        _serializer.Serialize(writer, message, typeof (object));
        writer.Flush();
 
        // insert length header.
        // BitConverter2 exists in a library and do use an existing buffer instead of allocating a new one.
        BitConverter2.GetBytes((int) _memoryStream.Length - 4, _memoryStream.GetBuffer(), 0);
 
        // and save how much we should send
        _bytesLeft = (int) _memoryStream.Length;
        _offset = 0;
    }
 
 
    public void Send(ISocketBuffer buffer)
    {
        // continue where the last send operation stopped.
        buffer.SetBuffer(_memoryStream.GetBuffer(), _offset, _bytesLeft);
    }
 
    public bool OnSendCompleted(int bytesTransferred)
    {
        // a send operation completed, move forward in our buffer.
        _offset += bytesTransferred;
        _bytesLeft -= bytesTransferred;
        return _bytesLeft <= 0;
    }
 
    public void Clear()
    {
        _bytesLeft = 4;
        _memoryStream.SetLength(0);
        _memoryStream.Position = 0;
    }
}
```

The decoder have to take into account that messages might be partial or that the incoming stream contains several messages.

```
public class MyProtocolDecoder : IMessageDecoder
{
    private readonly byte[] _headerBuf = new byte[4];
    private readonly JsonSerializer _serializer = new JsonSerializer();
    private readonly MemoryStream _stream = new MemoryStream();
    private int _bytesLeft;
    private int _headerBytesLeft = 4;
    private int _headerOffset = 0;
    private bool _isHeaderRead;
 
    public void ProcessReadBytes(ISocketBuffer buffer)
    {
        var offset = buffer.Offset;
        var count = buffer.BytesTransferred;
 
        // start by reading header.
        if (!_isHeaderRead)
        {
            var headerBytesRead = Math.Min(_headerBytesLeft, count);
            Buffer.BlockCopy(buffer.Buffer, offset, _headerBuf, _headerOffset, headerBytesRead);
            _headerBytesLeft -= headerBytesRead;
 
            // have not received all bytes left.
            // can occur if we send a lot of messages so that the nagle algorithm merges
            // messages so that the header is in the end of the socket stream.
            if (_headerBytesLeft > 0)
                return;
 
            count -= headerBytesRead;
            offset += headerBytesRead;
            _bytesLeft = BitConverter.ToInt32(_headerBuf, 0);
            _isHeaderRead = true;
        }
 
        var bodyBytesToRead = Math.Min(_bytesLeft, count);
        _stream.Write(buffer.Buffer, offset, bodyBytesToRead);
        _bytesLeft -= bodyBytesToRead;
        if (_bytesLeft == 0)
        {
            _stream.Position = 0;
            var item = _serializer.Deserialize(new StreamReader(_stream), typeof (object));
            MessageReceived(item);
            Clear();
 
            //TODO: Recursive call to read any more messages
        }
    }
 
    public void Clear()
    {
        _bytesLeft = 0;
        _headerBytesLeft = 4;
        _headerOffset = 0;
        _isHeaderRead = false;
        _stream.SetLength(0);
        _stream.Position = 0;
    }
 
    public Action<object> MessageReceived { get; set; }
}
```

..and finally the server that uses the custom protocol:

Here is a sample implementation which a custom protocol (length header + JSON). It can send any type of object over the network (as long as JSON.NET can serialize it).

```
class Program
{
    static void Main(string[] args)
    {
        var config = new ChannelTcpListenerConfiguration(
            () => new MyProtocolDecoder(), 
            () => new MyProtocolEncoder()
        );
        var server = new ChannelTcpListener(config);
        server.MessageReceived += OnMessage;
        server.Start(IPAddress.Any, 0);
 
 
        ExecuteClient(server).Wait();
 
        Console.WriteLine("Demo completed");
        Console.ReadLine();
 
    }
 
    private static async Task ExecuteClient(ChannelTcpListener server)
    {
        var client = new MyProtocolClient();
        await client.ConnectAsync(IPAddress.Loopback, server.LocalPort);
        await client.SendAsync(new Ping{Name = "TheClient"});
        var response = await client.ReceiveAsync();
        Console.WriteLine("Client received: " + response);
    }
 
    private static void OnMessage(ITcpChannel channel, object message)
    {
        var ping = (Ping) message;
 
        Console.WriteLine("Server received: " + message);
        channel.Send(new Pong
        {
            From = "Server", 
            To = ping.Name
        });
    }
}
```

# WebServer

Here is a sample HTTP server:

```
class Program2
{
    static void Main(string[] args)
    {
        var listener = new HttpListener();
        listener.MessageReceived = OnMessage;
        listener.BodyDecoder = new CompositeBodyDecoder();
        listener.Start(IPAddress.Any, 8083);
 
        Console.ReadLine();
    }
 
 
    private static void OnMessage(ITcpChannel channel, object message)
    {
        var request = (HttpRequestBase)message;
        var response = request.CreateResponse();
 
        if (request.Uri.AbsolutePath == "/favicon.ico")
        {
            response.StatusCode = 404;
            channel.Send(response);
            return;
        }
 
        var msg = Encoding.UTF8.GetBytes("Hello world");
        response.Body = new MemoryStream(msg);
        response.ContentType = "text/plain";
        channel.Send(response);
 
        if (request.HttpVersion == "HTTP/1.0")
            channel.Close();
    }
}
```

Using SSL is really easy, just change the channel factory:

```csharp
var certificate = new X509Certificate2("GriffinNetworkingTemp.pfx", "somepassword");
 
var listener = new HttpListener();
listener.ChannelFactory = new SecureTcpChannelFactory(new ServerSideSslStreamBuilder(certificate));
```
