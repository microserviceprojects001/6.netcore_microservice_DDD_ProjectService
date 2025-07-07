namespace User.API.Dtos;

public class UserIdentity
{
    /// <summary>
    /// 用户Id
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string Name { get; set; } = string.Empty;


    /// <summary>
    /// 头像地址
    /// </summary>
    public string Avatar { get; set; } = string.Empty;
}