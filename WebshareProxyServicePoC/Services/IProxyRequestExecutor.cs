using WebshareProxyServicePoC.Models;

namespace WebshareProxyServicePoC.Services
{
    public interface IProxyRequestExecutor
    {
        Task<ExecuteRequestResponse> ExecuteRequestViaProxy(string url, string methodString, string accountId);
    }
}