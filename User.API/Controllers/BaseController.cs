using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using User.API.Dtos;
namespace User.API.Controllers;

public class BaseController : ControllerBase
{
    protected UserIdentity UserIdentity =>
        new UserIdentity
        {
            UserId = 1,
            Name = "jesse"
        };

}