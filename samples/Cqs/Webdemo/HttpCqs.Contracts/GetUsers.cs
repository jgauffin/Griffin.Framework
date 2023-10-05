using DotNetCqs;

namespace HttpCqs.Contracts
{
    //[Authorize("Admin")]
    public class GetUsers : Query<GetUsersResult>
    {
    }
}