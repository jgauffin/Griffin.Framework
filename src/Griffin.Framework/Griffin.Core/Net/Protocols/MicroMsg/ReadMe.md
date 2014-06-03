# MicroMsg

MicroMsg is our own format which uses a tiny header before the actual message.

## Header

The header consists of:

* *Header length* `short` Number of bytes that are for the header. First counted byte is directly after this field<br><br>
* *Version* `byte` Defines the version of the micro protocol. Current version is 1<br><br>
* *Content Length* `int` Defines the length of the body. The body starts directly after the header<br><br>
* *Type length* `sbyte` Defines the length of the next header value<br><br>
* *Type name* `string` Fully qualified assembly name of the type that is our payload (UTF8 encoded)<br><br>

## Body

The format of the body is up to the implementor. It's however recommended that the *type name* includes the format. 

For instance:

    "protobuf;DemoApp.Models.UserDTO,DemoApp"
