Unit Of Work
===============

With databases, unit of work is a way to wrap a transaction without adding
hard dependencies against the data source.

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
    var conString = ConfigurationManager
                    .ConnectionStrings("MyDb")
                    .ConnectionString;
    var con = new SqlConnection(conString);
    con.Open();

    //true = owns connection
    return new AdoNetUnitOfWork(con, true); 
}
```
