# Simple bus

This bus assumes that all CQS handlers have a default constructor.

It will simply create the handler, execute the command and then get rid of the class. It will also invoke `Dispose()` if the class implements `IDisposable`.

## Sample handler


```csharp
using System;
using System.Threading.Tasks;
using DotNetCqs;

namespace Griffin.Cqs.Demo.Command
{
    public class IncreaseDiscountHandler : ICommandHandler<IncreaseDiscount>, IDisposable
    {
        public async Task ExecuteAsync(IncreaseDiscount command)
        {
            if (command.Percent == 1)
                throw new Exception("Must increase with at least two percent, cheap bastard!");

            Console.WriteLine("Being executed");
        }

        public void Dispose()
        {
            Console.WriteLine("Being disposed");
        }
    }
}
```
