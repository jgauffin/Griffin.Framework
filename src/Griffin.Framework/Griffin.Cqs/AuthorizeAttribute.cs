using System;

namespace Griffin.Cqs
{
    public class AuthorizeAttribute : Attribute
    {
        public AuthorizeAttribute(params string[] roles)
        {
            Roles = roles;
        }

        public string[] Roles { get; private set; }
    }
}