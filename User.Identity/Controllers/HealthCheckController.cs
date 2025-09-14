using Microsoft.AspNetCore.Mvc;

namespace User.Identity.Controllers;

[ApiController]

public class HealthCheckController : ControllerBase
{
    [HttpGet("HealthCheck")]
    public IActionResult Get() => Ok("Healthy");
}