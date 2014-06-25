namespace Griffin.Cqs.Net.Authentication
{
    /// <summary>
    /// 
    /// </summary>
    public interface IUserAccount
    {
        string UserName { get; set; }
        string HashedPassword { get; set; }
        string PasswordSalt { get; set; }

    }
}