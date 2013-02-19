namespace Griffin.Framework.Authentication.Password
{
    /// <summary>
    /// The password can also be decoded (decrypted)
    /// </summary>
    public interface IDecodablePasswordStrategy : IPasswordStrategy
    {
        void Decode(IPasswordDecodeContext context);
    }
}