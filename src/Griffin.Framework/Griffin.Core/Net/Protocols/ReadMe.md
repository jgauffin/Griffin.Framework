# Protocols

The protocols namespace are used to define encoders/decoders used to transport application level messages.

We have a couple of predefined protocols:

**name** | **description**
HTTP | A complete HTTP server implementation (i.e. the request encoder and response decoder is not completed)
MicroMsg | Our own efficient protocol which takes care of transporting messages (making sure that everything is received at the other site). It will also attach type information making it easier to deserialize the body at the other end.
Stomp | A message queue protocol which is a more lightweight alternative to AMQ. Supports transactions and acknowledgemens for safe messaging

