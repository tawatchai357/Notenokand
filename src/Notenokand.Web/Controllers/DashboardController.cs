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
    public async Task<IActionResult> Index(int? year)
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
        var financialTotalsByBuilding = await db.FinancialTransactions.AsNoTracking()
            .Where(x => x.AccountId == membership.AccountId && !x.IsDeleted && x.BuildingId.HasValue)
            .GroupBy(x => x.BuildingId!.Value)
            .Select(group => new
            {
                BuildingId = group.Key,
                TotalIncome = group.Sum(x => x.Type == Notenokand.Domain.Enums.TransactionType.Income ? x.Amount : 0m),
                TotalExpense = group.Sum(x => x.Type == Notenokand.Domain.Enums.TransactionType.Expense ? x.Amount : 0m)
            })
            .ToDictionaryAsync(x => x.BuildingId);
        var buildings = entities.Select(x => new BuildingCardViewModel
        {
            Id = x.Id, Code = x.Code, Name = x.Name, Latitude = x.Latitude, Longitude = x.Longitude,
            Status = x.Status, FloorCount = x.FloorCount, RoomCount = x.RoomCount,
            WidthMeters = x.WidthMeters, DepthMeters = x.DepthMeters, ConstructionBudget = x.ConstructionBudget,
            BuiltOrPurchasedYearBuddhist = x.BuiltOrPurchasedYear.HasValue ? x.BuiltOrPurchasedYear.Value + 543 : null,
            HasPhoto = !string.IsNullOrWhiteSpace(x.PhotoStorageKey),
            Location = BuildLocation(x.SubdistrictCode, x.DistrictCode, x.Province, x.PostalCode, subdistricts, districts)
        }).ToList();

        foreach (var building in buildings)
        {
            if (!financialTotalsByBuilding.TryGetValue(building.Id, out var totals)) continue;
            building.TotalIncome = totals.TotalIncome;
            building.TotalExpense = totals.TotalExpense;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
        var selectedYear = year is >= 2000 and <= 2200 ? year.Value : today.Year;
        var yearStart = new DateOnly(selectedYear, 1, 1);
        var yearEnd = yearStart.AddYears(1);
        var annualTotals = await db.FinancialTransactions.AsNoTracking()
            .Where(x => x.AccountId == membership.AccountId && !x.IsDeleted && x.TransactionDate >= yearStart && x.TransactionDate < yearEnd)
            .GroupBy(x => new { x.TransactionDate.Month, x.Type })
            .Select(group => new { group.Key.Month, group.Key.Type, Total = group.Sum(x => x.Amount) })
            .ToListAsync();
        var monthlyIncome = new decimal[12];
        var monthlyExpense = new decimal[12];
        foreach (var total in annualTotals)
        {
            if (total.Type == Notenokand.Domain.Enums.TransactionType.Income) monthlyIncome[total.Month - 1] = total.Total;
            else if (total.Type == Notenokand.Domain.Enums.TransactionType.Expense) monthlyExpense[total.Month - 1] = total.Total;
        }
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var monthEnd = monthStart.AddMonths(1);
        var monthTransactions = db.FinancialTransactions.AsNoTracking().Where(x => x.AccountId == membership.AccountId && !x.IsDeleted && x.TransactionDate >= monthStart && x.TransactionDate < monthEnd);
        var totalIncome = await monthTransactions.Where(x => x.Type == Notenokand.Domain.Enums.TransactionType.Income).SumAsync(x => (decimal?)x.Amount) ?? 0;
        var totalExpense = await monthTransactions.Where(x => x.Type == Notenokand.Domain.Enums.TransactionType.Expense).SumAsync(x => (decimal?)x.Amount) ?? 0;

        return View(new DashboardViewModel
        {
            AccountName = membership.Account.Name,
            DisplayName = user.DisplayName,
            BuildingCount = buildings.Count,
            RemainingStockKg = await db.HarvestItems.AsNoTracking()
                .Where(x => !x.IsDeleted && !x.HarvestRound.IsDeleted && !x.HarvestRound.Building.IsDeleted &&
                    x.HarvestRound.Building.AccountId == membership.AccountId && x.HarvestRound.Status != Notenokand.Domain.Enums.HarvestStatus.Draft)
                .SumAsync(x => (decimal?)x.RemainingWeightKg) ?? 0m,
            TotalIncomeThisMonth = totalIncome,
            TotalExpenseThisMonth = totalExpense,
            FinanceYear = selectedYear,
            MonthlyIncome = monthlyIncome,
            MonthlyExpense = monthlyExpense,
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
