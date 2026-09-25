using System.Globalization;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notenokand.Domain.Enums;
using Notenokand.Infrastructure.Persistence;
using Notenokand.Web.Models.Reports;
namespace Notenokand.Web.Controllers;

[Authorize, Route("app/reports")]
public sealed class ReportsController(NotenokandDbContext db) : Controller
{
    private async Task<Guid?> AccountIdAsync()
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return null;
        return await db.AccountUsers.Where(x => x.UserId == userId && x.IsActive && !x.IsDeleted).Select(x => (Guid?)x.AccountId).FirstOrDefaultAsync();
    }
    private static (DateOnly From, DateOnly To) Range(DateOnly? from, DateOnly? to)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
        var start = from ?? new DateOnly(today.Year, 1, 1); var end = to ?? today;
        if (start > end) (start, end) = (end, start);
        return (start, end);
    }
    [HttpGet("")]
    public async Task<IActionResult> Index(DateOnly? from, DateOnly? to)
    {
        var accountId = await AccountIdAsync(); if (!accountId.HasValue) return Forbid();
        var range = Range(from, to);
        var finance = db.FinancialTransactions.AsNoTracking().Where(x => x.AccountId == accountId && !x.IsDeleted && x.TransactionDate >= range.From && x.TransactionDate <= range.To);
        var harvest = db.HarvestRounds.AsNoTracking().Where(x => x.Building.AccountId == accountId && !x.IsDeleted && x.HarvestedOn >= range.From && x.HarvestedOn <= range.To);
        var sales = db.Sales.AsNoTracking().Where(x => x.AccountId == accountId && !x.IsDeleted && x.Status == SaleStatus.Confirmed && x.SaleDate >= range.From && x.SaleDate <= range.To);
        return View(new ReportSummaryViewModel {
            From = range.From, To = range.To,
            Income = await finance.Where(x => x.Type == TransactionType.Income).SumAsync(x => (decimal?)x.Amount) ?? 0,
            Expense = await finance.Where(x => x.Type == TransactionType.Expense).SumAsync(x => (decimal?)x.Amount) ?? 0,
            HarvestKg = await harvest.SumAsync(x => (decimal?)x.TotalWeightKg) ?? 0,
            StockKg = await db.HarvestItems.Where(x => x.HarvestRound.Building.AccountId == accountId && !x.IsDeleted && !x.HarvestRound.IsDeleted).SumAsync(x => (decimal?)x.RemainingWeightKg) ?? 0,
            SalesAmount = await sales.SumAsync(x => (decimal?)x.NetAmount) ?? 0,
            SalesKg = await db.SaleItems.Where(x => x.Sale.AccountId == accountId && x.Sale.Status == SaleStatus.Confirmed && x.Sale.SaleDate >= range.From && x.Sale.SaleDate <= range.To).SumAsync(x => (decimal?)x.WeightKg) ?? 0
        });
    }
    [HttpGet("finance.csv")]
    public async Task<IActionResult> FinanceCsv(DateOnly? from, DateOnly? to)
    {
        var accountId = await AccountIdAsync(); if (!accountId.HasValue) return Forbid();
        var range = Range(from, to);
        var rows = await db.FinancialTransactions.AsNoTracking().Where(x => x.AccountId == accountId && !x.IsDeleted && x.TransactionDate >= range.From && x.TransactionDate <= range.To)
            .OrderBy(x => x.TransactionDate).Select(x => new { x.TransactionDate, x.Type, x.Description, x.Amount, x.BuildingId, x.IsCapitalExpense, x.Counterparty, x.ReferenceNumber }).ToListAsync();
        return Csv("finance", ["วันที่","ประเภท","รายละเอียด","จำนวนเงิน","รหัสตึก","รายจ่ายลงทุน","คู่ค้า","เลขอ้างอิง"],
            rows.Select(x => new[] { x.TransactionDate.ToString("yyyy-MM-dd"), x.Type == TransactionType.Income ? "รายรับ" : "รายจ่าย", x.Description, x.Amount.ToString(CultureInfo.InvariantCulture), x.BuildingId?.ToString() ?? "", x.IsCapitalExpense ? "ใช่" : "ไม่", x.Counterparty ?? "", x.ReferenceNumber ?? "" }));
    }
    [HttpGet("harvest.csv")]
    public async Task<IActionResult> HarvestCsv(DateOnly? from, DateOnly? to)
    {
        var accountId = await AccountIdAsync(); if (!accountId.HasValue) return Forbid();
        var range = Range(from, to);
        var rows = await db.HarvestItems.AsNoTracking().Where(x => x.HarvestRound.Building.AccountId == accountId && !x.IsDeleted && !x.HarvestRound.IsDeleted && x.HarvestRound.HarvestedOn >= range.From && x.HarvestRound.HarvestedOn <= range.To)
            .OrderBy(x => x.HarvestRound.HarvestedOn).Select(x => new { x.HarvestRound.HarvestedOn, x.HarvestRound.RoundNumber, Building=x.HarvestRound.Building.Name, x.NestTypeId, x.ColorId, x.ConditionId, x.WeightKg, x.RemainingWeightKg }).ToListAsync();
        var names = await db.MasterOptions.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.Name);
        string Name(Guid? id) => id.HasValue && names.TryGetValue(id.Value, out var name) ? name : "";
        return Csv("harvest", ["วันที่เก็บ","เลขล็อต","ตึก","ลักษณะรัง","สี","สภาพ","รับเข้า กก.","คงเหลือ กก."],
            rows.Select(x => new[] { x.HarvestedOn.ToString("yyyy-MM-dd"), x.RoundNumber, x.Building, Name(x.NestTypeId), Name(x.ColorId), Name(x.ConditionId), x.WeightKg.ToString(CultureInfo.InvariantCulture), x.RemainingWeightKg.ToString(CultureInfo.InvariantCulture) }));
    }
    [HttpGet("sales.csv")]
    public async Task<IActionResult> SalesCsv(DateOnly? from, DateOnly? to)
    {
        var accountId = await AccountIdAsync(); if (!accountId.HasValue) return Forbid();
        var range = Range(from, to);
        var rows = await db.Sales.AsNoTracking().Where(x => x.AccountId == accountId && !x.IsDeleted && x.SaleDate >= range.From && x.SaleDate <= range.To).Include(x => x.Buyer).Include(x => x.Items).OrderBy(x => x.SaleDate).ToListAsync();
        return Csv("sales", ["วันที่ขาย","เลขที่","ผู้ซื้อ","สถานที่","สถานะ","น้ำหนัก กก.","ยอดขาย"],
            rows.Select(x => new[] { x.SaleDate.ToString("yyyy-MM-dd"), x.DocumentNumber, x.Buyer.Name, x.SaleLocation ?? "", x.Status.ToString(), x.Items.Sum(i => i.WeightKg).ToString(CultureInfo.InvariantCulture), x.NetAmount.ToString(CultureInfo.InvariantCulture) }));
    }
    private FileContentResult Csv(string name, string[] header, IEnumerable<string[]> rows)
    {
        static string Q(string value) => "\"" + value.Replace("\"", "\"\"") + "\"";
        var content = new StringBuilder().AppendLine(string.Join(",", header.Select(Q)));
        foreach (var row in rows) content.AppendLine(string.Join(",", row.Select(Q)));
        var bytes = new UTF8Encoding(true).GetBytes(content.ToString());
        return File(bytes, "text/csv; charset=utf-8", $"{name}-{DateTime.UtcNow.AddHours(7):yyyyMMdd}.csv");
    }
}
