using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Notenokand.Domain.Entities;
using Notenokand.Domain.Enums;
using Notenokand.Infrastructure.Persistence;
using Notenokand.Web.Models.Maintenance;

namespace Notenokand.Web.Controllers;

[Authorize, Route("app/maintenance")]
public sealed class MaintenanceController(NotenokandDbContext db) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(MaintenanceStatus? status)
    {
        var account = await ContextAsync(); if (account is null) return RedirectToAction("Index", "Onboarding");
        var query = db.MaintenanceJobs.AsNoTracking().Where(x => db.BirdBuildings.Any(b => b.Id == x.BuildingId && b.AccountId == account.Value.AccountId && !b.IsDeleted) && !x.IsDeleted);
        var all = await query.OrderBy(x => x.Status == MaintenanceStatus.Completed || x.Status == MaintenanceStatus.Cancelled)
            .ThenByDescending(x => x.Priority).ThenByDescending(x => x.ReportedAt).ToListAsync();
        var buildingIds = all.Select(x => x.BuildingId).Distinct().ToArray();
        var names = await db.BirdBuildings.AsNoTracking().Where(x => x.AccountId == account.Value.AccountId && buildingIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name);
        return View(new MaintenanceIndexViewModel
        {
            Jobs = status.HasValue ? all.Where(x => x.Status == status.Value).ToList() : all,
            BuildingNames = names, Status = status,
            OpenCount = all.Count(x => x.Status is not MaintenanceStatus.Completed and not MaintenanceStatus.Cancelled),
            TotalCost = all.Where(x => x.Status == MaintenanceStatus.Completed).Sum(x => x.PartsCost + x.LaborCost)
        });
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(Guid? buildingId)
    {
        var account = await ContextAsync(); if (account is null) return RedirectToAction("Index", "Onboarding");
        var model = new MaintenanceEditViewModel { BuildingId = buildingId };
        await LoadBuildings(model, account.Value.AccountId);
        return View("Edit", model);
    }

    [HttpPost("create"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MaintenanceEditViewModel model)
    {
        var account = await ContextAsync(); if (account is null) return Forbid();
        await ValidateBuilding(model, account.Value.AccountId);
        if (!ModelState.IsValid) { await LoadBuildings(model, account.Value.AccountId); return View("Edit", model); }
        var now = DateTimeOffset.UtcNow;
        db.MaintenanceJobs.Add(new MaintenanceJob
        {
            BuildingId = model.BuildingId!.Value, Type = model.Type, Priority = model.Priority, Status = model.Status,
            Issue = model.Issue.Trim(), ReportedAt = now,
            StartedAt = model.Status is MaintenanceStatus.InProgress or MaintenanceStatus.AwaitingAcceptance or MaintenanceStatus.Completed ? now : null,
            CompletedAt = model.Status == MaintenanceStatus.Completed ? now : null,
            PartsCost = model.PartsCost, LaborCost = model.LaborCost, DowntimeHours = model.DowntimeHours,
            AcceptanceResult = Clean(model.AcceptanceResult), NextInspectionOn = model.NextInspectionOn,
            CreatedByUserId = account.Value.UserId
        });
        await db.SaveChangesAsync(); TempData["SuccessMessage"] = "สร้างงานซ่อมบำรุงแล้ว";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var account = await ContextAsync(); if (account is null) return Forbid();
        var item = await db.MaintenanceJobs.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && db.BirdBuildings.Any(b => b.Id == x.BuildingId && b.AccountId == account.Value.AccountId) && !x.IsDeleted);
        if (item is null) return NotFound();
        var model = new MaintenanceEditViewModel { Id=item.Id, BuildingId=item.BuildingId, Issue=item.Issue, Type=item.Type, Priority=item.Priority, Status=item.Status, PartsCost=item.PartsCost, LaborCost=item.LaborCost, DowntimeHours=item.DowntimeHours, AcceptanceResult=item.AcceptanceResult, NextInspectionOn=item.NextInspectionOn };
        await LoadBuildings(model, account.Value.AccountId, item.BuildingId); return View(model);
    }

    [HttpPost("{id:guid}/edit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, MaintenanceEditViewModel model)
    {
        var account = await ContextAsync(); if (account is null) return Forbid();
        var item = await db.MaintenanceJobs.SingleOrDefaultAsync(x => x.Id == id && db.BirdBuildings.Any(b => b.Id == x.BuildingId && b.AccountId == account.Value.AccountId) && !x.IsDeleted);
        if (item is null) return NotFound();
        model.Id=id; await ValidateBuilding(model, account.Value.AccountId, item.BuildingId);
        if (!ModelState.IsValid) { await LoadBuildings(model, account.Value.AccountId, item.BuildingId); return View(model); }
        var previous = item.Status; var now = DateTimeOffset.UtcNow;
        item.BuildingId=model.BuildingId!.Value; item.Type=model.Type; item.Priority=model.Priority; item.Status=model.Status; item.Issue=model.Issue.Trim();
        item.PartsCost=model.PartsCost; item.LaborCost=model.LaborCost; item.DowntimeHours=model.DowntimeHours; item.AcceptanceResult=Clean(model.AcceptanceResult); item.NextInspectionOn=model.NextInspectionOn;
        if (item.StartedAt is null && model.Status is MaintenanceStatus.InProgress or MaintenanceStatus.AwaitingAcceptance or MaintenanceStatus.Completed) item.StartedAt=now;
        item.CompletedAt=model.Status == MaintenanceStatus.Completed ? item.CompletedAt ?? now : null;
        item.UpdatedAt=now; item.UpdatedByUserId=account.Value.UserId;
        await db.SaveChangesAsync(); TempData["SuccessMessage"] = previous == model.Status ? "บันทึกงานซ่อมบำรุงแล้ว" : "อัปเดตสถานะงานซ่อมบำรุงแล้ว";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateBuilding(MaintenanceEditViewModel model, Guid accountId, Guid? include = null)
    {
        if (!model.BuildingId.HasValue || !await db.BirdBuildings.AnyAsync(x => x.Id == model.BuildingId && x.AccountId == accountId && !x.IsDeleted && (x.Status == BuildingStatus.Active || x.Id == include)))
            ModelState.AddModelError(nameof(model.BuildingId), "กรุณาเลือกตึกนกที่ใช้งานในบัญชีนี้");
    }
    private async Task LoadBuildings(MaintenanceEditViewModel model, Guid accountId, Guid? include = null) =>
        model.Buildings = await db.BirdBuildings.AsNoTracking().Where(x => x.AccountId == accountId && !x.IsDeleted && (x.Status == BuildingStatus.Active || x.Id == include)).OrderBy(x => x.Name).Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToListAsync();
    private async Task<(Guid AccountId, Guid UserId)?> ContextAsync()
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return null;
        var accountId=await db.AccountUsers.Where(x=>x.UserId==userId && x.IsActive && !x.IsDeleted).Select(x=>(Guid?)x.AccountId).FirstOrDefaultAsync();
        return accountId.HasValue ? (accountId.Value,userId) : null;
    }
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
