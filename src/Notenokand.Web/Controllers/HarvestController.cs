using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Notenokand.Domain.Entities;
using Notenokand.Domain.Enums;
using Notenokand.Infrastructure.Identity;
using Notenokand.Infrastructure.Persistence;
using Notenokand.Web.Models.Harvest;
namespace Notenokand.Web.Controllers;

[Authorize]
[Route("app/harvest")]
public sealed class HarvestController(NotenokandDbContext db, UserManager<ApplicationUser> users) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var accountId = await AccountIdAsync();
        if (accountId is null) return RedirectToAction("Index", "Onboarding");
        var rounds = await db.HarvestRounds.AsNoTracking()
            .Where(x => x.Building.AccountId == accountId && !x.IsDeleted)
            .Include(x => x.Building).Include(x => x.Items.Where(i => !i.IsDeleted))
            .OrderByDescending(x => x.HarvestedOn).ThenByDescending(x => x.CreatedAt).ToListAsync();
        var names = await db.MasterOptions.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.Name);
        return View(new HarvestIndexViewModel { Rounds = rounds, OptionNames = names });
    }
    [HttpGet("create")]
    public async Task<IActionResult> Create(Guid? buildingId)
    {
        var accountId = await AccountIdAsync();
        if (accountId is null) return RedirectToAction("Index", "Onboarding");
        var model = new HarvestEditViewModel { BuildingId = buildingId };
        await LoadOptionsAsync(model, accountId.Value);
        return View(model);
    }
    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(HarvestEditViewModel model)
    {
        var accountId = await AccountIdAsync();
        if (accountId is null) return Forbid();
        if (!await db.BirdBuildings.AnyAsync(x => x.Id == model.BuildingId && x.AccountId == accountId && !x.IsDeleted && x.Status == BuildingStatus.Active))
            ModelState.AddModelError(nameof(model.BuildingId), "กรุณาเลือกตึกนกที่ใช้งานในบัญชีนี้");
        var options = await db.MasterOptions.AsNoTracking().Where(x => x.IsActive && !x.IsDeleted).ToDictionaryAsync(x => x.Id, x => x.Category);
        bool Valid(Guid? id, string category) => id.HasValue && options.TryGetValue(id.Value, out var found) && found == category;
        if (model.Items is null || model.Items.Count is < 1 or > 100)
            ModelState.AddModelError(nameof(model.Items), "กรุณาเพิ่มรายการรังนก 1–100 รายการ");
        else
            for (var i = 0; i < model.Items.Count; i++)
            {
                var item = model.Items[i];
                if (!Valid(item.NestTypeId, "HarvestNestType")) ModelState.AddModelError($"Items[{i}].NestTypeId", "กรุณาเลือกลักษณะรัง");
                if (!Valid(item.ColorId, "HarvestColor")) ModelState.AddModelError($"Items[{i}].ColorId", "กรุณาเลือกสีรัง");
                if (!Valid(item.ConditionId, "HarvestCondition")) ModelState.AddModelError($"Items[{i}].ConditionId", "กรุณาเลือกสภาพรัง");
            }
        if (!ModelState.IsValid)
        {
            model.Items ??= [new()];
            await LoadOptionsAsync(model, accountId.Value);
            return View(model);
        }
        var userId = Guid.Parse(users.GetUserId(User)!);
        var round = new HarvestRound
        {
            BuildingId = model.BuildingId!.Value, RoundNumber = $"HV-{model.HarvestedOn:yyyyMMdd}-{Guid.NewGuid():N}",
            HarvestedOn = model.HarvestedOn!.Value, StartedOn = model.HarvestedOn.Value,
            TotalWeightKg = model.TotalWeightKg, CollectorUserId = userId, CreatedByUserId = userId,
            EnvironmentNotes = model.Notes?.Trim(), Status = HarvestStatus.Confirmed,
            Items = model.Items!.Select(x => new HarvestItem
            {
                NestTypeId = x.NestTypeId!.Value, ColorId = x.ColorId, ConditionId = x.ConditionId,
                WeightKg = x.WeightKg!.Value, RemainingWeightKg = x.WeightKg.Value, CreatedByUserId = userId
            }).ToList()
        };
        db.HarvestRounds.Add(round);
        await db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"บันทึกการเก็บรังนก รวม {round.TotalWeightKg:N3} กก. แล้ว";
        return RedirectToAction(nameof(Index));
    }
    private async Task LoadOptionsAsync(HarvestEditViewModel model, Guid accountId)
    {
        model.Buildings = await db.BirdBuildings.AsNoTracking().Where(x => x.AccountId == accountId && !x.IsDeleted && x.Status == BuildingStatus.Active)
            .OrderBy(x => x.Name).Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToListAsync();
        var options = await db.MasterOptions.AsNoTracking().Where(x => x.IsActive && !x.IsDeleted).OrderBy(x => x.SortOrder).ToListAsync();
        IReadOnlyList<SelectListItem> Select(string category) => options.Where(x => x.Category == category).Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToList();
        model.NestTypes = Select("HarvestNestType"); model.Colors = Select("HarvestColor"); model.Conditions = Select("HarvestCondition");
    }
    private async Task<Guid?> AccountIdAsync()
    {
        if (!Guid.TryParse(users.GetUserId(User), out var id)) return null;
        return await db.AccountUsers.Where(x => x.UserId == id && x.IsActive && !x.IsDeleted).Select(x => (Guid?)x.AccountId).FirstOrDefaultAsync();
    }
}
