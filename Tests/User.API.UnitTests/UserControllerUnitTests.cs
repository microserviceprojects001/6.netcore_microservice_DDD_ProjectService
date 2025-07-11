using Moq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using User.API.Data;
using User.API.Models;
using FluentAssertions;
using Microsoft.AspNetCore.JsonPatch;

namespace User.API.UnitTests;

public class UserControllerUnitTests
{

    private Data.UserContext GetUserContext()
    {
        var options = new DbContextOptionsBuilder<Data.UserContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var UserContext = new Data.UserContext(options);
        UserContext.AppUsers.Add(new Models.AppUser
        {
            Name = "John",
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

    private (Controllers.UserController controller, Data.UserContext userContext) GetUserController()
    {
        var context = GetUserContext();
        var loggerMoq = new Mock<ILogger<Controllers.UserController>>();
        var controller = new Controllers.UserController(context, loggerMoq.Object);
        return (controller: controller, userContext: context);
    }

    [Fact]
    public async Task Get_ReturnRightUser_WithExpectedParameter()
    {
        // Console.WriteLine("等待调试器附加...");
        // await Task.Delay(30000); // 等待30秒
        // System.Diagnostics.Debugger.Launch();
        // System.Diagnostics.Debugger.Break();


        // Console.WriteLine("测试方法开始执行");
        // Console.WriteLine($"当前进程ID: {System.Diagnostics.Process.GetCurrentProcess().Id}");

        (Controllers.UserController controller, Data.UserContext userContext) = GetUserController();

        var response = await controller.Get();
        Console.WriteLine($"response is: {response}");
        //Assert.IsType(typeof(OkObjectResult), response);

        var result = response.Should().BeOfType<OkObjectResult>().Subject;
        var user = result.Value.Should().BeAssignableTo<Models.AppUser>().Subject;
        user.Id.Should().Be(1);
        user.Name.Should().Be("John");
    }

    [Fact]
    public async Task Patch_ReturnNewNname_WithExpectedNewNameParameter()
    {

        (Controllers.UserController controller, Data.UserContext userContext) = GetUserController();
        var document = new JsonPatchDocument<Models.AppUser>();
        document.Replace(u => u.Name, "NewName");
        var response = await controller.Patch(document);

        var result = response.Should().BeOfType<OkObjectResult>().Subject;

        //assert response
        var user = result.Value.Should().BeAssignableTo<Models.AppUser>().Subject;
        user.Name.Should().Be("NewName");

        // assert name value in ef context
        var userModel = userContext.AppUsers.SingleOrDefault(u => u.Id == 1);
        userModel.Should().NotBeNull();
        userModel.Name.Should().Be("NewName");
    }

    [Fact]
    public async Task Patch_ReturnNewProperties_WithExpectedNewProperties()
    {

        (Controllers.UserController controller, Data.UserContext userContext) = GetUserController();
        var document = new JsonPatchDocument<Models.AppUser>();
        document.Replace(u => u.Name, "NewName");
        var response = await controller.Patch(document);

        var result = response.Should().BeOfType<OkObjectResult>().Subject;

        //assert response
        var user = result.Value.Should().BeAssignableTo<Models.AppUser>().Subject;
        user.Name.Should().Be("NewName");

        // assert name value in ef context
        var userModel = userContext.AppUsers.SingleOrDefault(u => u.Id == 1);
        userModel.Should().NotBeNull();
        userModel.Name.Should().Be("NewName");
    }

    [Fact]
    public async Task Patch_ReturnNewProperties_WithAddNewProperty()
    {
        (Controllers.UserController controller, Data.UserContext userContext) = GetUserController();
        var document = new JsonPatchDocument<Models.AppUser>();
        document.Replace(u => u.Properties, new List<Models.UserProperty>
        {
            new Models.UserProperty { Key = "fin_industry", Value = "互联网", Text = "互联网" }
        });
        var response = await controller.Patch(document);

        var result = response.Should().BeOfType<OkObjectResult>().Subject;

        //assert response
        var user = result.Value.Should().BeAssignableTo<Models.AppUser>().Subject;
        user.Properties.Count.Should().Be(1);
        user.Properties.First().Value.Should().Be("互联网");
        user.Properties.First().Key.Should().Be("fin_industry");

        // assert name value in ef context
        var userModel = userContext.AppUsers.SingleOrDefault(u => u.Id == 1);
        userModel.Properties.Count.Should().Be(1);
        userModel.Properties.First().Value.Should().Be("互联网");
        userModel.Properties.First().Key.Should().Be("fin_industry");
    }

    [Fact]
    public async Task Patch_ReturnNewProperties_WithRemoveProperty()
    {

        (Controllers.UserController controller, Data.UserContext userContext) = GetUserController();
        var document = new JsonPatchDocument<Models.AppUser>();
        document.Replace(u => u.Properties, new List<Models.UserProperty>
        {

        });
        var response = await controller.Patch(document);

        var result = response.Should().BeOfType<OkObjectResult>().Subject;

        //assert response
        var user = result.Value.Should().BeAssignableTo<Models.AppUser>().Subject;
        user.Properties.Should().BeEmpty();

        // assert name value in ef context
        var userModel = userContext.AppUsers.SingleOrDefault(u => u.Id == 1);
        userModel.Properties.Should().BeEmpty();
    }
}
