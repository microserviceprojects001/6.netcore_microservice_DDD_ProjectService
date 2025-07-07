

namespace User.API.Models;

public class UserTag
{
    /// <summary>
    /// 用户Id
    /// </summary>
    public int UserId { get; set; }


    public string Tag { get; set; } = string.Empty;

    public DateTime CreateTime { get; set; }

}