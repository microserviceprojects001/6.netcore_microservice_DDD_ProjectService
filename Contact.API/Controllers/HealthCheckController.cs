using Microsoft.AspNetCore.Mvc;

namespace Contact.API.Controllers;

[ApiController]

public class HealthCheckController : ControllerBase
{
    [HttpGet("HealthCheck")]
    public IActionResult Get() => Ok("Healthy");
}