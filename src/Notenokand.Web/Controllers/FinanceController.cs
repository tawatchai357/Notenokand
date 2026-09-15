using System.Globalization;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Notenokand.Domain.Entities;
using Notenokand.Domain.Enums;
using Notenokand.Infrastructure.Identity;
using Notenokand.Infrastructure.Persistence;
using Notenokand.Web.Models.Finance;

namespace Notenokand.Web.Controllers;

[Authorize]
[Route("app/finance")]
public sealed class FinanceController(NotenokandDbContext db, UserManager<ApplicationUser> userManager, IWebHostEnvironment environment) : Controller
{
    private const long MaxReceiptBytes = 10 * 1024 * 1024;
    private static readonly Dictionary<string, string> AllowedReceiptTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg", ["image/png"] = ".png", ["image/webp"] = ".webp", ["application/pdf"] = ".pdf"
    };
    private static readonly string[] ThaiMonths = ["มกราคม", "กุมภาพันธ์", "มีนาคม", "เมษายน", "พฤษภาคม", "มิถุนายน", "กรกฎาคม", "สิงหาคม", "กันยายน", "ตุลาคม", "พฤศจิกายน", "ธันวาคม"];

    [HttpGet("")]
    public async Task<IActionResult> Index(string? period, Guid? buildingId, Guid? categoryId, TransactionType? type)
    {
        var context = await GetAccountContextAsync();
        if (context is null) return RedirectToAction("Index", "Onboarding");
        await EnsureDefaultCategoriesAsync(context.Value.AccountId, context.Value.UserId);
        var month = ParsePeriod(period);
        var start = new DateOnly(month.Year, month.Month, 1);
        var end = start.AddMonths(1);
        var monthQuery = db.FinancialTransactions.AsNoTracking().Where(x => x.AccountId == context.Value.AccountId && !x.IsDeleted && x.TransactionDate >= start && x.TransactionDate < end);
        var income = await monthQuery.Where(x => x.Type == TransactionType.Income).SumAsync(x => (decimal?)x.Amount) ?? 0;
        var expense = await monthQuery.Where(x => x.Type == TransactionType.Expense).SumAsync(x => (decimal?)x.Amount) ?? 0;
        var query = monthQuery;
        if (buildingId.HasValue) query = query.Where(x => x.BuildingId == buildingId);
        if (categoryId.HasValue) query = query.Where(x => x.ExpenseCategoryId == categoryId);
        if (type.HasValue) query = query.Where(x => x.Type == type);
        var entities = await query.OrderByDescending(x => x.TransactionDate).ThenByDescending(x => x.CreatedAt).ToListAsync();
        var categoryIds = entities.Where(x => x.ExpenseCategoryId.HasValue).Select(x => x.ExpenseCategoryId!.Value).Distinct().ToArray();
        var buildingIds = entities.Where(x => x.BuildingId.HasValue).Select(x => x.BuildingId!.Value).Distinct().ToArray();
        var categories = await db.ExpenseCategories.AsNoTracking().Where(x => x.AccountId == context.Value.AccountId && categoryIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name);
        var buildings = await db.BirdBuildings.AsNoTracking().Where(x => x.AccountId == context.Value.AccountId && buildingIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name);
        var transactionIds = entities.Select(x => x.Id).ToArray();
        var receipts = await db.TransactionReceipts.AsNoTracking().Where(x => x.AccountId == context.Value.AccountId && transactionIds.Contains(x.FinancialTransactionId) && !x.IsDeleted).GroupBy(x => x.FinancialTransactionId).Select(x => new { TransactionId = x.Key, Count = x.Count(), FirstId = x.Min(y => y.Id) }).ToDictionaryAsync(x => x.TransactionId);
        var cards = entities.Select(x => new FinanceCardViewModel
        {
            Id = x.Id, Type = x.Type, TransactionDate = x.TransactionDate, Description = x.Description, Amount = x.Amount,
            CategoryName = x.ExpenseCategoryId.HasValue && categories.TryGetValue(x.ExpenseCategoryId.Value, out var category) ? category : "ไม่ระบุหมวดหมู่",
            BuildingName = x.BuildingId.HasValue && buildings.TryGetValue(x.BuildingId.Value, out var building) ? building : "ส่วนกลาง",
            PaymentMethodName = PaymentMethodName(x.PaymentMethod), Counterparty = x.Counterparty, ReferenceNumber = x.ReferenceNumber,
            ReceiptCount = receipts.TryGetValue(x.Id, out var receipt) ? receipt.Count : 0,
            FirstReceiptId = receipts.TryGetValue(x.Id, out receipt) ? receipt.FirstId : null
        }).ToList();
        return View(new FinanceIndexViewModel
        {
            Period = $"{month.Year:0000}-{month.Month:00}", PeriodDisplay = $"{ThaiMonths[month.Month - 1]} {month.Year + 543}",
            BuildingId = buildingId, CategoryId = categoryId, Type = type, TotalIncome = income, TotalExpense = expense, Transactions = cards,
            Buildings = await LoadBuildingSelectAsync(context.Value.AccountId, includeInactive: true),
            Categories = await LoadCategorySelectAsync(context.Value.AccountId, includeInactive: true)
        });
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(TransactionType type = TransactionType.Expense)
    {
        var context = await GetAccountContextAsync();
        if (context is null) return RedirectToAction("Index", "Onboarding");
        await EnsureDefaultCategoriesAsync(context.Value.AccountId, context.Value.UserId);
        var model = new FinanceEditViewModel { Type = type, TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7)) };
        await LoadEditorOptionsAsync(model, context.Value.AccountId);
        return View("Edit", model);
    }

    [HttpPost("create")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FinanceEditViewModel model)
    {
        var context = await GetAccountContextAsync();
        if (context is null) return Forbid();
        await ValidateEditorAsync(model, context.Value.AccountId);
        if (!ModelState.IsValid) { await LoadEditorOptionsAsync(model, context.Value.AccountId); return View("Edit", model); }
        var transaction = new FinancialTransaction
        {
            AccountId = context.Value.AccountId, OwnerUserId = context.Value.UserId, BuildingId = model.BuildingId,
            ExpenseCategoryId = model.CategoryId, Type = model.Type, TransactionDate = model.TransactionDate, PaidOn = model.TransactionDate,
            Description = model.Description.Trim(), Amount = model.Amount, PaymentMethod = model.PaymentMethod,
            Counterparty = Clean(model.Counterparty), ReferenceNumber = Clean(model.ReferenceNumber), Notes = Clean(model.Notes), CreatedByUserId = context.Value.UserId
        };
        string? writtenPath = null;
        try
        {
            db.FinancialTransactions.Add(transaction);
            if (model.Receipt is not null)
            {
                var uploaded = await StoreReceiptAsync(model.Receipt, context.Value.AccountId, transaction.Id, context.Value.UserId);
                writtenPath = uploaded.PhysicalPath; db.TransactionReceipts.Add(uploaded.Entity);
            }
            await db.SaveChangesAsync();
        }
        catch { if (writtenPath is not null && System.IO.File.Exists(writtenPath)) System.IO.File.Delete(writtenPath); throw; }
        TempData["SuccessMessage"] = "บันทึกรายการเรียบร้อยแล้ว";
        return RedirectToAction(nameof(Index), new { period = $"{model.TransactionDate.Year:0000}-{model.TransactionDate.Month:00}" });
    }

    [HttpGet("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var context = await GetAccountContextAsync(); if (context is null) return Forbid();
        var entity = await db.FinancialTransactions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.AccountId == context.Value.AccountId && !x.IsDeleted);
        if (entity is null) return NotFound();
        var model = new FinanceEditViewModel
        {
            Id = entity.Id, Type = entity.Type, TransactionDate = entity.TransactionDate, Description = entity.Description,
            Amount = entity.Amount, BuildingId = entity.BuildingId, CategoryId = entity.ExpenseCategoryId,
            PaymentMethod = entity.PaymentMethod, Counterparty = entity.Counterparty, ReferenceNumber = entity.ReferenceNumber, Notes = entity.Notes,
            ExistingReceipts = await db.TransactionReceipts.AsNoTracking().Where(x => x.AccountId == context.Value.AccountId && x.FinancialTransactionId == id && !x.IsDeleted).Select(x => new ReceiptViewModel { Id = x.Id, FileName = x.OriginalFileName, SizeBytes = x.SizeBytes }).ToListAsync()
        };
        await LoadEditorOptionsAsync(model, context.Value.AccountId, includeBuildingId: entity.BuildingId);
        return View(model);
    }

    [HttpPost("{id:guid}/edit")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, FinanceEditViewModel model)
    {
        var context = await GetAccountContextAsync(); if (context is null) return Forbid();
        var entity = await db.FinancialTransactions.SingleOrDefaultAsync(x => x.Id == id && x.AccountId == context.Value.AccountId && !x.IsDeleted);
        if (entity is null) return NotFound();
        model.Id = id; await ValidateEditorAsync(model, context.Value.AccountId, entity.BuildingId);
        if (!ModelState.IsValid) { await LoadEditorOptionsAsync(model, context.Value.AccountId, entity.BuildingId); model.ExistingReceipts = await LoadReceiptsAsync(context.Value.AccountId, id); return View(model); }
        entity.Type = model.Type; entity.TransactionDate = model.TransactionDate; entity.PaidOn = model.TransactionDate;
        entity.Description = model.Description.Trim(); entity.Amount = model.Amount; entity.BuildingId = model.BuildingId;
        entity.ExpenseCategoryId = model.CategoryId; entity.PaymentMethod = model.PaymentMethod;
        entity.Counterparty = Clean(model.Counterparty); entity.ReferenceNumber = Clean(model.ReferenceNumber); entity.Notes = Clean(model.Notes);
        entity.UpdatedAt = DateTimeOffset.UtcNow; entity.UpdatedByUserId = context.Value.UserId;
        string? writtenPath = null;
        try
        {
            if (model.Receipt is not null) { var uploaded = await StoreReceiptAsync(model.Receipt, context.Value.AccountId, id, context.Value.UserId); writtenPath = uploaded.PhysicalPath; db.TransactionReceipts.Add(uploaded.Entity); }
            await db.SaveChangesAsync();
        }
        catch { if (writtenPath is not null && System.IO.File.Exists(writtenPath)) System.IO.File.Delete(writtenPath); throw; }
        TempData["SuccessMessage"] = "แก้ไขรายการเรียบร้อยแล้ว";
        return RedirectToAction(nameof(Index), new { period = $"{model.TransactionDate.Year:0000}-{model.TransactionDate.Month:00}" });
    }

    [HttpPost("{id:guid}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var context = await GetAccountContextAsync(); if (context is null) return Forbid();
        var entity = await db.FinancialTransactions.SingleOrDefaultAsync(x => x.Id == id && x.AccountId == context.Value.AccountId && !x.IsDeleted);
        if (entity is null) return NotFound();
        entity.IsDeleted = true; entity.UpdatedAt = DateTimeOffset.UtcNow; entity.UpdatedByUserId = context.Value.UserId;
        await db.SaveChangesAsync(); TempData["SuccessMessage"] = "ลบรายการแล้ว"; return RedirectToAction(nameof(Index));
    }

    [HttpGet("receipts/{id:guid}")]
    public async Task<IActionResult> Receipt(Guid id)
    {
        var context = await GetAccountContextAsync(); if (context is null) return Forbid();
        var receipt = await db.TransactionReceipts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.AccountId == context.Value.AccountId && !x.IsDeleted);
        if (receipt is null) return NotFound();
        var path = ResolveReceiptPath(receipt.StorageKey); if (!System.IO.File.Exists(path)) return NotFound();
        return PhysicalFile(path, receipt.ContentType, enableRangeProcessing: true);
    }

    [HttpPost("receipts/{id:guid}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReceipt(Guid id)
    {
        var context = await GetAccountContextAsync(); if (context is null) return Forbid();
        var receipt = await db.TransactionReceipts.Include(x => x.FinancialTransaction).SingleOrDefaultAsync(x => x.Id == id && x.AccountId == context.Value.AccountId && !x.IsDeleted);
        if (receipt is null) return NotFound();
        receipt.IsDeleted = true; receipt.UpdatedAt = DateTimeOffset.UtcNow; receipt.UpdatedByUserId = context.Value.UserId; await db.SaveChangesAsync();
        TempData["SuccessMessage"] = "ลบไฟล์แนบแล้ว"; return RedirectToAction(nameof(Edit), new { id = receipt.FinancialTransactionId });
    }

    [HttpGet("categories")]
    public async Task<IActionResult> Categories()
    {
        var context = await GetAccountContextAsync(); if (context is null) return Forbid();
        await EnsureDefaultCategoriesAsync(context.Value.AccountId, context.Value.UserId);
        var items = await db.ExpenseCategories.AsNoTracking().Where(x => x.AccountId == context.Value.AccountId && !x.IsDeleted).OrderBy(x => x.Type).ThenBy(x => x.SortOrder).ThenBy(x => x.Name).Select(x => new CategoryCardViewModel { Id = x.Id, Name = x.Name, Type = x.Type, IsSystem = x.IsSystem, IsActive = x.IsActive }).ToListAsync();
        return View(new FinanceCategoryPageViewModel { Categories = items });
    }

    [HttpPost("categories")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory([Bind(Prefix = "NewCategory")] CategoryEditViewModel model)
    {
        var context = await GetAccountContextAsync(); if (context is null) return Forbid();
        var name = model.Name?.Trim() ?? string.Empty;
        if (await db.ExpenseCategories.AnyAsync(x => x.AccountId == context.Value.AccountId && x.Type == model.Type && x.Name == name && !x.IsDeleted)) ModelState.AddModelError(nameof(model.Name), "มีหมวดหมู่นี้แล้ว");
        if (!ModelState.IsValid)
        {
            var items = await db.ExpenseCategories.AsNoTracking().Where(x => x.AccountId == context.Value.AccountId && !x.IsDeleted).OrderBy(x => x.Type).ThenBy(x => x.SortOrder).Select(x => new CategoryCardViewModel { Id = x.Id, Name = x.Name, Type = x.Type, IsSystem = x.IsSystem, IsActive = x.IsActive }).ToListAsync();
            return View("Categories", new FinanceCategoryPageViewModel { NewCategory = model, Categories = items });
        }
        db.ExpenseCategories.Add(new ExpenseCategory { AccountId = context.Value.AccountId, OwnerUserId = context.Value.UserId, Name = name, Type = model.Type, SortOrder = 1000, CreatedByUserId = context.Value.UserId });
        await db.SaveChangesAsync(); TempData["SuccessMessage"] = "เพิ่มหมวดหมู่แล้ว"; return RedirectToAction(nameof(Categories));
    }

    [HttpPost("categories/{id:guid}/toggle")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleCategory(Guid id)
    {
        var context = await GetAccountContextAsync(); if (context is null) return Forbid();
        var category = await db.ExpenseCategories.SingleOrDefaultAsync(x => x.Id == id && x.AccountId == context.Value.AccountId && !x.IsDeleted);
        if (category is null) return NotFound();
        category.IsActive = !category.IsActive; category.UpdatedAt = DateTimeOffset.UtcNow; category.UpdatedByUserId = context.Value.UserId;
        await db.SaveChangesAsync(); TempData["SuccessMessage"] = category.IsActive ? "เปิดใช้หมวดหมู่แล้ว" : "ปิดใช้หมวดหมู่แล้ว"; return RedirectToAction(nameof(Categories));
    }

    private async Task ValidateEditorAsync(FinanceEditViewModel model, Guid accountId, Guid? existingBuildingId = null)
    {
        if (model.BuildingId.HasValue && !await db.BirdBuildings.AnyAsync(x => x.Id == model.BuildingId && x.AccountId == accountId && !x.IsDeleted && (x.Status == BuildingStatus.Active || x.Id == existingBuildingId))) ModelState.AddModelError(nameof(model.BuildingId), "ตึกที่เลือกไม่พร้อมใช้งาน");
        if (!model.CategoryId.HasValue || !await db.ExpenseCategories.AnyAsync(x => x.Id == model.CategoryId && x.AccountId == accountId && x.Type == model.Type && x.IsActive && !x.IsDeleted)) ModelState.AddModelError(nameof(model.CategoryId), "หมวดหมู่ไม่ตรงกับประเภทรายการหรือถูกปิดใช้งาน");
        if (model.Receipt is not null)
        {
            if (model.Receipt.Length <= 0 || model.Receipt.Length > MaxReceiptBytes) ModelState.AddModelError(nameof(model.Receipt), "ไฟล์ต้องมีขนาดไม่เกิน 10 MB");
            if (DetectReceipt(model.Receipt) is null) ModelState.AddModelError(nameof(model.Receipt), "รองรับเฉพาะ JPG, PNG, WebP หรือ PDF");
        }
    }

    private async Task LoadEditorOptionsAsync(FinanceEditViewModel model, Guid accountId, Guid? includeBuildingId = null)
    {
        model.Buildings = await db.BirdBuildings.AsNoTracking().Where(x => x.AccountId == accountId && !x.IsDeleted && (x.Status == BuildingStatus.Active || x.Id == includeBuildingId)).OrderBy(x => x.Name).Select(x => new SelectListItem(x.Name + (x.Status == BuildingStatus.Inactive ? " (ไม่ใช้งาน)" : ""), x.Id.ToString())).ToListAsync();
        model.CategoryOptions = await db.ExpenseCategories.AsNoTracking().Where(x => x.AccountId == accountId && x.IsActive && !x.IsDeleted).OrderBy(x => x.Type).ThenBy(x => x.SortOrder).ThenBy(x => x.Name).Select(x => new FinanceCategoryOptionViewModel { Id = x.Id, Name = x.Name, Type = x.Type }).ToListAsync();
    }
    private async Task<IReadOnlyList<ReceiptViewModel>> LoadReceiptsAsync(Guid accountId, Guid transactionId) => await db.TransactionReceipts.AsNoTracking().Where(x => x.AccountId == accountId && x.FinancialTransactionId == transactionId && !x.IsDeleted).Select(x => new ReceiptViewModel { Id = x.Id, FileName = x.OriginalFileName, SizeBytes = x.SizeBytes }).ToListAsync();
    private async Task<IReadOnlyList<SelectListItem>> LoadBuildingSelectAsync(Guid accountId, bool includeInactive) => await db.BirdBuildings.AsNoTracking().Where(x => x.AccountId == accountId && !x.IsDeleted && (includeInactive || x.Status == BuildingStatus.Active)).OrderBy(x => x.Name).Select(x => new SelectListItem(x.Name + (x.Status == BuildingStatus.Inactive ? " (ไม่ใช้งาน)" : ""), x.Id.ToString())).ToListAsync();
    private async Task<IReadOnlyList<SelectListItem>> LoadCategorySelectAsync(Guid accountId, bool includeInactive) => await db.ExpenseCategories.AsNoTracking().Where(x => x.AccountId == accountId && !x.IsDeleted && (includeInactive || x.IsActive)).OrderBy(x => x.Type).ThenBy(x => x.SortOrder).Select(x => new SelectListItem((x.Type == TransactionType.Income ? "รายรับ · " : "ค่าใช้จ่าย · ") + x.Name, x.Id.ToString())).ToListAsync();

    private async Task EnsureDefaultCategoriesAsync(Guid accountId, Guid userId)
    {
        if (await db.ExpenseCategories.AnyAsync(x => x.AccountId == accountId)) return;
        var defaults = new (TransactionType Type, string Name)[] { (TransactionType.Income,"ขายรังนก"),(TransactionType.Income,"รายได้อื่น"),(TransactionType.Expense,"ค่าไฟฟ้า"),(TransactionType.Expense,"ค่าแรง"),(TransactionType.Expense,"ซ่อมบำรุง"),(TransactionType.Expense,"อุปกรณ์และวัสดุ"),(TransactionType.Expense,"ค่าก่อสร้าง"),(TransactionType.Expense,"ค่าเดินทาง"),(TransactionType.Expense,"ค่าใช้จ่ายอื่น") };
        var order = 0; foreach (var item in defaults) db.ExpenseCategories.Add(new ExpenseCategory { AccountId = accountId, OwnerUserId = userId, Name = item.Name, Type = item.Type, IsSystem = true, SortOrder = order++, CreatedByUserId = userId });
        await db.SaveChangesAsync();
    }

    private async Task<(TransactionReceipt Entity, string PhysicalPath)> StoreReceiptAsync(IFormFile file, Guid accountId, Guid transactionId, Guid userId)
    {
        var receiptId = Guid.NewGuid();
        await using var input = file.OpenReadStream(); using var memory = new MemoryStream(); await input.CopyToAsync(memory); var bytes = memory.ToArray();
        var detected = DetectReceipt(bytes) ?? throw new InvalidOperationException("Unsupported receipt format.");
        var storageKey = Path.Combine(accountId.ToString("N"), transactionId.ToString("N"), receiptId.ToString("N") + detected.Extension).Replace(Path.DirectorySeparatorChar, '/');
        var physicalPath = ResolveReceiptPath(storageKey); Directory.CreateDirectory(Path.GetDirectoryName(physicalPath)!);
        await System.IO.File.WriteAllBytesAsync(physicalPath, bytes);
        var originalName = Path.GetFileName(file.FileName); if (string.IsNullOrWhiteSpace(originalName)) originalName = "receipt" + detected.Extension; if (originalName.Length > 255) originalName = originalName[..255];
        return (new TransactionReceipt { Id = receiptId, AccountId = accountId, FinancialTransactionId = transactionId, StorageKey = storageKey, OriginalFileName = originalName, ContentType = detected.ContentType, SizeBytes = bytes.LongLength, Sha256 = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(), CreatedByUserId = userId }, physicalPath);
    }
    private static (string Extension, string ContentType)? DetectReceipt(IFormFile file)
    {
        using var stream = file.OpenReadStream(); Span<byte> header = stackalloc byte[12]; var read = stream.Read(header); return DetectReceipt(header[..read]);
    }
    private static (string Extension, string ContentType)? DetectReceipt(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF) return (".jpg", "image/jpeg");
        if (bytes.Length >= 8 && bytes[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })) return (".png", "image/png");
        if (bytes.Length >= 12 && bytes[..4].SequenceEqual("RIFF"u8) && bytes[8..12].SequenceEqual("WEBP"u8)) return (".webp", "image/webp");
        if (bytes.Length >= 5 && bytes[..5].SequenceEqual("%PDF-"u8)) return (".pdf", "application/pdf");
        return null;
    }
    private string ResolveReceiptPath(string storageKey)
    {
        var root = Path.GetFullPath(Path.Combine(environment.ContentRootPath, "App_Data", "receipts"));
        var path = Path.GetFullPath(Path.Combine(root, storageKey.Replace('/', Path.DirectorySeparatorChar)));
        if (!path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Invalid receipt path.");
        return path;
    }
    private async Task<(Guid AccountId, Guid UserId)?> GetAccountContextAsync()
    {
        if (!Guid.TryParse(userManager.GetUserId(User), out var userId)) return null;
        var accountId = await db.AccountUsers.Where(x => x.UserId == userId && x.IsActive && !x.IsDeleted).Select(x => (Guid?)x.AccountId).FirstOrDefaultAsync();
        return accountId.HasValue ? (accountId.Value, userId) : null;
    }
    private static DateOnly ParsePeriod(string? value) => DateOnly.TryParseExact(value + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
    private static string PaymentMethodName(PaymentMethod method) => method switch { PaymentMethod.Cash => "เงินสด", PaymentMethod.BankTransfer => "โอนเงิน", PaymentMethod.CreditCard => "บัตรเครดิต", _ => "อื่น ๆ" };
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}