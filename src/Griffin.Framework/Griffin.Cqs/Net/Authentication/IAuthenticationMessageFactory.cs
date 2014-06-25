namespace Griffin.Cqs.Net.Authentication
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAuthenticationMessageFactory
    {
        IClientPreAuthentication CreateClientPreAuthentication();
        IServerPreAuthentication CreateServerPreAuthentication(IUserAccount user);
        IClientAuthentication CreateClientAuthentication();
    }
}