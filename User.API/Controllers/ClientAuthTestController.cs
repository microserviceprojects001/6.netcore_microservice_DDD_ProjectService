// User.API/Controllers/ClientAuthTestController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

namespace User.API.Controllers
{
    [ApiController]
    [Route("api/test/client-auth")]
    [Authorize("user_api")]
    public class ClientAuthTestController : ControllerBase
    {
        [HttpGet("public")]
        public IActionResult PublicEndpoint()
        {
            return Ok(new
            {

                Message = "这是一个公共端点，不需要认证",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("protected")]
        [Authorize] // 需要任何有效的认证
        public IActionResult ProtectedEndpoint()
        {
            var clientId = User.FindFirstValue("client_id");
            var scopes = User.FindAll("scope").Select(c => c.Value).ToList();

            return Ok(new
            {
                Message = "这是一个受保护的端点，需要有效的客户端认证",
                ClientId = clientId,
                Scopes = scopes,
                Timestamp = DateTime.UtcNow,
                Claims = User.Claims.Select(c => new { c.Type, c.Value })
            });
        }

        [HttpGet("user-api-only")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult UserApiOnlyEndpoint()
        {
            // 这个端点需要 audience 为 user_api 的令牌
            var clientId = User.FindFirstValue("client_id");
            var scopes = User.FindAll("scope").Select(c => c.Value).ToList();

            return Ok(new
            {
                Message = "这个端点需要 audience 为 user_api 的令牌",
                ClientId = clientId,
                Scopes = scopes,
                Timestamp = DateTime.UtcNow,
                IsUserApiAudience = User.HasClaim("aud", "user_api")
            });
        }

        [HttpGet("require-user-api-scope")]
        [Authorize(Policy = "RequireInternalAccess")]
        public IActionResult RequireUserApiScopeEndpoint()
        {
            var clientId = User.FindFirstValue("client_id");

            return Ok(new
            {
                Message = "这个端点需要 user_api scope",
                ClientId = clientId,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}