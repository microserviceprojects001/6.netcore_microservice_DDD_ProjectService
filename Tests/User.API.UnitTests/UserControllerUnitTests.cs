using Moq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using User.API.Data;
using User.API.Models;

namespace User.API.UnitTests;

public class UserControllerUnitTests
{

    private Data.UserContext GetUserContext()
    {
        var options = new DbContextOptionsBuilder<Data.UserContext>()
            .UseInMemoryDatabase(new Guid().ToString())
            .Options;

        var UserContext = new Data.UserContext(options);
        UserContext.AppUsers.Add(new Models.AppUser
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
        UserContext.SaveChanges();
        return UserContext;
    }

    [Fact]
    public async Task Get_ReturnRightUser_WithExpectedParameter()
    {
        var context = GetUserContext();
        var loggerMoq = new Mock<ILogger<Controllers.UserController>>();
        var controller = new Controllers.UserController(context, loggerMoq.Object);
        var response = await controller.Get();
        Console.WriteLine($"response is: {response}");
        Assert.IsType(typeof(OkObjectResult), response);

    }


}
