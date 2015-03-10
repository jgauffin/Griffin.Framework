using DotNetCqs;
using Griffin.Cqs.Authorization;

namespace HttpCqs.Contracts
{
    //[Authorize("Admin")]
    public class GetUsers : Query<GetUsersResult>
    {
    }
}