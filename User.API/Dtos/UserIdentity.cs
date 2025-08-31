namespace User.API.Dtos;

public class UserIdentity
{
    /// <summary>
    /// 用户Id
    /// </summary>
    public int UserId { get; set; }

    public string Name { get; set; }

    /// <summary>
    /// 公司
    /// </summary>
    public string Company { get; set; }

    /// <summary>
    /// 职位
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 头像
    /// </summary>
    public string Avatar { get; set; }
}