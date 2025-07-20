namespace User.Identity.Services;

public class TestAuthCodeService : IAuthCodeService
{
    public bool Validate(string phone, string authcode)
    {
        // 模拟验证逻辑，实际项目中应替换为真实的验证码验证逻辑
        return true;
        //!string.IsNullOrEmpty(phone) && !string.IsNullOrEmpty(authcode) && authcode == "123456";
    }
}