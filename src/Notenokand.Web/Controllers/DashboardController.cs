using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notenokand.Infrastructure.Identity;
using Notenokand.Infrastructure.Persistence;
using Notenokand.Web.Models.Account;

namespace Notenokand.Web.Controllers;

[Authorize]
[Route("app")]
public sealed class DashboardController(NotenokandDbContext db, UserManager<ApplicationUser> userManager) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
            return Challenge();

        var membership = await db.AccountUsers
            .AsNoTracking()
            .Include(x => x.Account)
            .FirstOrDefaultAsync(x => x.UserId == user.Id && x.IsActive);
        if (membership is null)
            return RedirectToAction("Index", "Onboarding");

        var buildingCount = await db.BirdBuildings.CountAsync(x => x.AccountId == membership.AccountId && !x.IsDeleted);
        return View(new DashboardViewModel
        {
            AccountName = membership.Account.Name,
            DisplayName = user.DisplayName,
            BuildingCount = buildingCount
        });
    }
}