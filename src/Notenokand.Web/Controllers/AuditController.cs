using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notenokand.Infrastructure.Persistence;
namespace Notenokand.Web.Controllers;

[Authorize, Route("app/audit")]
public sealed class AuditController(NotenokandDbContext db) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(int page = 1)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Forbid();
        var member = await db.AccountUsers.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId && x.IsActive && !x.IsDeleted);
        if (member is null || member.RoleName != "Owner") return Forbid();
        page = Math.Clamp(page, 1, 100000);
        var query = db.AuditLogs.AsNoTracking().Where(x => x.AccountId == member.AccountId);
        ViewData["Page"] = page; ViewData["HasNext"] = await query.CountAsync() > page * 50;
        return View(await query.OrderByDescending(x => x.Id).Skip((page - 1) * 50).Take(50).ToListAsync());
    }
}
