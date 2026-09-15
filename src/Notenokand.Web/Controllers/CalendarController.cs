using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Notenokand.Domain.Entities;
using Notenokand.Domain.Enums;
using Notenokand.Infrastructure.Identity;
using Notenokand.Infrastructure.Persistence;
using Notenokand.Web.Models.Calendar;

namespace Notenokand.Web.Controllers;

[Authorize]
[Route("app/calendar")]
public sealed class CalendarController(NotenokandDbContext db, UserManager<ApplicationUser> userManager) : Controller
{
    private static readonly string[] ThaiMonths = ["มกราคม", "กุมภาพันธ์", "มีนาคม", "เมษายน", "พฤษภาคม", "มิถุนายน", "กรกฎาคม", "สิงหาคม", "กันยายน", "ตุลาคม", "พฤศจิกายน", "ธันวาคม"];

    [HttpGet("")]
    public async Task<IActionResult> Index(string? period)
    {
        var context = await GetAccountContextAsync();
        if (context is null) return RedirectToAction("Index", "Onboarding");
        var month = ParsePeriod(period);
        var start = new DateOnly(month.Year, month.Month, 1);
        var end = start.AddMonths(1);
        var entities = await db.CalendarAppointments.AsNoTracking()
            .Where(x => x.AccountId == context.Value.AccountId && !x.IsDeleted && x.ScheduledDate >= start && x.ScheduledDate < end)
            .OrderBy(x => x.ScheduledDate).ThenBy(x => x.ScheduledTime).ThenBy(x => x.Title)
            .ToListAsync();
        var buildingIds = entities.Where(x => x.BuildingId.HasValue).Select(x => x.BuildingId!.Value).Distinct().ToArray();
        var buildings = await db.BirdBuildings.AsNoTracking().Where(x => x.AccountId == context.Value.AccountId && buildingIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name);
        var cards = entities.Select(x => new AppointmentCardViewModel
        {
            Id = x.Id, Type = x.Type, Status = x.Status, ScheduledDate = x.ScheduledDate, ScheduledTime = x.ScheduledTime,
            Title = x.Title, Location = x.Location, Notes = x.Notes,
            BuildingName = x.BuildingId.HasValue && buildings.TryGetValue(x.BuildingId.Value, out var name) ? name : "ทุกตึก/ส่วนกลาง"
        }).ToList();
        return View(new CalendarIndexViewModel
        {
            Period = start.ToString("yyyy-MM", CultureInfo.InvariantCulture), PeriodDisplay = $"{ThaiMonths[start.Month - 1]} {start.Year + 543}",
            MonthStart = start, PreviousPeriod = start.AddMonths(-1).ToString("yyyy-MM", CultureInfo.InvariantCulture), NextPeriod = start.AddMonths(1).ToString("yyyy-MM", CultureInfo.InvariantCulture),
            Appointments = cards
        });
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(AppointmentType type = AppointmentType.Maintenance, string? date = null)
    {
        var context = await GetAccountContextAsync();
        if (context is null) return RedirectToAction("Index", "Onboarding");
        var scheduledDate = DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) ? parsed : DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
        var model = new AppointmentEditViewModel { Type = type, ScheduledDate = scheduledDate };
        await LoadBuildingsAsync(model, context.Value.AccountId);
        return View("Edit", model);
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppointmentEditViewModel model)
    {
        var context = await GetAccountContextAsync(); if (context is null) return Forbid();
        await ValidateAsync(model, context.Value.AccountId);
        if (!ModelState.IsValid) { await LoadBuildingsAsync(model, context.Value.AccountId); return View("Edit", model); }
        db.CalendarAppointments.Add(new CalendarAppointment
        {
            AccountId = context.Value.AccountId, OwnerUserId = context.Value.UserId, BuildingId = model.BuildingId,
            Type = model.Type, Status = AppointmentStatus.Scheduled, ScheduledDate = model.ScheduledDate, ScheduledTime = model.ScheduledTime,
            Title = model.Title.Trim(), Location = Clean(model.Location), Notes = Clean(model.Notes), CreatedByUserId = context.Value.UserId
        });
        await db.SaveChangesAsync();
        TempData["SuccessMessage"] = "เพิ่มนัดหมายเรียบร้อยแล้ว";
        return RedirectToAction(nameof(Index), new { period = model.ScheduledDate.ToString("yyyy-MM", CultureInfo.InvariantCulture) });
    }

    [HttpGet("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var context = await GetAccountContextAsync(); if (context is null) return Forbid();
        var entity = await db.CalendarAppointments.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.AccountId == context.Value.AccountId && !x.IsDeleted);
        if (entity is null) return NotFound();
        var model = new AppointmentEditViewModel { Id = entity.Id, Type = entity.Type, Status = entity.Status, ScheduledDate = entity.ScheduledDate, ScheduledTime = entity.ScheduledTime, Title = entity.Title, BuildingId = entity.BuildingId, Location = entity.Location, Notes = entity.Notes };
        await LoadBuildingsAsync(model, context.Value.AccountId, entity.BuildingId);
        return View(model);
    }

