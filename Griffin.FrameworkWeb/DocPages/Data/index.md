Data extensions
============

Extensions for ADO.NET to make it easier to work with queries and SQL commands.

# Adding parameters

As the `IDbCommand` interface do not provide a simple way to insert arguments into a command we've added an small extension method for that:

```csharp
using (var cmd = connection.CreateCommand())
{
    cmd.CommandText = "SELECT * FROM Users WHERE Name = @name";
    cmd.AddParameter("name", "Arne Nilsson");
}
```

# More info

* [Data mapper](mapper) - A complete mapping layer
* [Unit Of Work](unitofwork) - An ADO.NET Unit Of Work implementation.