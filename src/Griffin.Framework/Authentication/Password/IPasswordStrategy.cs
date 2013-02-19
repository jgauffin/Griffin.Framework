using System;

namespace Griffin.Framework.Authentication.Password
{
    /// <summary>
    /// Use to handle passwoprds
    /// </summary>
    public interface IPasswordStrategy
    {
        /// <summary>
        /// Validate password during login
        /// </summary>
        /// <param name="context">Password information. The context depends on which strategy is used</param>
        /// <returns><c>true</c> if passwords match; otherwise false;</returns>
        bool Compare(ComparePasswordContext context);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        void Encode(IPasswordEncodeContext context);
    }

    public interface IPasswordEncodeContext
    {
        /// <summary>
        /// Gets password as specified by the user
        /// </summary>
        string ClearTextPassword { get; }

        /// <summary>
        /// Gets or sets the encoded password
        /// </summary>
        string EncodedPassword { get; set; }
    }

    public class SaltedPasswordEncodeContext : IPasswordEncodeContext
    {
        public SaltedPasswordEncodeContext(string clearTextPassword, string salt)
        {
            if (clearTextPassword == null) throw new ArgumentNullException("clearTextPassword");
            if (salt == null) throw new ArgumentNullException("salt");

            ClearTextPassword = clearTextPassword;
            Salt = salt;
        }

        /// <summary>
        /// Gets password as specified by the user
        /// </summary>
        public string ClearTextPassword { get; private set; }

        /// <summary>
        /// Gets password salt
        /// </summary>
        public string Salt { get; private set; }

        /// <summary>
        /// Set the encoded password
        /// </summary>
        public string EncodedPassword { get; set; }
    }

    public interface IPasswordDecodeContext
    {
        
    }
}