    [HttpPost("{id:guid}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, AppointmentEditViewModel model)
    {
        var context = await GetAccountContextAsync(); if (context is null) return Forbid();
        var entity = await db.CalendarAppointments.SingleOrDefaultAsync(x => x.Id == id && x.AccountId == context.Value.AccountId && !x.IsDeleted);
        if (entity is null) return NotFound();
        model.Id = id; await ValidateAsync(model, context.Value.AccountId, entity.BuildingId);
        if (!ModelState.IsValid) { await LoadBuildingsAsync(model, context.Value.AccountId, entity.BuildingId); return View(model); }
        entity.Type = model.Type; entity.Status = model.Status; entity.ScheduledDate = model.ScheduledDate; entity.ScheduledTime = model.ScheduledTime;
        entity.Title = model.Title.Trim(); entity.BuildingId = model.BuildingId; entity.Location = Clean(model.Location); entity.Notes = Clean(model.Notes);
        entity.UpdatedAt = DateTimeOffset.UtcNow; entity.UpdatedByUserId = context.Value.UserId;
        await db.SaveChangesAsync();
        TempData["SuccessMessage"] = "แก้ไขนัดหมายเรียบร้อยแล้ว";
        return RedirectToAction(nameof(Index), new { period = model.ScheduledDate.ToString("yyyy-MM", CultureInfo.InvariantCulture) });
    }

    [HttpPost("{id:guid}/complete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(Guid id)
    {
        var context = await GetAccountContextAsync(); if (context is null) return Forbid();
        var entity = await db.CalendarAppointments.SingleOrDefaultAsync(x => x.Id == id && x.AccountId == context.Value.AccountId && !x.IsDeleted);
        if (entity is null) return NotFound();
        entity.Status = entity.Status == AppointmentStatus.Completed ? AppointmentStatus.Scheduled : AppointmentStatus.Completed;
        entity.UpdatedAt = DateTimeOffset.UtcNow; entity.UpdatedByUserId = context.Value.UserId;
        await db.SaveChangesAsync();
        TempData["SuccessMessage"] = entity.Status == AppointmentStatus.Completed ? "ทำเครื่องหมายนัดหมายว่าเสร็จแล้ว" : "เปิดนัดหมายอีกครั้งแล้ว";
        return RedirectToAction(nameof(Index), new { period = entity.ScheduledDate.ToString("yyyy-MM", CultureInfo.InvariantCulture) });
    }

    [HttpPost("{id:guid}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var context = await GetAccountContextAsync(); if (context is null) return Forbid();
        var entity = await db.CalendarAppointments.SingleOrDefaultAsync(x => x.Id == id && x.AccountId == context.Value.AccountId && !x.IsDeleted);
        if (entity is null) return NotFound();
        entity.IsDeleted = true; entity.UpdatedAt = DateTimeOffset.UtcNow; entity.UpdatedByUserId = context.Value.UserId;
        await db.SaveChangesAsync(); TempData["SuccessMessage"] = "ลบนัดหมายแล้ว";
        return RedirectToAction(nameof(Index), new { period = entity.ScheduledDate.ToString("yyyy-MM", CultureInfo.InvariantCulture) });
    }

    private async Task ValidateAsync(AppointmentEditViewModel model, Guid accountId, Guid? existingBuildingId = null)
    {
        if (model.BuildingId.HasValue && !await db.BirdBuildings.AnyAsync(x => x.Id == model.BuildingId && x.AccountId == accountId && !x.IsDeleted && (x.Status == BuildingStatus.Active || x.Id == existingBuildingId)))
            ModelState.AddModelError(nameof(model.BuildingId), "ตึกที่เลือกไม่พร้อมใช้งาน");
    }

    private async Task LoadBuildingsAsync(AppointmentEditViewModel model, Guid accountId, Guid? includeBuildingId = null) =>
        model.Buildings = await db.BirdBuildings.AsNoTracking().Where(x => x.AccountId == accountId && !x.IsDeleted && (x.Status == BuildingStatus.Active || x.Id == includeBuildingId)).OrderBy(x => x.Name).Select(x => new SelectListItem(x.Name + (x.Status == BuildingStatus.Inactive ? " (ไม่ใช้งาน)" : ""), x.Id.ToString())).ToListAsync();

    private async Task<(Guid AccountId, Guid UserId)?> GetAccountContextAsync()
    {
        if (!Guid.TryParse(userManager.GetUserId(User), out var userId)) return null;
        var accountId = await db.AccountUsers.Where(x => x.UserId == userId && x.IsActive && !x.IsDeleted).Select(x => (Guid?)x.AccountId).FirstOrDefaultAsync();
        return accountId.HasValue ? (accountId.Value, userId) : null;
    }

    private static DateOnly ParsePeriod(string? period) => DateOnly.TryParseExact(period + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) ? parsed : DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}