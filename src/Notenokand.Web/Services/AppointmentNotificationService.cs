using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Notenokand.Domain.Enums;
using Notenokand.Infrastructure.Persistence;
using Notenokand.Web.Models.Notifications;

namespace Notenokand.Web.Services;

public sealed class AppointmentNotificationService(NotenokandDbContext db)
{
    public async Task<Guid?> AccountIdAsync(ClaimsPrincipal user)
    {
        if (!Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return null;
        return await db.AccountUsers.Where(x => x.UserId == userId && x.IsActive && !x.IsDeleted)
            .Select(x => (Guid?)x.AccountId).FirstOrDefaultAsync();
    }

    public async Task<NotificationViewModel> LoadAsync(ClaimsPrincipal user)
    {
        var now = DateTime.UtcNow.AddHours(7);
        var accountId = await AccountIdAsync(user);
        if (accountId is null) return new() { LocalNow = now };
        var end = DateOnly.FromDateTime(now).AddDays(8);
        var appointments = await db.CalendarAppointments.AsNoTracking()
            .Where(x => x.AccountId == accountId && !x.IsDeleted && x.Status == AppointmentStatus.Scheduled && x.ScheduledDate < end)
            .Include(x => x.Building)
            .OrderBy(x => x.ScheduledDate).ThenBy(x => x.ScheduledTime).ThenBy(x => x.Id)
            .ToListAsync();
        // Do not expose a building navigation outside the current account or one removed from use.
        foreach (var item in appointments)
            if (item.Building is not null && (item.Building.AccountId != accountId || item.Building.IsDeleted))
                item.Building = null;
        var maintenance = await db.MaintenanceJobs.AsNoTracking()
            .Where(x => db.BirdBuildings.Any(b => b.Id == x.BuildingId && b.AccountId == accountId && !b.IsDeleted) &&
                !x.IsDeleted && x.Status != MaintenanceStatus.Completed && x.Status != MaintenanceStatus.Cancelled)
            .OrderByDescending(x => x.Priority).ThenBy(x => x.ReportedAt).Take(20).ToListAsync();
        var maintenanceBuildingIds = maintenance.Select(x => x.BuildingId).Distinct().ToArray();
        var buildingNames = await db.BirdBuildings.AsNoTracking().Where(x => x.AccountId == accountId && maintenanceBuildingIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name);
        return new() { LocalNow = now, Appointments = appointments, MaintenanceJobs = maintenance, MaintenanceBuildingNames = buildingNames };
    }
}
