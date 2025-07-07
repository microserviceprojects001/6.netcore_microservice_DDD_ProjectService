using Microsoft.EntityFrameworkCore;
using User.API.Data;
using User.API.Models;

namespace User.API.Data;

public class UserContextSeed
{
    // UpdateUserPoints(1);       // 输出：跳过积分更新（null）
    // UpdateUserPoints(1, null); // 输出：跳过积分更新
    // UpdateUserPoints(1, 0);    // 输出：积分已清零
    // UpdateUserPoints(1, 100);  // 输出：积分更新为100
    public static void UpdateUserPoints(int userId, int? points = null)
    {
        if (!points.HasValue)
        {
            Console.WriteLine("跳过积分更新");
            return;
        }

        // 明确处理 points=0 的情况
        //_db.Execute($"UPDATE Users SET Points = {points} WHERE Id = {userId}");
        Console.WriteLine(points == 0 ? "积分已清零" : $"积分更新为{points}");
    }

    private static void EnsureDatabaseCreated(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbcontext = scope.ServiceProvider.GetRequiredService<UserContext>();
        // 替换 EnsureCreated() 为：
        dbcontext.Database.Migrate(); // 应用所有未执行的迁移
        //dbcontext.Database.EnsureCreated();
        if (!dbcontext.AppUsers.Any())
        {
            dbcontext.AppUsers.Add(new AppUser
            {
                Name = "John Doe",
                Company = "Example Corp",
                Title = "Software Engineer",
                Email = "John@email.com",
                Address = "Dalian, China",
                Avatar = "default.png", // ✅ 添加这行
                Phone = "1234567890",   // ✅ 这些字段也是非空的，建议全部加上
                Gender = 1,
                Tel = "0411-12345678",
                Province = "Liaoning",
                ProvinceId = 21,
                City = "Dalian",
                CityId = 2102,
                Properties = new List<UserProperty>(),
                Age = 111
            });
            dbcontext.SaveChanges();
        }
    }
    public static void Seed(WebApplication app, int retry = 0)
    {
        var retryCount = retry;
        try
        {
            // 确保数据库已创建并应用迁移
            EnsureDatabaseCreated(app);
            Console.WriteLine("数据库已成功初始化。");
        }
        catch (Exception ex)
        {
            Thread.Sleep(3000); // 等待2秒后重试
            retryCount++;
            if (retryCount < 10)
            {
                Console.WriteLine($"数据库初始化失败，正在重试... {retryCount}");
                Seed(app, retryCount);
            }
            else
            {
                Console.WriteLine("数据库初始化失败，超过最大重试次数。");
                throw;
            }
        }

    }
}