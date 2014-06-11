using System;
using System.Threading.Tasks;
using DotNetCqs;

namespace Griffin.Cqs.Demo.Command
{
    public class IncreaseDiscountHandler : ICommandHandler<IncreaseDiscount>, IDisposable
    {
        /// <summary>
        /// Execute a command asynchronously.
        /// </summary>
        /// <param name="command">Command to execute.</param>
        /// <returns>
        /// Task which will be completed once the command has been executed.
        /// </returns>
        public async Task ExecuteAsync(IncreaseDiscount command)
        {
            if (command.Percent == 1)
                throw new Exception("Must increase with at least two percent, cheap bastard!");

            Console.WriteLine("Being executed");
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Console.WriteLine("Being disposed");
        }
    }
}