namespace Griffin.Cqs.Net.Authentication
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPasswordHasher
    {
        string CreateSalt();
        string HashPassword(string password, string salt);
        bool Compare(string hashedPassword1, string hashedPassword2);
    }
}