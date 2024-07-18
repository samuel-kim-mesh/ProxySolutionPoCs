using System.Text.Json;
using WebshareProxyServicePoC.Models;
using System.Text;
using System.Net.Http.Headers;
using System.Net;

namespace WebshareProxyServicePoC.Services
{
    public class ProxyService : IProxyService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiToken;
        private readonly string _username;
        private readonly string _password;

        public ProxyService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiToken = configuration["Webshare:ApiToken"] ?? throw new InvalidOperationException("Webshare API Token is not configured.");
            _httpClient.BaseAddress = new Uri("https://proxy.webshare.io/api/v2/");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Token {_apiToken}");
            _username = configuration["ProxySettings:Username"] ?? throw new InvalidOperationException("Webshare username is not configured.");
            _password = configuration["ProxySettings:Password"] ?? throw new InvalidOperationException("Webshare password is not configured.");
        }

        public async Task<ExecuteRequestResponse> ExecuteRequestViaProxy(string url, string methodString)
        {
            var proxyListResponse = await ListProxies();
            if (proxyListResponse.Results == null || !proxyListResponse.Results.Any())
            {
                throw new InvalidOperationException("No proxies available.");
            }

            // Select the first proxy from the list
            var selectedProxy = proxyListResponse.Results.First();
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
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Token {_apiToken}");

            var method = new HttpMethod(methodString);
            var request = new HttpRequestMessage(method, url);

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine(content);
            return new ExecuteRequestResponse
            {
                StatusCode = (int)response.StatusCode,
                Headers = response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value)),
                Content = content
            };
        }
        public async Task<ProxyListResponse> ListProxies(int page = 1, int pageSize = 25)
        {
            // mode direct must always be specified
            var response = await _httpClient.GetAsync($"proxy/list/?mode=direct&page={page}&page_size={pageSize}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };

            return JsonSerializer.Deserialize<ProxyListResponse>(content, options) 
                   ?? new ProxyListResponse();
        }
        public async Task<List<Proxy>> GetAllProxies()
        {
            var allProxies = new List<Proxy>();
            var page = 1;
            const int pageSize = 100; // Increase page size to reduce number of requests

            while (true)
            {
                var response = await ListProxies(page, pageSize);
                allProxies.AddRange(response.Results);

                if (string.IsNullOrEmpty(response.Next))
                {
                    break; // No more pages
                }

                page++;
            }

            return allProxies;
        }
        // this only works if we have on_demand_refreshes_available
        // our plan currently has 0
        // this *should* refresh the entire proxy list
        public async Task<bool> RefreshProxyList()
        {
            var response = await _httpClient.PostAsync("proxy/list/refresh/", null);
        
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return true;
            }
        
            // If the status code is not 204 No Content, we'll throw an exception
            response.EnsureSuccessStatusCode();
        
            // This line should never be reached if the API behaves as expected,
            // but we'll return false just in case
            return false;
        }
        public async Task<ProxyReplacementResponse> ReplaceProxies(ProxyReplacementRequest request)
        {
            // this time need to get request first
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _httpClient.PostAsync("proxy/replace/", content);

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