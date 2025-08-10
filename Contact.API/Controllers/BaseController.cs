using Microsoft.AspNetCore.Mvc;

using Contact.API.Dtos;

namespace Contact.API.Controllers;

public class BaseController : ControllerBase
{
    protected UserIdentity UserIdentity =>
        new UserIdentity
        {
            UserId = 1,
            Name = "jesse"
        };

}