namespace User.Identity.Services;

public class UserService : IUserService
{
    public int CheckOrCreate(string phone)
    {
        // 模拟检查手机号是否已注册，如果没有注册就创建一个用户
        // 实际项目中应替换为真实的数据库操作
        if (string.IsNullOrEmpty(phone))
        {
            return 0; // 返回0表示用户创建失败
        }

        // 模拟返回一个用户ID
        return new Random().Next(1, 1000); // 返回1到999之间的随机用户ID
    }
}