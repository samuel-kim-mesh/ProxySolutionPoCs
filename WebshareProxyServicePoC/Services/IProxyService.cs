using WebshareProxyServicePoC.Models;

namespace WebshareProxyServicePoC.Services
{
    public interface IProxyService
    {
        Task<ExecuteRequestResponse> ExecuteRequestViaProxy(string url, string methodString);
        Task<ProxyListResponse> ListProxies(int page = 1, int pageSize = 25);
        Task<List<Proxy>> GetAllProxies();
        Task<bool> RefreshProxyList();
        Task<ProxyReplacementResponse> ReplaceProxies(ProxyReplacementRequest request);

    }
}