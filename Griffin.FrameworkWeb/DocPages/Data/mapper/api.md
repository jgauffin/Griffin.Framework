Mapper methods
==================

This library have full support for asynchronous operations in the data layer. 
To see them, [go here](async_api).


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
connection.Insert(user);
```

### Insert with a transaction

`IAdoNetUnitOfWork` represents a transaction in this library. Two know how to configure it, check the [complete sample](#CompleteSample) in the bottom of this document.

```csharp
using (var uow = UnitOfWorkFactory.Create())
{
    var user = new User(){FirstName = "Jonas" };
    var id = uow.Insert(user);
    user.Id = (int)id;
}
```

## UPDATE

Updates will use your defined keys in the WHERE clause. Make sure that you've configured them using `IsPrimaryKey = true` in the mapping.

### Update without a transaction

If you do not require a transaction you can perform an update directly on a connection:

```csharp
var user = new User(){ Id = 20, FirstName = "Jonas" };
connection.Update(user);
```

### Update with a transaction

`IAdoNetUnitOfWork` represents a transaction in this library. Two know how to configure it, check the [complete sample](#CompleteSample) in the bottom of this document.

```csharp
using (var uow = UnitOfWorkFactory.Create())
{
    var user = new User(){ Id = 20, FirstName = "Jonas" };
    uow.Update(user);
}
```

## DELETE

Deletes can be done both on the connection and the unit of work, just as Inserts and Updates. You can however use alternative syntaxes:

### Complete entity

If you've already fetched the entity you can use it in the delete command:

```csharp
_unitOfWork.Delete(user);
```

You can also specify just the key in the entity:

```csharp
_unitOfWork.Delete(new User { Id = userId });
```

### Anonymous object

You can use an anonymous object (names must be same as the property names, and the value must be of the same type as defined in the properties):

```csharp
_unitOfWork.Delete<User>(new { Id = userId });
```

...which is translated into `"DELETE FROM Users WHERE id = @id"`. 

You can specify multiple columns:

```csharp
_unitOfWork.Delete<User>(new { FirstName = firstName, LastName = lastName });
```

...which is translated into `"DELETE FROM Users WHERE FirstName = @firstName AND LastName = @lastName"`. 


### Short query:

Short queries allows you to only specify the WHERE statement and to include the parameters directly.

```csharp
_unitOfWork.Delete<User>("expires < @date", new { id = minDate ));
```

You can use a value array:

```csharp
_unitOfWork.Delete<User>("expires < @1 AND state = @2", minDate, UserState.ActivationRequired);
```

### Complete query

You can also write complete queries:

```csharp
_unitOfWork.Delete<User>("DELETE FROM Users WHERE expires < @date", new { id = minDate ));
```

You can use a value array:

```csharp
_unitOfWork.Delete<User>("DELETE FROM Users WHERE expires < @1 AND state = @2", minDate, UserState.ActivationRequired);
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
var user = _unitOfWork.First<User>(new { id = userId });

// SELECT * FROM Users WHERE FirstName = @firstName AND LastName = @lastName;
var user = _unitOfWork.First<User>(new { FirstName = firstName, LastName = lastName });

// SELECT * FROM Users WHERE firstName = @firstName AND lastName = @lastName;
var user = _unitOfWork.First<User>(new { firstName, lastName });
```

### Using a short query

```csharp
// SELECT * FROM Users WHERE id = @userId;
var user = _unitOfWork.First<User>("id = @id", new { id = userId });

// SELECT * FROM Users WHERE id = @userId;
var user = _unitOfWork.First<User>("id = @1", userId);

// SELECT * FROM Users WHERE FirstName = @firstName AND LastName = @lastName;
var user = _unitOfWork.First<User>("FirstName = @firstName AND LastName = @lastName", { firstName, lastName });

// SELECT * FROM Users WHERE FirstName = @firstName AND LastName = @lastName;
var user = _unitOfWork.First<User>("FirstName = @1 AND LastName = @2", firstName, lastName);
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
var user = _unitOfWork.First<Age>("SELECT Age FROM Users WHERE id = @id", new { id = userId });

// SELECT * FROM Users WHERE id = @userId;
var user = _unitOfWork.First<Age>("SELECT Age FROM Users WHERE id = @1", userId);

// SELECT * FROM Users WHERE FirstName = @firstName AND LastName = @lastName;
var user = _unitOfWork.First<Age>("SELECT Age FROM Users WHERE FirstName = @firstName AND LastName = @lastName", { firstName, lastName });

// SELECT * FROM Users WHERE FirstName = @firstName AND LastName = @lastName;
var user = _unitOfWork.First<Age>("SELECT Age FROM Users WHERE FirstName = @1 AND LastName = @2", firstName, lastName);
```

### FirstOrDefault

Works just like First, and the syntax is the same. The difference is just that `null` is returned if no rows are found.

#ToList

ToList have the same API as `First()`/`FirstOrDefault()`, but a list is returned instead. 

An empty list is returned if no rows are found, hence no need to check for `null`.

#ToEnumerable

ToEnumerable is a lazy loaded version which do not map each row until it's requested by you.
