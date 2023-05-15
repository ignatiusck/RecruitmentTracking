using Microsoft.AspNetCore.Mvc;
using MainLogger;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly Log4net<AdminController> _logger = new();

    [HttpGet]
    [Route("/admin/{id}")]
    public IEnumerable<AdminController> GetAdmin()
    {
        return default!;
    }

    // [HttpPost]
    // [Route("/addJob")]
    // public 
}
