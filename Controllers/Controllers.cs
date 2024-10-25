using Microsoft.AspNetCore.Mvc;

namespace realnofake.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2,0")]
[ApiVersion("3.0")]
public class TestController : ControllerBase
{
    public TestController()
    {
        
    }

    [HttpGet("testMessage")]
    public string Test() 
    {
        return "Test Controller";
    }
}