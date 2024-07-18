using OxylabsPoC.Models;

namespace OxylabsPoC.Services
{
    public interface IProxyService
    { 
        Task<ExecuteRequestResponse> ExecuteRequestViaProxy(string url, string methodString, int accountId);
        Task<List<ProxyList>> GetAllProxies();
        Task<List<Proxy>> GetAllProxiesDetails();
    }
}