Mappings
=============

Mappings in this library can be divided into two categories.

# Mappings

These classes are only used to map a result to a .NET class. They can be used to map a table, a custom query, or against a view. They are defined by the `IEntityMapper<T>` interface.

The most trivial example is to just create a new class and let it inherit from `EntityMapper<T>`:

```csharp
public class AccountMapper : EntityMapper<Account>
{
}
```

That requires that each property can be mapped against a column in the query and that the column types are the same as the property types.

If a column differ in type you can add a converter:

```csharp
public class AccountMapper : EntityMapper<Account>
{
    public override void Configure(IDictionary<string, IPropertyMapping> mappings)
    {
        base.Configure(mappings);
        mappings["AccountState"].ColumnToPropertyAdapter = o => (AccountStatus)Enum.Parse(typeof(AccountStatus), (string)o, true);
    }
}
```

If the column name is not the same you can specify the column name:

```csharp
public class AccountMapper : EntityMapper<Account>
{
    public override void Configure(IDictionary<string, IPropertyMapping> mappings)
    {
        base.Configure(mappings);
        mappings["AccountState"].ColumnToPropertyAdapter = o => (AccountStatus)Enum.Parse(typeof(AccountStatus), (string)o, true);
        mappings["SelectedOrganizationId"].ColumnName = "LastUsedOrganization";
    }
}
```

## Mapping child aggregates

I use the mappings to be able to store child aggregates in a column in the main table. If I've created a column called `ChatHistory` I could do the following to deserialize the history from JSON:

```csharp
public class ChatWindowMapper : EntityMapper<ChatWindow>
{
    public override void Configure(IDictionary<string, IPropertyMapping> mappings)
    {
        base.Configure(mappings);
        mappings["ChatHistory"].ColumnToPropertyAdapter = MapChatHistory;
    }
    
    public object MapChatHistory(object columnValue)
    {
        var json = (string)columnValue;
        return JsonConvert.DeserializeObject<ChatHistoryEntry[]>(json);
    }
}
```

## CRUD mappings

The mapper library supports CRUD operations. You can for instance insert items into the DB by using:

```csharp
unitOfWork.Insert(new ChatWindow(/*....*/));
```

To enable support for that you have to inherit from `CrudEntityMapper<T>` instead of `EntityMapper<T>`.

