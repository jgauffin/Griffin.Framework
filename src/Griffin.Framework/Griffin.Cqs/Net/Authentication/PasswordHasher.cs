using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.Cqs.Net.Authentication
{
    /// <summary>
    /// Uses RNGCryptoServiceProvider to generate the salt and Rfc2898DeriveBytes for hashing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Uses the class defined here: https://crackstation.net/hashing-security.htm#aspsourcecode
    /// </para>
    /// </remarks>
    public class PasswordHasher : IPasswordHasher
    {
        public string CreateSalt()
        {
            var salt = new byte[24];
            using (var csprng = new RNGCryptoServiceProvider())
            {
                csprng.GetBytes(salt);
            }
            return Convert.ToBase64String(salt);
        }

        public string HashPassword(string password, string salt)
        {
            var hashWithSalt = password + ":" + salt;
            var saltBytes = Encoding.UTF8.GetBytes(hashWithSalt);
            using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, saltBytes, 1000))
                return Convert.ToBase64String(rfc2898DeriveBytes.GetBytes(256));
        }

        public bool Compare(string hashedPassword1, string hashedPassword2)
        {
            var hash1 = Convert.FromBase64String(hashedPassword1);
            var hash2 = Convert.FromBase64String(hashedPassword2);
            return SlowEquals(hash1, hash2);
        }

        /// <summary>
        /// Compares two byte arrays in length-constant time. This comparison
        /// method is used so that password hashes cannot be extracted from
        /// on-line systems using a timing attack and then attacked off-line.
        /// </summary>
        /// <param name="a">The first byte array.</param>
        /// <param name="b">The second byte array.</param>
        /// <returns>True if both byte arrays are equal. False otherwise.</returns>
        /// <remarks>
        /// Credits: https://crackstation.net/hashing-security.htm#aspsourcecode
        /// </remarks>
        private static bool SlowEquals(byte[] a, byte[] b)
        {
            uint diff = (uint)a.Length ^ (uint)b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
                diff |= (uint)(a[i] ^ b[i]);
            return diff == 0;
        }
    }
}
