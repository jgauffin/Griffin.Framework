Data mapper
=====================

A lightweight data mapper which extends ADO.NET instead of hiding it.

Read about the mappings [here](Docs/Mappings.md).

# Example repository:

```csharp
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
```
	
# Full article

http://blog.gauffin.org/2014/02/introducing-the-data-mapper-in-griffin-framework/

# Sample project

Can be found [here](https://github.com/jgauffin/Griffin.Framework/tree/master/src/Examples/Data/Sqlite)

