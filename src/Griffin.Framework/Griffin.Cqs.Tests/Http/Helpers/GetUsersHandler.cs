using System.Threading.Tasks;
using DotNetCqs;

namespace Griffin.Cqs.Tests.Http.Helpers
{
    public class GetUsersHandler : IQueryHandler<GetUsers, GetUsersResult>
    {
        public async Task<GetUsersResult> ExecuteAsync(GetUsers command)
        {
            return new GetUsersResult() { Count = 10 };
        }
    }
}