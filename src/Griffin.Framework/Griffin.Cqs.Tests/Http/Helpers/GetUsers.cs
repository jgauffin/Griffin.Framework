using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetCqs;

namespace Griffin.Cqs.Tests.Http.Helpers
{
    public class GetUsers : Query<GetUsersResult>
    {
        public string FirstName { get; set; }
    }
}
