using System.Net;
using System.Net.Http.Headers;
using WebshareProxyServicePoC.Models;
using Microsoft.Extensions.Caching.Memory;

namespace WebshareProxyServicePoC.Services
{
    public class ProxyRequestExecutor : IProxyRequestExecutor
    {
        private readonly IProxyService _proxyService;
        private readonly IMemoryCache _cache;
        private readonly string _apiToken;
        private readonly string _username;
        private readonly string _password;

        public ProxyRequestExecutor(IProxyService proxyService, IMemoryCache cache, IConfiguration configuration)
        {
            _proxyService = proxyService;
            _cache = cache;
            _apiToken = configuration["Webshare:ApiToken"] ?? throw new InvalidOperationException("Webshare API Token is not configured.");
            _username = configuration["ProxySettings:Username"] ?? throw new InvalidOperationException("Webshare username is not configured.");
            _password = configuration["ProxySettings:Password"] ?? throw new InvalidOperationException("Webshare password is not configured.");
        }

        public async Task<ExecuteRequestResponse> ExecuteRequestViaProxy(string url, string methodString, string accountId)
        {
            var proxyListResponse = await _proxyService.ListProxies();
            if (proxyListResponse.Results == null || !proxyListResponse.Results.Any())
            {
                throw new InvalidOperationException("No proxies available.");
            }

            var usedProxies = _cache.GetOrCreate($"UsedProxies_{accountId}", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return new HashSet<string>();
            }) ?? new HashSet<string>();

            var selectedProxy = proxyListResponse.Results.FirstOrDefault(proxy => proxy?.ProxyAddress != null && !usedProxies.Contains(proxy.ProxyAddress));
            if (selectedProxy == null || selectedProxy.ProxyAddress == null)
            {
                throw new InvalidOperationException("No unused proxies available.");
            }

            usedProxies.Add(selectedProxy.ProxyAddress);
            _cache.Set($"UsedProxies_{accountId}", usedProxies);
            Console.WriteLine($"Used proxies for account {accountId}:");
            foreach (var proxy in usedProxies)
            {
                Console.WriteLine($"- {proxy}");
            }

            var proxyIp = selectedProxy.ProxyAddress;
            var proxyPort = selectedProxy.Port;
            Console.WriteLine(proxyIp);
            Console.WriteLine(proxyPort);
            var handler = new HttpClientHandler
            {
                Proxy = new WebProxy(proxyIp, proxyPort)
                {
                    Credentials = new NetworkCredential(_username, _password)
                },
                UseProxy = true
            };

            using var httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", _apiToken);

            var method = new HttpMethod(methodString);
            var request = new HttpRequestMessage(method, url);

            try
            {
                var response = await httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                return new ExecuteRequestResponse
                {
                    StatusCode = (int)response.StatusCode,
                    Headers = response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value)),
                    Content = content
                };
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP-related exceptions
                return new ExecuteRequestResponse
                {
                    StatusCode = 500,
                    Headers = new Dictionary<string, string> { { "Error", ex.Message } },
                    Content = string.Empty
                };
            }
        }
    }
}