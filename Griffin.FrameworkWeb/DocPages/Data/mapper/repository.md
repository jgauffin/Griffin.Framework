Repository pattern sample
==========================

Here is how a simple repository pattern implementation would look like.

## Without transactions

The repository:

```csharp
public class MyRepository
{
	IDbConnection _connection;
		 
	// You can also use the IDbTransaction
	public MyRepository(IDbConnection connection)
	{
		if (connection == null) throw new ArgumentNullException("connection");
		_connection = connection;
	}
		
	public void Create(User user)
	{
		if (user == null) throw new ArgumentNullException("user");

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
```

## With transactions

```csharp
public class MyRepository
{
	IAdoNetUnitOfWork _unitOfWork;
		 
	// You can also use the IDbTransaction
	public MyRepository(IAdoNetUnitOfWork unitOfWork)
	{
		if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");
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
```

Usage:

```csharp
using (var uow = UnitOfWorkFactory.Create())
{
  //TODO: to be completed
}

```