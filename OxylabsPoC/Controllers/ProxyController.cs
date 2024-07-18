using Microsoft.AspNetCore.Mvc;
using OxylabsPoC.Models;
using OxylabsPoC.Services;
using OxylabsPoC.Exceptions;

namespace OxylabsPoC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProxyController : ControllerBase
    {
        private readonly IProxyService _proxyService;
        private readonly ILogger<ProxyController> _logger;

        public ProxyController(IProxyService proxyService, ILogger<ProxyController> logger)
        {
            _proxyService = proxyService;
            _logger = logger;
        }
        // only execute works
        [HttpPost("execute")]
        public async Task<IActionResult> ExecuteRequest([FromBody] ExecuteRequestModel model)
        {
            try
            {
                var result = await _proxyService.ExecuteRequestViaProxy(model.Url, model.Method, model.AccountId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ProxyExecutionException ex)
            {
                _logger.LogError(ex, "Proxy execution failed");
                return StatusCode(500, "An error occurred during proxy execution");
            }
        }
        // not being used since not on non enterprise
        [HttpGet("all")]
        public async Task<IActionResult> GetAllProxies()
        {
            try
            {
                var proxies = await _proxyService.GetAllProxies();
                return Ok(proxies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching proxies");
                return StatusCode(500, "An error occurred while fetching proxies");
            }
        }
    }
}