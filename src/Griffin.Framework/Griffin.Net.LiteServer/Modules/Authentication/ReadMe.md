Authentication module
===========================

This module uses a three-step authentication process (described in the bottom of this document). To use it you need to store
your users together with a salt and a hashed password.

The built in hasing process is described in the default [hash implementation](PasswordHasher.cs). If you hash your passwords in another
way you need to write your own implementation of [IPasswordHasher](IPasswordHasher.cs) and assign it to the [AuthenticationModule](AuthenticationModule.cs).

Once authenticated, the correct `IPrincipal` are set on the current thread every time a new message is loaded. Per default `GenericPrincipal` is used.

# Get started (server side)

Users are loaded from your datasource using the [IUserFetcher](IUserFetcher.cs) interface. That means that the first thing you need to
do is to write an implementation of that contract. 

Once done create a new instance of the authentication module:

```csharp
var module = new AuthenticationModule(new YourUserFetcher());
```

Then registering the authentication module in the lite server configuration:

```csharp
var config = new LiteServerConfiguration();
config.Modules.AddAuthentication(module);

// add your own module here (which process what ever you are working on)
//[...]
```

Finally create the server and you are ready to go:

```csharp
var server = new LiteServer(config);
server.Start();
```

## Options

There are a few options that you can customize to get the authentication process to work per your requirements.

### Principal

If you do not want to use `GenericPrincipal` you can load a custom one by setting `authModule.PrincipalFactory = new YourCustomFactory();` to your own implementation.

Look at the source code of [IPrincipalFactory](IPrincipalFactory.cs) for a sample implementation.

## Custom messages

The built in messages are tagged with `[Serializable]` and `[DataContract]`/`[DataMember]` attributes. But if that is not enough for your own 
serializer you can specify your own message implementations using `authModule.AuthenticationMessageFactory = new YourCustomFactory()`;

Look at [AuthenticationMessageFactory](AuthenticationMessageFactory.cs) for a sample implementation.

## Password hashing

If you are not storing passwords using the same hashes as we you need to create your own password hasher. You do that by
implementing [IPasswordHasher](IPasswordHasher.cs).

If you are building your own project you can use our hasher directly when your users are being registered. Look at our default implementation [PasswordHasher](PasswordHasher.cs).

# Authentication process

This authentication module uses the authentication mechanism [described here](http://stackoverflow.com/q/12254710/70386).

![Login process image](http://i.stack.imgur.com/cS3Fc.png)

The actual messages have been abstracted away and being created by a factory so that the can be adapted for different types of transport protocols.
