using System.Text.Json;
using WebshareProxyServicePoC.Models;
using System.Text;
using System.Net.Http.Headers;
using Microsoft.Extensions.Caching.Memory;

namespace WebshareProxyServicePoC.Services
{
    public class ProxyService : IProxyService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly string _apiToken;

        public ProxyService(IHttpClientFactory httpClientFactory, IMemoryCache cache, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
            _apiToken = configuration["Webshare:ApiToken"] ?? throw new InvalidOperationException("Webshare API Token is not configured.");
        }

        private HttpClient CreateHttpClient()
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://proxy.webshare.io/api/v2/");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", _apiToken);
            return client;
        }

        public async Task<ProxyListResponse> ListProxies(int page = 1, int pageSize = 25)
        {
            return await _cache.GetOrCreateAsync($"ProxyList_{page}_{pageSize}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                var client = CreateHttpClient();
                var response = await client.GetAsync($"proxy/list/?mode=direct&page={page}&page_size={pageSize}");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                };

                return JsonSerializer.Deserialize<ProxyListResponse>(content, options) ?? new ProxyListResponse();
            }) ?? new ProxyListResponse();
        }

        public async Task<List<Proxy>> GetAllProxies()
        {
            var allProxies = new List<Proxy>();
            var page = 1;
            const int pageSize = 100;

            while (true)
            {
                var response = await ListProxies(page, pageSize);
                if (response.Results == null || !response.Results.Any())
                {
                    break;
                }
                allProxies.AddRange(response.Results);

                if (string.IsNullOrEmpty(response.Next))
                {
                    break;
                }

                page++;
            }

            return allProxies;
        }

        public async Task<bool> RefreshProxyList()
        {
            var client = CreateHttpClient();
            var response = await client.PostAsync("proxy/list/refresh/", null);

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return true;
            }

            response.EnsureSuccessStatusCode();
            return false;
        }

        public async Task<ProxyReplacementResponse> ReplaceProxies(ProxyReplacementRequest request)
        {
            var client = CreateHttpClient();
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await client.PostAsync("proxy/replace/", content);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };

            return JsonSerializer.Deserialize<ProxyReplacementResponse>(responseContent, options) 
                   ?? throw new InvalidOperationException("Failed to deserialize the proxy replacement response.");
        }
    }
}