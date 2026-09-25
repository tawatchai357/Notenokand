using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Notenokand.Infrastructure.Persistence;
using Notenokand.Web.Models.Calendar;
using Notenokand.Web.Models.Finance;
using Notenokand.Web.Models.Harvest;
using Notenokand.Web.Models.Maintenance;
using Notenokand.Web.Models.Quality;
using Notenokand.Web.Models.Stock;

namespace Notenokand.Web.Services;

public sealed class AccountPermissionFilter(NotenokandDbContext db) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (HttpMethods.IsGet(context.HttpContext.Request.Method) || HttpMethods.IsHead(context.HttpContext.Request.Method))
        { await next(); return; }
        // Public POST actions (sign in, registration, password reset) must still reach
        // their own validation and rate-limit handlers. Authorization attributes guard
        // protected controllers; this filter only adds account-level permissions.
        if (context.HttpContext.User.Identity?.IsAuthenticated != true)
        { await next(); return; }
        var userIdText = context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdText, out var userId)) { context.Result = new ForbidResult(); return; }
        var member = await db.AccountUsers.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId && x.IsActive && !x.IsDeleted);
        if (member is null) { await next(); return; } // registration/onboarding has no membership yet
        var controller = context.RouteData.Values["controller"]?.ToString() ?? "";
        if (controller is "Account" or "Profile") { await next(); return; }
        if (member.RoleName == "Owner") { await next(); return; }
        if (member.RoleName != "Editor") { context.Result = new ForbidResult(); return; }

        var action = context.RouteData.Values["action"]?.ToString() ?? "";
        if (controller is "Members" or "Audit" or "Onboarding" || (controller == "Finance" && action.Contains("Category", StringComparison.OrdinalIgnoreCase)) ||
            (controller == "Buildings" && action == "Create"))
        { context.Result = new ForbidResult(); return; }

        var buildingIds = new HashSet<Guid>();
        foreach (var value in context.ActionArguments.Values)
        {
            switch (value)
            {
                case FinanceEditViewModel x when x.BuildingId.HasValue: buildingIds.Add(x.BuildingId.Value); break;
                case AppointmentEditViewModel x when x.BuildingId.HasValue: buildingIds.Add(x.BuildingId.Value); break;
                case HarvestEditViewModel x when x.BuildingId.HasValue: buildingIds.Add(x.BuildingId.Value); break;
                case MaintenanceEditViewModel x when x.BuildingId.HasValue: buildingIds.Add(x.BuildingId.Value); break;
                case QualityScoreEditViewModel x:
                    var qualityBuildingId = await db.HarvestRounds.Where(i => i.Id == x.HarvestRoundId && i.Building.AccountId == member.AccountId)
                        .Select(i => (Guid?)i.BuildingId).FirstOrDefaultAsync();
                    if (qualityBuildingId.HasValue) buildingIds.Add(qualityBuildingId.Value);
                    break;
                case SaleCreateViewModel x:
                    var itemIds = x.Items?.Select(i => i.HarvestItemId).Where(i => i != Guid.Empty).ToArray() ?? [];
                    foreach (var stockBuildingId in await db.HarvestItems.Where(i => itemIds.Contains(i.Id)).Select(i => i.HarvestRound.BuildingId).Distinct().ToListAsync()) buildingIds.Add(stockBuildingId);
                    if (itemIds.Length == 0 || buildingIds.Count == 0) { context.Result = new ForbidResult(); return; }
                    break;
            }
        }
        if (context.RouteData.Values.TryGetValue("id", out var raw) && Guid.TryParse(raw?.ToString(), out var id))
        {
            if (controller == "Stock")
                foreach (var buildingId in await db.SaleItems.Where(x => x.SaleId == id && x.Sale.AccountId == member.AccountId)
                    .Select(x => x.HarvestItem.HarvestRound.BuildingId).Distinct().ToListAsync()) buildingIds.Add(buildingId);
            Guid? found = controller switch
            {
                "Buildings" => await db.BirdBuildings.Where(x => x.Id == id && x.AccountId == member.AccountId).Select(x => (Guid?)x.Id).FirstOrDefaultAsync(),
                "HarvestManagement" => await db.HarvestRounds.Where(x => x.Id == id && x.Building.AccountId == member.AccountId).Select(x => (Guid?)x.BuildingId).FirstOrDefaultAsync(),
                "Maintenance" => await db.MaintenanceJobs.Where(x => x.Id == id && db.BirdBuildings.Any(b => b.Id == x.BuildingId && b.AccountId == member.AccountId)).Select(x => (Guid?)x.BuildingId).FirstOrDefaultAsync(),
                "Finance" => await db.FinancialTransactions.Where(x => x.Id == id && x.AccountId == member.AccountId).Select(x => x.BuildingId).FirstOrDefaultAsync(),
                "Calendar" or "Notifications" => await db.CalendarAppointments.Where(x => x.Id == id && x.AccountId == member.AccountId).Select(x => x.BuildingId).FirstOrDefaultAsync(),
                "Stock" => null,
                _ => null
            };
            if (found.HasValue) buildingIds.Add(found.Value);
        }
        // Central transactions and unscoped business mutations are Owner-only.
        if (controller is "Finance" or "Calendar" or "Harvest" or "HarvestManagement" or "Maintenance" or "Quality" or "Buildings" or "Stock" or "Notifications")
        {
            if (buildingIds.Count == 0) { context.Result = new ForbidResult(); return; }
            var allowed = await db.BuildingUsers.AsNoTracking().Where(x => x.UserId == userId && !x.IsDeleted && buildingIds.Contains(x.BuildingId))
                .Select(x => x.BuildingId).Distinct().CountAsync();
            if (allowed != buildingIds.Count) { context.Result = new ForbidResult(); return; }
        }
        await next();
    }
}
