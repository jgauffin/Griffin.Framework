namespace Griffin.Security
{
    /// <summary>
    ///     Used to hash passwords allowing different parts of Griffin.Framework to reuse the hashing to provide features like
    ///     authentication.
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        ///     Compare two hashes.
        /// </summary>
        /// <param name="hashedPassword1">First hash</param>
        /// <param name="hashedPassword2">Second hash</param>
        /// <returns>
        ///     Allows us to make sure that all comparisons take about the same time so that the comparison is not exited
        ///     early if the hashes differ in the beginning.
        /// </returns>
        bool Compare(string hashedPassword1, string hashedPassword2);

        /// <summary>
        ///     Generate a salt that can be used to hash passwords.
        /// </summary>
        /// <returns>Generated salt</returns>
        string CreateSalt();

        /// <summary>
        ///     Hash password using the specified salt
        /// </summary>
        /// <param name="password"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        /// <remarks>
        ///     <para>
        ///         Implementations should hashed passwords by using colon as separator.
        ///         <code>Hash(string.Format("{0}:{1}", password, salt))</code>
        ///     </para>
        /// </remarks>
        string HashPassword(string password, string salt);
    }
}