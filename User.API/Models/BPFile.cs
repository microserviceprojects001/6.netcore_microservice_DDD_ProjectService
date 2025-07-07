

namespace User.API.Models;

public class BPFile
{
    /// <summary>
    /// BP id
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// 用户Id
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// 文件名称
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 上传的源文件地址
    /// </summary>
    public string OriginalFilePath { get; set; } = string.Empty;

    /// <summary>
    /// 格式转化后的文件地址
    /// </summary>
    public string FromatFilePath { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }

}