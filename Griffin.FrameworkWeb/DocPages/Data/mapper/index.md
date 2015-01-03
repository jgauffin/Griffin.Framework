Data mapper
=====================

A lightweight data mapper which extends ADO.NET instead of hiding it. Supports both
synchronous and asynchronous methods. The mapper also features detailed exception messages when an error happens so that you easily can
understand what went wrong.

Do note that this is a mapping layer and not a OR/M. You still have to write your
SQL queries, but with a simpler API. You can for instance write:

```charp
var users = connection.ToList<User>("firstName LIKE @1 AND lastName LIKE @2", firstName, lastName);
```

The mapper is based on [mapping files](mappings) where are provided to the mapping
layer with the help of the `EntityMappingProvider` class. The default implementation
(`AssemblyScanningMappingProvider`) searches all loaded assemblies after mappings.

# Getting started

1. To get started you need to create mappings as described [here](mappings).
2. Create a Unit Of Work, we have a [unit of work factory](../unitofwork).
3. Use one of the [API methods](api)

There is also a repository pattern [sample](repository).

# Database support

* [Sqlite](Sqlite) (install the nuget package GriffinFramework.Sqlite)
* SQL Server (built in)

# More info

* [API)(api) / [Async api](async_api)
* [Mappings](mappings)
* [Repository](repository)

# Links

* [Article](http://blog.gauffin.org/2014/02/introducing-the-data-mapper-in-griffin-framework/)
* [Sample project](https://github.com/jgauffin/Griffin.Framework/tree/master/src/Examples/Data/Sqlite)
