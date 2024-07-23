using Microsoft.AspNetCore.Mvc;
using WebshareProxyServicePoC.Services;
using WebshareProxyServicePoC.Models;

namespace WebshareProxyServicePoC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProxyController : ControllerBase
    {
        private readonly IProxyService _proxyService;
        private readonly IProxyRequestExecutor _proxyRequestExecutor;

        public ProxyController(IProxyService proxyService, IProxyRequestExecutor proxyRequestExecutor)
        {
            _proxyService = proxyService;
            _proxyRequestExecutor = proxyRequestExecutor;
        }

        [HttpPost("execute")]
        public async Task<IActionResult> ExecuteRequest([FromBody] ProxyRequest request)
        {
            if (string.IsNullOrEmpty(request.Url) || string.IsNullOrEmpty(request.Method) || string.IsNullOrEmpty(request.AccountId))
            {
                return BadRequest("Url, Method, and AccountId are required.");
            }
            var response = await _proxyRequestExecutor.ExecuteRequestViaProxy(request.Url, request.Method, request.AccountId);
            return Ok(response);
        }

        [HttpGet("list")]
        public async Task<IActionResult> ListProxies([FromQuery] int page = 1, [FromQuery] int pageSize = 25)
        {
            var result = await _proxyService.ListProxies(page, pageSize);
            return Ok(result);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllProxies()
        {
            var result = await _proxyService.GetAllProxies();
            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshProxyList()
        {
            try
            {
                var result = await _proxyService.RefreshProxyList();
                if (result)
                {
                    return NoContent(); // 204 No Content
                }
                else
                {
                    return StatusCode(500, "Proxy list refresh failed");
                }
            }
            catch (HttpRequestException ex)
            {
                // Log the exception
                return StatusCode(500, $"An error occurred while refreshing the proxy list: {ex.Message}");
            }
        }

        [HttpPost("replace")]
        public async Task<IActionResult> ReplaceProxies([FromBody] ProxyReplacementRequest request)
        {
            try
            {
                var result = await _proxyService.ReplaceProxies(request);
                return Ok(result);
            }
            catch (HttpRequestException ex)
            {
                // Log the exception
                return StatusCode(500, $"An error occurred while replacing proxies: {ex.Message}");
            }
        }
    }

    public class ProxyRequest
    {
        public string? Url { get; set; }
        public string? Method { get; set; }
        public string? AccountId { get; set; }
    }
}