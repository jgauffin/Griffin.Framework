Griffin.Framework
=================

Griffin.Framework is an open source library built using the experiences from all my previous libraries. I will merge all other libraries into this one. 


#Stable parts

Libraries which have been completed.

## Griffin.Data

A data mapper which works as an extension to ADO.NET. That gives you full control while not having to take care of mappings or CRUD operations.

Example repository:

	public class MyRepository
	{
		IDbConnection _connection;
		 
		// You can also use the IDbTransaction
		public MyRepository(IDbConnection connection)
		{
			if (connection == null) throw new ArgumentNullException();
			_connection = connection;
		}
		
		public void Create(User user)
		{
			_connection.Insert(user);
			//todo: Get identity. Depends on the db engine.
		}
	 
	 
		public void Update(User user)
		{
			_connection.Update(user);
		}
	 
		public void Delete(int id)
		{
			// can be any field or the actual entity
			_connection.Delete<User>(new { Id = id });
		}
		
		public User GetUser(int id)
		{
			return _connection.First<User>(new { id });
		}
		
		public IEnumerable<User> FindUsers()
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = @"SELECT * FROM Users WHERE CompanyId = @companyId AND FirstName LIKE @firstName";
				command.AddParameter("companyId", LoggedInUser.companyId);
				command.AddParameter("firstName", firstName + "%");
				
				//skip through the first 1000 rows without mapping them and then just map the next 10 rows.
				return command.ToEnumerable<User>().Skip(1000).Take(10).ToList();
			}
		}
	}

Full article: http://blog.gauffin.org/2014/02/introducing-the-data-mapper-in-griffin-framework/

## Griffin.Networking

High performance networking layer which makes it very easy to allow .NET applications to communicate.

Supports HTTP, STOMP and our own MicroMsg transport protocol.

**Sample server**

	var settings = new ChannelTcpListenerConfiguration(
		() => new MicroMessageDecoder(new ProtoBufSerializer()),
		() => new MicroMessageEncoder(new ProtoBufSerializer())
		);
	 
	var server = new MicroMessageTcpListener(settings);
	server.MessageReceived = OnServerMessage;
	 
	server.Start(IPAddress.Any, 1234);

	private static void OnServerMessage(ITcpChannel channel, object message)
	{
		var auth = (Authenticate) message;
		channel.Send(new AuthenticateReply() {Success = true});
	}

**Sample client**

	private static async Task RunClient()
	{
		var client = new ChannelTcpClient<object>(
			new MicroMessageEncoder(new ProtoBufSerializer()),
			new MicroMessageDecoder(new ProtoBufSerializer())
			);
	 
		await client.ConnectAsync(IPAddress.Parse("192.168.1.3"), 1234);
	 
		await client.SendAsync(new Authenticate { UserName = "jonas", Password = "king123" });
		var reply = (AuthenticateReply)await client.ReceiveAsync();
	 
		if (reply.Success)
		{
			Console.WriteLine("Client: Yay, we are logged in.");
		}
		else
		{
			Console.WriteLine("Client: " + reply.Decision);
		}
	 
		await client.CloseAsync();
	}
	
Full article: http://blog.gauffin.org/2014/06/easy-and-perfomant-clientserver-communication-with-protobuf-net-griffin-framework/

# What's next?

Next up is a complete rewrite of Griffin.Decoupled, which will be a Command/Query library with support for interprocess communication and IoC containers.

Commercial support is available through Gauffin Interactive AB: support@gauffin.com.
Profiling tools provided by JetBrains (DotTrace)
