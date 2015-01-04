Command/Query support
=============================

Have you read about the Command/Query separation pattern and wondered how hard it would be to get started with it? With Griffin framework you only need a few lines of code to have everything configured, no matter if the messages are being executed in process or executed in a server application somewhere.

This implementation is based on the `DotNetCqs` specification library which
means that your business code will not be coupled to this implementation but only
to the `DotNetCqs` library.

Here is a small example. Let's start with the message:

```csharp
public class Login : Request<LoginReply>
{
    public Login(string userName, string password)
    {
        Password = password;
        UserName = userName;
    }
 
    public string UserName { get; private set; }
    public string Password { get; private set; }
}
 
public class LoginReply
{
    public bool Success { get; set; }
    public Account Account { get; set; }
}

public class LoginHandler : IRequestHandler<Login, LoginReply>
{
	public Task<LoginReply> ExecuteAsync(Login request)
	{
		//do something
		// exceptions will be handled by the library
	}
}

```

To trigger the CQS messages you execute them through BUS classes. 
To invoke the request above you run the following code:

```csharp
var reply = await requestBus.ExecuteAsync(new Login("jonas", "arne"));
if (reply.Success)
{
    Console.WriteLine("Authenticated as " + reply.Account.UserName);
}
```

# Implementation

Griffin Framework contains three different implementations that you can choose from.

# The simple

The first implementation uses reflection to find all CQS handlers. 
It index them and create a new handler each time a message is being executed.

## Sample setup

```csharp
// setup
var bus = new SimpleCommandBus();
bus.Register(Assembly.GetExecutingAssembly());
 
//execute
await bus.ExecuteAsync(new IncreaseSalary(19549, 50000));
```
[More info](simple)

# Inversion of control

This implementations can utilize your favorite container through our [container abstraction](../ioc/abstraction). 

[More info](ioc)

# Network
Finally I’ve created a dead easy server in Griffin Framework which supports pipelining, secure transport, authentication etc etc. It’s called SimpleServer. I’ve used it as a base to create a CQS server and a client to accompany it.

The server itself can take different shapes depending on how you configure it. It can for instance run as a HTTP server or use our built in transport protocol MicroMsg. The default implementation uses MicroMsg as transport and DataContractSerializer to serialize the CQS messages.

At client side register CqsClient as ICommandBus, IQueryBus, IEventBus and IRequestReplyBus in your favorite container (or use it directly).

Sample setup:

```csharp
class ClientDemo
{
    private CqsClient _client;
 
    public ClientDemo()
    {
        _client = new CqsClient(() => new JsonMessageSerializer());
    }
 
    public async Task RunAsync(int port)
    {
        await _client.StartAsync(IPAddress.Loopback, port);
 
        var response = await _client.ExecuteAsync<LoginReply>(new Login("jonas", "mamma"));
        if (response.Success)
            Console.WriteLine("Client: Logged in successfully");
        else
        {
            Console.WriteLine("Client: Failed to login");
            return;
        }
 
        await _client.ExecuteAsync(new IncreaseDiscount(20));
 
 
        var discounts = await _client.QueryAsync(new GetDiscounts());
        Console.WriteLine("Client: First discount: " + discounts[0].Name);
    }
}
```

The server is configured like this:

```csharp
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
```

This implementation will also transport all exceptions from the server back to the client automatically. That makes the client/server implementation transparent (if you ignore the network latency) to the caller.



# Flexibility

As the contracts (i.e. the interfaces) are so lightweight it allows us to do easy combinations. You could for instance use the decorator pattern to create a bus that uses load balancing, temporary storage and networking:

```csharp
var bus = new TransactionalBus(
              new LoadBalancedBus(
                  new NetworkBus("192.168.1.20:3040"), 
                  new NetworkBus("192.168.1.21:3040"), 
                  new NetworkBus("192.168.1.22:3040")
              )
          );
```

That’s 100% transparent for the user once configured. All they know is that they use a simple interface with one method.
