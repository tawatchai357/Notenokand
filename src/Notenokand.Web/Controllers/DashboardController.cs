using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notenokand.Infrastructure.Identity;
using Notenokand.Infrastructure.Persistence;
using Notenokand.Web.Models.Account;
using Notenokand.Web.Models.Buildings;

namespace Notenokand.Web.Controllers;

[Authorize]
[Route("app")]
public sealed class DashboardController(NotenokandDbContext db, UserManager<ApplicationUser> userManager) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();
        var membership = await db.AccountUsers.AsNoTracking().Include(x => x.Account).FirstOrDefaultAsync(x => x.UserId == user.Id && x.IsActive && !x.IsDeleted);
        if (membership is null) return RedirectToAction("Index", "Onboarding");

        var entities = await db.BirdBuildings.AsNoTracking().Where(x => x.AccountId == membership.AccountId && !x.IsDeleted).OrderByDescending(x => x.Status).ThenBy(x => x.Name).ToListAsync();
        var districtCodes = entities.Where(x => x.DistrictCode.HasValue).Select(x => x.DistrictCode!.Value).Distinct().ToArray();
        var subdistrictCodes = entities.Where(x => x.SubdistrictCode.HasValue).Select(x => x.SubdistrictCode!.Value).Distinct().ToArray();
        var districts = await db.ThaiDistricts.AsNoTracking().Where(x => districtCodes.Contains(x.Code)).ToDictionaryAsync(x => x.Code, x => x.NameTh);
        var subdistricts = await db.ThaiSubdistricts.AsNoTracking().Where(x => subdistrictCodes.Contains(x.Code)).ToDictionaryAsync(x => x.Code, x => x.NameTh);
        var buildings = entities.Select(x => new BuildingCardViewModel
        {
            Id = x.Id, Code = x.Code, Name = x.Name, Latitude = x.Latitude, Longitude = x.Longitude,
            Status = x.Status, FloorCount = x.FloorCount, RoomCount = x.RoomCount,
            WidthMeters = x.WidthMeters, DepthMeters = x.DepthMeters, ConstructionBudget = x.ConstructionBudget,
            Location = BuildLocation(x.SubdistrictCode, x.DistrictCode, x.Province, x.PostalCode, subdistricts, districts)
        }).ToList();

        return View(new DashboardViewModel
        {
            AccountName = membership.Account.Name,
            DisplayName = user.DisplayName,
            BuildingCount = buildings.Count,
            Buildings = buildings
        });
    }

    private static string BuildLocation(int? subdistrictCode, int? districtCode, string? province, string? postalCode, IReadOnlyDictionary<int, string> subdistricts, IReadOnlyDictionary<int, string> districts)
    {
        var parts = new List<string>();
        if (subdistrictCode.HasValue && subdistricts.TryGetValue(subdistrictCode.Value, out var subdistrict)) parts.Add(subdistrict);
        if (districtCode.HasValue && districts.TryGetValue(districtCode.Value, out var district)) parts.Add(district);
        if (!string.IsNullOrWhiteSpace(province)) parts.Add(province);
        if (!string.IsNullOrWhiteSpace(postalCode)) parts.Add(postalCode);
        return parts.Count == 0 ? "ยังไม่ได้ระบุที่อยู่" : string.Join(" · ", parts);
    }
}