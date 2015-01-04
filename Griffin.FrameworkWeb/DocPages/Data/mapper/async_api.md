Async methods
==================

This library have full support for asynchronous operations in the data layer. 

All async operations 
require that a `DbCommand` subclass is available. 
Hence you cannot use `IDbCommand` for these methods. 
This restriction is 
enforced by .NET and not by this library (there are no interfaces for the async methods, they are defined directly in `DbCommand`).

Most ADO.NET drivers is however based in the `DbCommand` base class and should therefore work fine.


[CRUD](#CRUD) | [First & FirstOrDefault](#FIRST) | [ToEnumerable & ToList](TOENUMERABLE)

<a name="CRUD"/>
# CRUD operations

CRUD operations are provided both for `IDbConnection`, `IAdoNetUnitOfWork` and `DbCommand`. All CRUD operations
require that you have defined a `ICrudEntityMapper<T>` for your entity. For more information read the [Mappings](Mappings.md) page.

## INSERT

We do currently not populate the key property in your class automatically. We do however return the ID from the query.

### Insert without a transaction

If you do not require a transaction you can perform an insert directly on a connection:

```csharp
var user = new User(){FirstName = "Jonas" };
await connection.InsertAsync(user);
```

### Insert with a transaction

`IAdoNetUnitOfWork` represents a transaction in this library. Two know how to configure it, check the [complete sample](#CompleteSample) in the bottom of this document.

```csharp
using (var uow = UnitOfWorkFactory.Create())
{
    var user = new User(){FirstName = "Jonas" };
    var id = await uow.InsertAsync(user);
    user.Id = (int)id;
}
```

## UPDATE

Updates will use your defined keys in the WHERE clause. Make sure that you've configured them using `IsPrimaryKey = true` in the mapping.

### Update without a transaction

If you do not require a transaction you can perform an update directly on a connection:

```csharp
var user = new User(){ Id = 20, FirstName = "Jonas" };
await connection.UpdateAsync(user);
```

### Update with a transaction

`IAdoNetUnitOfWork` represents a transaction in this library. Two know how to configure it, check the [complete sample](#CompleteSample) in the bottom of this document.

```csharp
using (var uow = UnitOfWorkFactory.Create())
{
    var user = new User(){ Id = 20, FirstName = "Jonas" };
    await uow.UpdateAsync(user);
}
```

## DELETE

Deletes can be done both on the connection and the unit of work, just as Inserts and Updates. You can however use alternative syntaxes:

### Complete entity

If you've already fetched the entity you can use it in the delete command:

```csharp
await _unitOfWork.DeleteAsync(user);
```

You can also specify just the key in the entity:

```csharp
await _unitOfWork.DeleteAsync(new User { Id = userId });
```

### Anonymous object

You can use an anonymous object (names must be same as the property names, and the value must be of the same type as defined in the properties):

```csharp
await _unitOfWork.DeleteAsync<User>(new { Id = userId });
```

...which is translated into `"DELETE FROM Users WHERE id = @id"`. 

You can specify multiple columns:

```csharp
await _unitOfWork.DeleteAsync<User>(new { FirstName = firstName, LastName = lastName });
```

...which is translated into `"DELETE FROM Users WHERE FirstName = @firstName AND LastName = @lastName"`. 


### Short query:

Short queries allows you to only specify the WHERE statement and to include the parameters directly.

```csharp
await _unitOfWork.DeleteAsync<User>("expires < @date", new { id = minDate ));
```

You can use a value array:

```csharp
await _unitOfWork.DeleteAsync<User>("expires < @1 AND state = @2", minDate, UserState.ActivationRequired);
```

### Complete query

You can also write complete queries:

```csharp
await _unitOfWork.DeleteAsync<User>("DELETE FROM Users WHERE expires < @date", new { id = minDate ));
```

You can use a value array:

```csharp
await _unitOfWork.DeleteAsync<User>("DELETE FROM Users WHERE expires < @1 AND state = @2", minDate, UserState.ActivationRequired);
```

<a name="FIRST" />
#First & FirstOrDefault

Sometimes you want to fetch a single item. These methods are specialized for that. The methods works for `IDbConnection`, `IAdoNetUnitOfWork` and `DbCommand`.

## First

First requires that the entity is found. It throws a developer friendly exception if it's not (including the entity type, the parameters and even the command sql).

### Using an anonymous object

Contains are used to provide a clause which will be translated into a WHERE clause with equals ANDs.

```csharp
// SELECT * FROM Users WHERE id = @userId;
var user = await _unitOfWork.FirstAsync<User>(new { id = userId });

// SELECT * FROM Users WHERE FirstName = @firstName AND LastName = @lastName;
var user = await _unitOfWork.FirstAsync<User>(new { FirstName = firstName, LastName = lastName });

// SELECT * FROM Users WHERE firstName = @firstName AND lastName = @lastName;
var user = await _unitOfWork.FirstAsync<User>(new { firstName, lastName });
```

### Using a short query

```csharp
// SELECT * FROM Users WHERE id = @userId;
var user = await _unitOfWork.FirstAsync<User>("id = @id", new { id = userId });

// SELECT * FROM Users WHERE id = @userId;
var user = await _unitOfWork.FirstAsync<User>("id = @1", userId);

// SELECT * FROM Users WHERE FirstName = @firstName AND LastName = @lastName;
var user = await _unitOfWork.FirstAsync<User>("FirstName = @firstName AND LastName = @lastName", { firstName, lastName });

// SELECT * FROM Users WHERE FirstName = @firstName AND LastName = @lastName;
var user = await _unitOfWork.FirstAsync<User>("FirstName = @1 AND LastName = @2", firstName, lastName);
```

### Using a complete query

These examples uses a custom mapping:

```csharp
public class AgeMapping : EntityMapper<Age>
{
    public AgeMapping
}
```

Which enables us to fetch just a subset:

```csharp
var user = await _unitOfWork.FirstAsync<Age>("SELECT Age FROM Users WHERE id = @id", new { id = userId });

// SELECT * FROM Users WHERE id = @userId;
var user = await _unitOfWork.FirstAsync<Age>("SELECT Age FROM Users WHERE id = @1", userId);

// SELECT * FROM Users WHERE FirstName = @firstName AND LastName = @lastName;
var user = await _unitOfWork.FirstAsync<Age>("SELECT Age FROM Users WHERE FirstName = @firstName AND LastName = @lastName", { firstName, lastName });

// SELECT * FROM Users WHERE FirstName = @firstName AND LastName = @lastName;
var user = await _unitOfWork.FirstAsync<Age>("SELECT Age FROM Users WHERE FirstName = @1 AND LastName = @2", firstName, lastName);
```

### FirstOrDefault

Works just like First, and the syntax is the same. The difference is just that `null` is returned if no rows are found.

#ToListAsync

ToList have the same API as `FirstAsync()`/`FirstOrDefaultAsync()`, but a list is returned instead. 

An empty list is returned if no rows are found, hence no need to check for `null`.

#ToEnumerableAsync

ToEnumerable is a lazy loaded version which do not map each row until it's requested by you.
