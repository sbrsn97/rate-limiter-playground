using Microsoft.AspNetCore.Mvc;

namespace RateLimiterPlayground.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
           Message = "Request allowed.",
           Time = DateTime.UtcNow 
        });
    }
}