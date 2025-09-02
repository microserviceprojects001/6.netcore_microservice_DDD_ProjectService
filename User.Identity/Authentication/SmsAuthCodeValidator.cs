// SmsAuthCodeValidator.cs
using Duende.IdentityServer.Validation;
using System.Security.Claims;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using User.Identity.Services;

public class SmsAuthCodeValidator : IExtensionGrantValidator
{
    private readonly IAuthCodeService _authCodeService;
    private readonly IUserService _userService;

    public SmsAuthCodeValidator(IAuthCodeService authCodeService, IUserService userService)
    {
        _authCodeService = authCodeService;
        _userService = userService;
    }


    public string GrantType => "sms_code"; // 自定义授权类型标识

    public async Task ValidateAsync(ExtensionGrantValidationContext context)
    {
        // 1. 从请求获取参数
        var phone = context.Request.Raw.Get("phone");
        var code = context.Request.Raw.Get("code");

        // 2. 简单验证逻辑
        if (string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(code))
        {
            context.Result = new GrantValidationResult(
                TokenRequestErrors.InvalidRequest,
                "手机号和验证码不能为空");
            return;
        }

        // 3. 检查验证码是否正确
        if (!_authCodeService.Validate(phone, code)) // 实际项目替换为真实验证
        {
            context.Result = new GrantValidationResult(
                TokenRequestErrors.InvalidGrant,
                "验证码错误");
            return;
        }

        // 4. 检查手机号是否已注册，如果没有注册就创建一个用户
        var userInfo = await _userService.CheckOrCreate(phone);
        if (userInfo == null)
        {
            context.Result = new GrantValidationResult(
                            TokenRequestErrors.InvalidGrant,
                            "验证用户错误");
            return;
        }
        // claims 只加到这里返回还不行，还不能加入到token里面去，需要定义ProfileService
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, userInfo.Name ?? string.Empty), //这里不能是null 赋值
            new Claim("name", userInfo.Name ?? string.Empty),
            new Claim("company", userInfo.Company ?? string.Empty),
            new Claim("title", userInfo.Title ?? string.Empty),
            new Claim("avatar", userInfo.Avatar ?? string.Empty),
        };
        context.Result = new GrantValidationResult(
            subject: userInfo.Id.ToString(),
            authenticationMethod: GrantType,
            claims: claims
        );
    }
}