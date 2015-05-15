MicroMsg
========

MicroMsg is our own format which uses a tiny header before the actual message.

# Packet format

A description of the packet format.

## Header

The header consists of:

name | type | description
---- | ---- | -----------
*Header length* | `short` | Number of bytes that are for the header. First counted byte is directly after this field<br><br>
*Version* | `byte` | Defines the version of the micro protocol. Current version is 1<br><br>
*Content Length* | `int` | Defines the length of the body. The body starts directly after the header<br><br>
*Type length* | `sbyte` | Defines the length of the next header value<br><br>
*Type name* | `string` | Fully qualified assembly name of the type that is our payload (UTF8 encoded)<br><br>

### Byte order 

To work with computers that use different byte ordering, all numeric values in the header are sent in network byte order which has the most significant byte first.

## Body

The format of the body is up to the implementor. It's however recommended that the *type name* includes the format. 

For instance:

    "application/protobuf;DemoApp.Models.UserDTO,DemoApp"

Protobuf mime type per [standard proposition](http://tools.ietf.org/html/draft-rfernando-protocol-buffers-00)

# Examples

Examples to get started. This example requires that you install the `griffin.framework.json` nuget package.

## Sample server

Getting a server working requires just a few lines of code:

```csharp
public class Program
{
    public static void Main(string[] argv)
    {
        var settings = new ChannelTcpListenerConfiguration(
            () => new MicroMessageDecoder(new JsonMessageSerializer()),
            () => new MicroMessageEncoder(new JsonMessageSerializer())
            );
 
        var server = new MicroMessageTcpListener(settings);
        server.MessageReceived = OnServerMessage;
 
        server.Start(IPAddress.Any, 1234);	

        //dont kill the server directly
        Console.ReadLine();
    }

    private static void OnServerMessage(ITcpChannel channel, object message)
    {
        var auth = (Authenticate) message;
        channel.Send(new AuthenticateReply() {Success = true});
    }
}
```

If you want to use SSL, simply change to use the secure channel factory by setting the `server.ChannelFactory` before starting the server.

## Sample client

```csharp
public class Program
{
    public static void Main(string[] argv)
    {
        // everything in a method to support async.
        RunClient().Wait();

        //dont kill the server directly
        Console.ReadLine();
    }

    private static async Task RunClient()
    {
        var client = new ChannelTcpClient<object>(
            new MicroMessageEncoder(new JsonMessageSerializer()),
            new MicroMessageDecoder(new JsonMessageSerializer())
            );
 
        await client.ConnectAsync(IPAddress.Parse("192.168.1.3"), 1234);
 
        for (int i = 0; i < 20000; i++)
        {
            //authenticate is just a sample object (replace it with any of your own classes).
            await client.SendAsync(new Authenticate { UserName = "jonas", Password = "king123" });

            //we've sending AuthenticateReply as a reply in the server.
            var reply = (AuthenticateReply)await client.ReceiveAsync();
 
            if (reply.Success)
            {
                Console.WriteLine("Client: Yay, we are logged in.");
            }
            else
            {
                Console.WriteLine("Client: " + reply.Decision);
            }
        }
 
        await client.CloseAsync();
    }

}
```