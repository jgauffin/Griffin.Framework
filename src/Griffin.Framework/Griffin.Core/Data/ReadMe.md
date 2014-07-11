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

# Unit of work

The library contains an `IUnitOfWork` which also is specialized into `IAdoNetUnitOfWork` to allow you to work with ADO.NET while abstracting away that detail from your business layer.

You can do this in your business layer:

```csharp
using (var uow = UnitOfWorkFactory.Create())
{
    var repos = new AccountRepository(uow);
    repos.Insert(new User(/*....*/));

    uow.SaveChanges();
}
```

While this is done in your data layer:

```csharp
public class AccountRepository
{
    IAdoNetUnitOfWork _unitOfWork;
    
    public AccountRepository(IUnitOfWork uow)
    {
        //Violation of Liskovs Substituation Principle in theory,
        // but will unlikely causing and problems in reality since you probabaly wont
        // change data storage without going through the data layer in detail.
        _unitOfWork = (IAdoNetUnitOfWork)uow;
    }
    
    public void Insert(Account account)
    {
        //will automatically enlist the command in the current transaction
        using (var cmd = _unitOfWork.CreateCommand())
        {
            cmd.CommandText = ".....";
            cmd.AddParameter("id", account.Id);
            // [...]
            
            cmd.ExecuteNonQuery();
        }
    }
}
```

To make it work you need to configure the `UnitOfWorkFactory`:

```csharp
public static void Main(string[] argv)
{
    UnitOfWorkFactory.SetFactoryMethod(CreateUow);

}

public IUnitOfWork Create()
{
    var conString = ConfigurationManager.ConnectionStrings("MyDb").ConnectionString;
    var con = new SqlConnection(conString);
    con.Open();
    return new AdoNetUnitOfWork(con, true); //true = owns connection
}
```

# Data mapper

There are also a lightweight data mapper in the [Mapper](Mapper/) folder.

