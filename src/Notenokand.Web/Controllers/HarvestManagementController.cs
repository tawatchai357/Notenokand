using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Notenokand.Domain.Entities;
using Notenokand.Domain.Enums;
using Notenokand.Infrastructure.Persistence;
using Notenokand.Web.Models.Harvest;
using Notenokand.Web.Services;
namespace Notenokand.Web.Controllers;

[Authorize, Route("app/harvest")]
public sealed class HarvestManagementController(NotenokandDbContext db, AppointmentNotificationService accounts) : Controller
{
    private async Task LoadAsync(HarvestEditViewModel model, Guid accountId)
    {
        model.Buildings = await db.BirdBuildings.AsNoTracking().Where(x => x.AccountId == accountId && !x.IsDeleted &&
            (x.Status == BuildingStatus.Active || x.Id == model.BuildingId)).OrderBy(x => x.Name)
            .Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToListAsync();
        var options = await db.MasterOptions.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.SortOrder).ToListAsync();
        IReadOnlyList<SelectListItem> Select(string type) => options.Where(x => x.Category == type).Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToList();
        model.NestTypes = Select("HarvestNestType"); model.Colors = Select("HarvestColor"); model.Conditions = Select("HarvestCondition");
    }
    private Task<HarvestRound?> RoundAsync(Guid accountId, Guid id) => db.HarvestRounds.Include(x => x.Items)
        .SingleOrDefaultAsync(x => x.Id == id && x.Building.AccountId == accountId && !x.IsDeleted);
    private async Task<bool> LinkedAsync(HarvestRound round)
    {
        var ids = round.Items.Select(x => x.Id).ToArray();
        return await db.SaleItems.AnyAsync(x => ids.Contains(x.HarvestItemId)) ||
            round.Items.Any(x => x.RemainingWeightKg != x.WeightKg) ||
            await db.QualityScores.AnyAsync(x => x.HarvestRoundId == round.Id) ||
            await db.HarvestSamples.AnyAsync(x => x.HarvestRoundId == round.Id && !x.IsDeleted);
    }
    [HttpGet("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var accountId = await accounts.AccountIdAsync(User); if (!accountId.HasValue) return Forbid();
        var round = await RoundAsync(accountId.Value, id); if (round is null) return NotFound();
        if (await LinkedAsync(round)) { TempData["HarvestError"] = "ล็อตนี้เคยขาย ปรับสต็อก หรือประเมินคุณภาพแล้ว จึงไม่สามารถแก้ไขรอบเก็บได้"; return RedirectToAction("Index", "Harvest"); }
        var model = new HarvestEditViewModel
        {
            Id = id, Version = Convert.ToBase64String(round.RowVersion), BuildingId = round.BuildingId,
            HarvestedOn = round.HarvestedOn, Notes = round.EnvironmentNotes,
            Items = round.Items.Where(x => !x.IsDeleted).Select(x => new HarvestLineViewModel
                { NestTypeId = x.NestTypeId, ColorId = x.ColorId, ConditionId = x.ConditionId, WeightKg = x.WeightKg }).ToList()
        };
        await LoadAsync(model, accountId.Value);
        return View("~/Views/Harvest/Create.cshtml", model);
    }
    [HttpPost("{id:guid}/edit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, HarvestEditViewModel model)
    {
        var accountId = await accounts.AccountIdAsync(User); if (!accountId.HasValue) return Forbid();
        model.Id = id;
        await using var transaction = await db.Database.BeginTransactionAsync();
        await InventoryLock.AcquireAsync(db, accountId.Value);
        var round = await RoundAsync(accountId.Value, id); if (round is null) return NotFound();
        if (await LinkedAsync(round)) ModelState.AddModelError("", "ล็อตนี้มีรายการอ้างอิงแล้ว ไม่สามารถแก้ไขได้");
        if (model.Version != Convert.ToBase64String(round.RowVersion)) ModelState.AddModelError("", "ข้อมูลถูกแก้ไขแล้ว กรุณาเปิดหน้านี้ใหม่");
        if (!await db.BirdBuildings.AnyAsync(x => x.Id == model.BuildingId && x.AccountId == accountId && !x.IsDeleted &&
            (x.Status == BuildingStatus.Active || x.Id == round.BuildingId))) ModelState.AddModelError("", "ตึกไม่พร้อมใช้งาน");
        var options = await db.MasterOptions.AsNoTracking().Where(x => !x.IsDeleted).ToDictionaryAsync(x => x.Id, x => x.Category);
        bool Valid(Guid? id, string category) => id.HasValue && options.TryGetValue(id.Value, out var found) && found == category;
        foreach (var item in model.Items ?? [])
            if (!Valid(item.NestTypeId, "HarvestNestType") || !Valid(item.ColorId, "HarvestColor") || !Valid(item.ConditionId, "HarvestCondition"))
                ModelState.AddModelError("", "ประเภทรัง สี หรือสภาพรังไม่ถูกต้อง");
        if (!ModelState.IsValid)
        {
            await transaction.RollbackAsync();
            model.Items ??= [new()]; await LoadAsync(model, accountId.Value);
            return View("~/Views/Harvest/Create.cshtml", model);
        }
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        foreach (var old in round.Items) { old.IsDeleted = true; old.UpdatedAt = DateTimeOffset.UtcNow; old.UpdatedByUserId = userId; }
        foreach (var item in model.Items!)
            round.Items.Add(new HarvestItem { NestTypeId = item.NestTypeId!.Value, ColorId = item.ColorId, ConditionId = item.ConditionId,
                WeightKg = item.WeightKg!.Value, RemainingWeightKg = item.WeightKg.Value, CreatedByUserId = userId });
        round.BuildingId = model.BuildingId!.Value; round.HarvestedOn = model.HarvestedOn!.Value; round.StartedOn = round.HarvestedOn;
        round.TotalWeightKg = model.TotalWeightKg; round.EnvironmentNotes = model.Notes?.Trim();
        round.UpdatedAt = DateTimeOffset.UtcNow; round.UpdatedByUserId = userId;
        await db.SaveChangesAsync(); await transaction.CommitAsync();
        TempData["SuccessMessage"] = "แก้ไขรอบเก็บและยอดสต็อกแล้ว"; return RedirectToAction("Index", "Harvest");
    }
    [HttpPost("{id:guid}/cancel"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id, string version)
    {
        var accountId = await accounts.AccountIdAsync(User); if (!accountId.HasValue) return Forbid();
        await using var transaction = await db.Database.BeginTransactionAsync();
        await InventoryLock.AcquireAsync(db, accountId.Value);
        var round = await RoundAsync(accountId.Value, id); if (round is null) return NotFound();
        if (version != Convert.ToBase64String(round.RowVersion) || await LinkedAsync(round))
        {
            TempData["HarvestError"] = "ยกเลิกไม่ได้ ข้อมูลเปลี่ยนหรือมีรายการอ้างอิงแล้ว";
            return RedirectToAction("Index", "Harvest");
        }
        round.IsDeleted = true; round.UpdatedAt = DateTimeOffset.UtcNow;
        round.UpdatedByUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        foreach (var item in round.Items) item.IsDeleted = true;
        await db.SaveChangesAsync(); await transaction.CommitAsync();
        TempData["SuccessMessage"] = "ยกเลิกรอบเก็บแล้ว เก็บประวัติไว้ ไม่ลบข้อมูลถาวร";
        return RedirectToAction("Index", "Harvest");
    }
}
