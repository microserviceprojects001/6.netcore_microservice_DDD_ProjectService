
namespace User.Identity.Services;

public interface IAuthCodeService
{

    /// <summary>
    /// 根据手机号验证验证码
    /// 手机号
    /// 验证码
    /// </summary>

    bool Validate(string phone, string authcode);
}