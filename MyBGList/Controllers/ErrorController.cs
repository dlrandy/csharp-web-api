using Microsoft.AspNetCore.Mvc;

namespace MyBGList.Controllers;

[ApiController]
public class ErrorController : ControllerBase
{
    // 在这里是controller处理错误和特定的路由，会覆盖到中间件的处理方式
    [Route("/error1")]
    [HttpGet]
    public IActionResult Error() {
        return Problem();
    }

    [Route("/error/test")]
    [HttpGet]
    public IActionResult Test() {
        throw new Exception("TEst");
    }

}

