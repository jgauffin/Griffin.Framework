Sqlite
============

Sqlite support.

# Installation

Download the nuget package `griffinframework.sqlite` and activate it:

```csharp
CommandBuilderFactory.Assign(mapper => new SqliteCommandBuilder(mapper));
```

# Example

To start with you have a business entity somewhere:

```csharp
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
```

.. which you create a mapper for ..

```csharp
public class UserMapper : EntityMapper<User>
{
    private static readonly DateTime UnixDate = new DateTime(1970, 1, 1);
 
    public UserMapper() : base("Users")
    {
    }
 
    public override void Configure(IDictionary<string, IPropertyMapping> mappings)
    {
        base.Configure(mappings);
        mappings["Id"].ColumnToPropertyAdapter = i => Convert.ToInt32(i);
        mappings["CreatedAtUtc"].ColumnToPropertyAdapter = o => UnixDate.AddSeconds(Convert.ToInt32(o));
        mappings["CreatedAtUtc"].PropertyToColumnAdapter = o => ((DateTime) o).Subtract(UnixDate).TotalSeconds;
    }
}
```

As you can see, the mapper supports conversions between column and property types. You could even store child aggregates as JSON in a column if you would like.

The mapping is discovered automatically by the library (thanks to the AssemblyScanningMappingProvider).

# Usage

```csharp
class Program
{
    static void Main(string[] args)
    {
        CommandBuilderFactory.Assign(mapper => new SqliteCommandBuilder(mapper));
 
        string cs = "URI=file:test.db";
        var connection = new SQLiteConnection(cs);
        connection.Open();
 
        if (!connection.TableExists("Users"))
        {
            using (var uow = new AdoNetUnitOfWork(connection))
            {
                //no need to create a DbCommand
                uow.Execute(
                    "CREATE TABLE Users (Id INTEGER PRIMARY KEY AUTOINCREMENT, FirstName TEXT, LastName text, CreatedAtUtc INTEGER)");
 
                uow.SaveChanges();
            }
        }
 
        //converts to a "SELECT * FROM Users WHERE FirstName LIKE @FirstName"
        var users = connection.ToList<User>(new {FirstName = "Gau%"});
 
        // converts to "SELECT * FROM Users WHERE Id = @id"
        var first = connection.First<User>(new {Id = 1});
 
 
 
        using (var uow = new AdoNetUnitOfWork(connection))
        {
            using (var cmd = uow.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Users";
                cmd.AddParameter("id", "983498043903");
 
                // ToEnumerable doesn't map until row is read = better performance
                foreach (var entity in cmd.ToEnumerable<User>())
                {
                    Console.WriteLine(entity.FirstName);
                }
 
 
            }
 
            //truncate using the correct DB command
            uow.Truncate<User>();
 
            for (int i = 0; i < 100; i++)
            {
                //simple insert.
                uow.Insert(new User { FirstName = "Arne" + i });
            }
 
            uow.SaveChanges();
        }
 
 
    }
}
```
