Inversion Of Control container support
========================================

This namespace gives you two different features.

# Abstraction

Allows Griffin.Framework to use your favorite container as long as you implement two interfaces.

[Read more](Abstraction)

# Registration

Our container adapter packages (nuget) also include a simplified 
way of
 registering your classes in your favorite IoC container.

Simply add the `[ContainerService]` attribute to your classes and then register them using the `RegisterServices()` extension method.


## Example

```csharp
// Add this attribute.
[ContainerService]
public class SomeService
{
}
```

You can also specify lifetime:

```csharp
// Add this attribute.
[ContainerService(ContainerLifetime.SingleInstance)]
public class SomeService
{
}
```


## Registration

To register all classes in your container you need to invoke the
`RegisterServices`  method for every assembly. The classes will
be registered as self and all implemented interfaces.

### Example

Example for Autofac (requires the nuget package `griffin.framework.autofac`).

```csharp
var cb = new ContainerBuilder();
cb.RegisterServices(typeof(SomeService).Assembly);
var autofac = cb.Build();

// the adapter which can be used by Griffin.Framework
var container = new AutofacAdapter(autofac);
```

Do not get fooled by the `typeof(SomeService).Assembly`. 
What it says is that all classes that exist in the same ***assembly*** as the `SomeService` class should get registered. 
It doesn't necessarily register `SomeService` (it will if that class has the `[ContainerService]` attribute).