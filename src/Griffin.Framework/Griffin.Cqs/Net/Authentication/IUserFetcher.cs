using System.Threading.Tasks;

namespace Griffin.Cqs.Net.Authentication
{
    /// <summary>
    /// Used to fetch accounts from your data source.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The implementation will be treated as a single instance
    /// </para>
    /// </remarks>
    public interface IUserFetcher
    {
        Task<IUserAccount> GetAsync(string userName);
        Task<string[]> GetRolesAsync(IUserAccount account);
    }
}