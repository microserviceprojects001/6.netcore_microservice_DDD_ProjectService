using Microsoft.AspNetCore.Mvc;
using Contact.API.Dtos;
using System.Security.Claims;

namespace Contact.API.Controllers;

public class BaseController : ControllerBase
{
    protected UserIdentity UserIdentity
    {
        get
        {
            // 直接从 User.Claims 中获取信息
            var userIdClaim = User.FindFirst("sub") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            var nameClaim = User.FindFirst("name") ?? User.FindFirst(ClaimTypes.Name);
            var companyClaim = User.FindFirst("company");
            var titleClaim = User.FindFirst("title");
            var avatarClaim = User.FindFirst("avatar");

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return new UserIdentity
                {
                    UserId = userId,
                    Name = nameClaim?.Value ?? string.Empty,
                    Company = companyClaim?.Value ?? string.Empty,
                    Title = titleClaim?.Value ?? string.Empty,
                    Avatar = avatarClaim?.Value ?? string.Empty
                };
            }

            // 开发环境回退（仅在无法从 token 获取信息时使用）
            return new UserIdentity
            {
                UserId = 1,
                Name = "jesse",
                Company = "测试公司",
                Title = "测试职位",
                Avatar = "https://example.com/avatar.jpg"
            };
        }
    }
}