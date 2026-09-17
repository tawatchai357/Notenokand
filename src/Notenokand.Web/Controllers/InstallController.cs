using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Notenokand.Web.Controllers;

[AllowAnonymous]
[Route("install")]
public sealed class InstallController : Controller
{
    [HttpGet("")]
    public IActionResult Index() => View();
}
