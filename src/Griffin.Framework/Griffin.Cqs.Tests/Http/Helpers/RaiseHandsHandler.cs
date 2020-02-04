using System.Threading.Tasks;
using DotNetCqs;

namespace Griffin.Cqs.Tests.Http.Helpers
{
    public class RaiseHandsHandler : ICommandHandler<RaiseHands>
    {
        public async Task ExecuteAsync(RaiseHands command)
        {

        }
    }
}