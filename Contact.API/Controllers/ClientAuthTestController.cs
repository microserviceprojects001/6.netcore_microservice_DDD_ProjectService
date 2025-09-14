// Contact.API/Controllers/ClientAuthTestController.cs
using Contact.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Contact.API.Controllers
{
    [ApiController]
    [Route("api/test/client-auth")]
    public class ClientAuthTestController : ControllerBase
    {
        private readonly IClientAuthTestService _testService;
        private readonly ILogger<ClientAuthTestController> _logger;

        public ClientAuthTestController(
            IClientAuthTestService testService,
            ILogger<ClientAuthTestController> logger)
        {
            _testService = testService;
            _logger = logger;
        }

        [HttpGet("token")]
        public async Task<IActionResult> TestTokenAcquisition()
        {
            _logger.LogInformation("Testing token acquisition...");
            var result = await _testService.TestTokenAcquisition();
            return Ok(result);
        }

        [HttpGet("public")]
        public async Task<IActionResult> TestPublicEndpoint()
        {
            _logger.LogInformation("Testing public endpoint...");
            var result = await _testService.TestPublicEndpoint();
            return Ok(result);
        }

        [HttpGet("protected")]
        public async Task<IActionResult> TestProtectedEndpoint()
        {
            _logger.LogInformation("Testing protected endpoint...");
            var result = await _testService.TestProtectedEndpoint();
            return Ok(result);
        }

        [HttpGet("user-api-only")]
        public async Task<IActionResult> TestUserApiOnlyEndpoint()
        {
            _logger.LogInformation("Testing user-api-only endpoint...");
            var result = await _testService.TestUserApiOnlyEndpoint();
            return Ok(result);
        }

        [HttpGet("require-scope")]
        public async Task<IActionResult> TestRequireUserApiScopeEndpoint()
        {
            _logger.LogInformation("Testing require-user-api-scope endpoint...");
            var result = await _testService.TestRequireUserApiScopeEndpoint();
            return Ok(result);
        }

        [HttpGet("all")]
        public async Task<IActionResult> TestAllEndpoints()
        {
            _logger.LogInformation("Testing all client auth endpoints...");

            var results = new
            {
                TokenTest = await _testService.TestTokenAcquisition(),
                PublicTest = await _testService.TestPublicEndpoint(),
                ProtectedTest = await _testService.TestProtectedEndpoint(),
                UserApiOnlyTest = await _testService.TestUserApiOnlyEndpoint(),
                RequireScopeTest = await _testService.TestRequireUserApiScopeEndpoint()
            };

            return Ok(results);
        }
    }
}