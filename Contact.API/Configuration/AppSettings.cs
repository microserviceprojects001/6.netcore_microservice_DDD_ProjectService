
namespace Contact.API.Configuration;

public class AppSettings
{
    /// <summary>
    /// MongoDB连接字符串
    /// </summary>
    public string MongoContactConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// MongoDB数据库名称
    /// </summary>
    public string MongoContactDatabaseName { get; set; } = string.Empty;
}