using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetCqs;
using Griffin.Container;
using HttpCqs.Contracts;

namespace HttpCqs.Server.App
{
    [ContainerService]
    class GetUsersHandler : IQueryHandler<GetUsers, GetUsersResult>
    {
        public async Task<GetUsersResult> ExecuteAsync(GetUsers query)
        {
            return new GetUsersResult()
            {
                Items = new[]
            {
                new GetUsersResultItem
                {
                    Id = 1,
                    UserName = "arne"
                }
            }
            };
        }
    }
}
