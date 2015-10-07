using System.Threading.Tasks;
using DotNetCqs;
using Griffin.Container;
using HttpCqs.Contracts;

namespace HttpCqs.Server.App
{
    [ContainerService]
    class GetUsersHandler : IQueryHandler<GetUsers, GetUsersResult>
    {
        public Task<GetUsersResult> ExecuteAsync(GetUsers query)
        {
            return Task.FromResult(new GetUsersResult()
            {
                Items = new[]
                {
                    new GetUsersResultItem
                    {
                        Id = 1,
                        UserName = "arne"
                    }
                }
            });
        }
    }
}
