using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Notenokand.Domain.Enums;
using Notenokand.Infrastructure.Persistence;
using Notenokand.Web.Models.Stock;
using Notenokand.Web.Services;

namespace Notenokand.Web.Controllers;

[Authorize]
[Route("app/stock")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class StockController(NotenokandDbContext db, AppointmentNotificationService accounts, LotSaleService sales) : Controller
{
    private async Task<(Guid AccountId, Guid UserId)?> ContextAsync()
    {
        var accountId = await accounts.AccountIdAsync(User);
        return accountId.HasValue && Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            ? (accountId.Value, userId) : null;
    }

    private async Task<StockPageViewModel> StockAsync(Guid accountId) => new()
    {
        Items = await db.HarvestItems.AsNoTracking().Include(x => x.HarvestRound).ThenInclude(x => x.Building)
            .Where(x => x.HarvestRound.Building.AccountId == accountId && !x.IsDeleted && !x.HarvestRound.IsDeleted &&
                !x.HarvestRound.Building.IsDeleted && x.HarvestRound.Status != HarvestStatus.Draft)
            .OrderBy(x => x.HarvestRound.HarvestedOn).ThenBy(x => x.Id).ToListAsync(),
        Names = await db.MasterOptions.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.Name)
    };

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var context = await ContextAsync(); if (context is null) return Forbid();
        return View(await StockAsync(context.Value.AccountId));
    }

    [HttpGet("sell")]
    public async Task<IActionResult> Create(Guid? itemId)
    {
        var context = await ContextAsync(); if (context is null) return Forbid();
        return View(new SaleCreateViewModel { Stock = await StockAsync(context.Value.AccountId),
            Items = [new() { HarvestItemId = itemId ?? Guid.Empty }] });
    }

    [HttpPost("sell")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SaleCreateViewModel model)
    {
        var context = await ContextAsync(); if (context is null) return Forbid();
        if (ModelState.IsValid)
        {
            try
            {
                var id = await sales.CreateAsync(context.Value.AccountId, context.Value.UserId, model);
                TempData["SuccessMessage"] = "บันทึกการขาย ตัดสต็อก และเชื่อมรายรับแล้ว";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (LotSaleException ex) { ModelState.AddModelError("", ex.Message); }
            catch (SqlException ex) when (ex.Number is 1205 or 51000) { ModelState.AddModelError("", "มีรายการกำลังบันทึก กรุณาลองอีกครั้ง"); }
        }
        model.Items ??= [new()];
        model.Stock = await StockAsync(context.Value.AccountId);
        return View(model);
    }

    [HttpGet("sales")]
    public async Task<IActionResult> Sales(int page = 1)
    {
        var context = await ContextAsync(); if (context is null) return Forbid();
        page = Math.Clamp(page, 1, 100000);
        var query = db.Sales.AsNoTracking().Where(x => x.AccountId == context.Value.AccountId && !x.IsDeleted);
        ViewData["Page"] = page; ViewData["HasNext"] = await query.CountAsync() > page * 30;
        return View(await query.Include(x => x.Buyer).OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Id)
            .Skip((page - 1) * 30).Take(30).ToListAsync());
    }

    [HttpGet("sales/{id:guid}")]
    public async Task<IActionResult> Details(Guid id)
    {
        var context = await ContextAsync(); if (context is null) return Forbid();
        var sale = await db.Sales.AsNoTracking().Include(x => x.Buyer)
            .Include(x => x.Items).ThenInclude(x => x.HarvestItem).ThenInclude(x => x.HarvestRound).ThenInclude(x => x.Building)
            .SingleOrDefaultAsync(x => x.Id == id && x.AccountId == context.Value.AccountId && !x.IsDeleted);
        if (sale is null) return NotFound();
        return View(new SaleDetailsViewModel { Sale = sale, Stock = new() { Names = await db.MasterOptions.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.Name) } });
    }

    [HttpPost("sales/{id:guid}/cancel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var context = await ContextAsync(); if (context is null) return Forbid();
        try
        {
            await sales.CancelAsync(context.Value.AccountId, context.Value.UserId, id);
            TempData["SuccessMessage"] = "ยกเลิกการขาย คืนสต็อก และนำรายรับที่เชื่อมออกจากยอดสรุปแล้ว";
        }
        catch (LotSaleException ex) { TempData["StockError"] = ex.Message; }
        catch (SqlException ex) when (ex.Number is 1205 or 51000) { TempData["StockError"] = "มีรายการกำลังบันทึก กรุณาลองอีกครั้ง"; }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("sales/{id:guid}/payment")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Payment(Guid id, SalePaymentViewModel model)
    {
        var context = await ContextAsync(); if (context is null) return Forbid();
        if (!ModelState.IsValid) TempData["StockError"] = "ข้อมูลรับชำระไม่ถูกต้อง";
        else try
        {
            await sales.RecordPaymentAsync(context.Value.AccountId, context.Value.UserId, id, model);
            TempData["SuccessMessage"] = "บันทึกรับชำระและเชื่อมรายรับแล้ว";
        }
        catch (LotSaleException ex) { TempData["StockError"] = ex.Message; }
        catch (SqlException ex) when (ex.Number is 1205 or 51000) { TempData["StockError"] = "มีรายการกำลังบันทึก กรุณาลองอีกครั้ง"; }
        return RedirectToAction(nameof(Details), new { id });
    }
}
