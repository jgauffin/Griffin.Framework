using System;

namespace Griffin.Framework.Authentication.Password.Strategies
{
    public class DigestAuthenticationStrategy : IPasswordStrategy
    {
        /// <summary>
        /// Validate password during login
        /// </summary>
        /// <param name="context">Password information. The context depends on which strategy is used</param>
        /// <returns><c>true</c> if passwords match; otherwise false;</returns>
        public bool Compare(ComparePasswordContext context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void Encode(IPasswordEncodeContext context)
        {
            throw new NotImplementedException();
        }
    }
}
