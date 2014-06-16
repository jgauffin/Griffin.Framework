# Inversion Of Control container abstraction

Allows Griffin.Framework to use your favorite container as long as you implement [IContainer](IContainer.cs) and [IContainerScope](IContainerScope.cs). Read the embedded documentation for more information.

Our container adapter packages (nuget) also include a simplified way of register your classes in the container.

Simply add the `[ContainerService]` attribute to your classes and then register them using the `RegisterServices()` extension method.


## example

```csharp
[ContainerService]
public class SomeService
{
}
```

### Registration

Example for Autofac (requires the nuget package `griffin.framework.autofac`).

```csharp
var cb = new ContainerBuilder();
cb.RegisterServices(typeof(SomeService).Assembly);

var container = cb.Build();

// the adapter which can be used by Griffin.Framework
var griffinAdapter = new AutofacContainer(container);
```

Do not get fooled by the `typeof(SomeService).Assembly`. 
What it says is that all classes that exist in the same ***assembly*** as the `SomeService` class should get registered. 
It doesn't necessarily register `SomeService` (it will if that class has the `[ContainerService]` attribute).
