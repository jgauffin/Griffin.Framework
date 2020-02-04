using System.Threading.Tasks;

namespace Griffin.Net.Protocols.Http.Results
{
    public interface IHttpResultInvoker<in TResult> where TResult : HttpResult
    {
        Task InvokeAsync(HttpResultInvokerContext context, TResult result);
    }
}