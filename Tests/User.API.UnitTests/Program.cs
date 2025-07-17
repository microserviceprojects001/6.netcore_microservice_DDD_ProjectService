// Tests/User.API.UnitTests/Program.cs
namespace User.API.UnitTests;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("测试方法开始执行");
        // 这个空方法是编译所需的入口点
        // 实际测试运行由 XUnit 框架处理
        new UserControllerUnitTests().Get_ReturnRightUser_WithExpectedParameter().GetAwaiter().GetResult();
    }
}