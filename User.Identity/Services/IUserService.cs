using User.Identity.Dtos;

namespace User.Identity.Services;

public interface IUserService
{
    /// <summary>
    /// 检查手机号是否已注册吗，如果没有注册话就注册一个用户
    /// 手机号
    /// </summary>
    Task<UserInfo> CheckOrCreate(string phone);
}