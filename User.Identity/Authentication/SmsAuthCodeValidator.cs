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
        int userId = await _userService.CheckOrCreate(phone);
        if (userId <= 0)
        {
            context.Result = new GrantValidationResult(
                            TokenRequestErrors.InvalidGrant,
                            "验证用户错误");
            return;
        }

        context.Result = new GrantValidationResult(
            subject: userId.ToString(),
            authenticationMethod: GrantType,
            claims: new[] { new Claim(ClaimTypes.Name, phone) }
        );
    }
}