using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notenokand.Domain.Enums;
using Notenokand.Infrastructure.Persistence;
using Notenokand.Web.Services;

namespace Notenokand.Web.Controllers;

[Authorize]
[Route("app/notifications")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class NotificationsController(NotenokandDbContext db, AppointmentNotificationService notifications) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index() => View(await notifications.LoadAsync(User));

    [HttpPost("{id:guid}/complete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(Guid id)
    {
        var accountId = await notifications.AccountIdAsync(User);
        if (accountId is null || !Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Forbid();
        var item = await db.CalendarAppointments.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id && x.AccountId == accountId && !x.IsDeleted);
        if (item is null) return NotFound();
        var updated = await db.CalendarAppointments
            .Where(x => x.Id == id && x.AccountId == accountId && !x.IsDeleted && x.Status == AppointmentStatus.Scheduled)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.Status, AppointmentStatus.Completed)
                .SetProperty(x => x.UpdatedAt, DateTimeOffset.UtcNow)
                .SetProperty(x => x.UpdatedByUserId, (Guid?)userId));
        TempData["SuccessMessage"] = updated > 0 ? "ทำเครื่องหมายว่าเสร็จแล้ว" : "รายการนี้เปลี่ยนสถานะแล้ว กรุณาตรวจสอบอีกครั้ง";
        return RedirectToAction(nameof(Index));
    }
}
