using System.Net;
using OxylabsPoC.Models;
using Polly;
using Polly.Timeout;
using OxylabsPoC.Exceptions;

namespace OxylabsPoC.Services
{
    public class ProxyService : IProxyService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProxyService> _logger;
        private readonly AsyncPolicy<HttpResponseMessage> _retryPolicy;
        
        public ProxyService(IConfiguration configuration, ILogger<ProxyService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _retryPolicy = Policy.WrapAsync(
                Policy.TimeoutAsync<HttpResponseMessage>(15), // 15 second timeout
                Policy<HttpResponseMessage>
                    .Handle<HttpRequestException>()
                    .Or<TimeoutRejectedException>()
                    .WaitAndRetryAsync(
                        3, // Retry 3 times
                        retryAttempt => TimeSpan.FromSeconds(15),
                        onRetry: (outcome, timespan, retryAttempt, context) =>
                        {
                            _logger.LogWarning($"Delaying for {timespan.TotalSeconds} seconds, then making retry {retryAttempt}");
                        }
                    )
            );
        }

        public async Task<ExecuteRequestResponse> ExecuteRequestViaProxy(string url, string methodString, int accountId)
        {
            var proxyUsername = _configuration["ProxySettings:Username"];
            var proxyPassword = _configuration["ProxySettings:Password"];
            var proxyAddress = "ddc.oxylabs.io";
            var proxyPort = 8000;

            if (string.IsNullOrEmpty(proxyUsername) || string.IsNullOrEmpty(proxyPassword))
            {
                throw new InvalidOperationException("Proxy credentials are not configured.");
            }

            var proxy = new WebProxy
            {
                Address = new Uri($"http://{proxyAddress}:{proxyPort}"),
                Credentials = new NetworkCredential(proxyUsername, proxyPassword)
            };

            var httpClientHandler = new HttpClientHandler { Proxy = proxy };

            using var client = new HttpClient(handler: httpClientHandler, disposeHandler: true);

            try
            {
                _logger.LogInformation($"Sending {methodString} request to {url} via proxy {proxyAddress}:{proxyPort}");
                var response = await _retryPolicy.ExecuteAsync(async () =>
                {
                    var method = new HttpMethod(methodString);
                    var request = new HttpRequestMessage(method, url);
                    return await client.SendAsync(request);
                });
                _logger.LogInformation($"Received response with status code {response.StatusCode}");

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
                _logger.LogError(ex, $"HTTP request error occurred while sending request to {url}");
                throw new ProxyExecutionException("Error occurred during proxy execution", ex);
            }
            catch (TimeoutRejectedException ex)
            {
                _logger.LogError(ex, $"Request timed out while sending request to {url}");
                throw new ProxyExecutionException("Request timed out", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error occurred while sending request to {url}");
                throw new ProxyExecutionException("Unexpected error occurred during proxy execution", ex);
            }
        }

        public Task<List<ProxyList>> GetAllProxies()
        {
            // This method is not applicable for self-service dedicated datacenter proxies
            throw new NotImplementedException("GetAllProxies is not supported for dedicated datacenter proxies.");
        }

        public Task<List<Proxy>> GetAllProxiesDetails()
        {
            // This method is not applicable for self-service dedicated datacenter proxies
            throw new NotImplementedException(
                "GetAllProxiesDetails is not supported for dedicated datacenter proxies.");
        }

    }
}