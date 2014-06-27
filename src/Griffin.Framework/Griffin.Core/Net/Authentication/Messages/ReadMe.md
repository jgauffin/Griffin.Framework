Messages
===========

These messages are the ones that are transferred between the client/server to be able to authenticate.

The authentication process works like this:

1. Client connects
2. The client sends an [IClientPreAuthentication](IClientPreAuthentication.cs) message to the server.
3. The server loads the specified user and returns a [IServerPreAuthentication](IServerPreAuthentication.cs) implementation to the client
4. The client hashes the password with the AccountSalt and then hashes the result with the SessionSalt.
5. The client sends back the generated hash to the server using [IClientAuthentication](IClientAuthentication.cs).
6. The server takes the hashed password from the [IUserAccount](../IUserAccount.cs) and hashes it again with the SessionSalt
7. Finally the server compares the two hashes and returns a [IAuthenticationResult](IAuthenticationResult.cs) to the client

The authentication is now completed.

Do note that the `IAuthenticationResult` can be returned earlier if the user is not found or if something failed in the server during the authentication process.



