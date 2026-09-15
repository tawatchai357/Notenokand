using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Notenokand.Domain.Enums;

namespace Notenokand.Web.Models.Finance;

public sealed class FinanceIndexViewModel
{
    public string Period { get; init; } = string.Empty;
    public string PeriodDisplay { get; init; } = string.Empty;
    public Guid? BuildingId { get; init; }
    public Guid? CategoryId { get; init; }
    public TransactionType? Type { get; init; }
    public decimal TotalIncome { get; init; }
    public decimal TotalExpense { get; init; }
    public decimal NetAmount => TotalIncome - TotalExpense;
    public IReadOnlyList<SelectListItem> Buildings { get; init; } = [];
    public IReadOnlyList<SelectListItem> Categories { get; init; } = [];
    public IReadOnlyList<FinanceCardViewModel> Transactions { get; init; } = [];
}

public sealed class FinanceCardViewModel
{
    public Guid Id { get; init; }
    public TransactionType Type { get; init; }
    public DateOnly TransactionDate { get; init; }
    public required string Description { get; init; }
    public decimal Amount { get; init; }
    public string CategoryName { get; init; } = "ไม่ระบุหมวดหมู่";
    public string BuildingName { get; init; } = "ส่วนกลาง";
    public string PaymentMethodName { get; init; } = string.Empty;
    public string? Counterparty { get; init; }
    public string? SaleLocation { get; init; }
    public decimal? AveragePricePerKg { get; init; }
    public string? ReferenceNumber { get; init; }
    public int ReceiptCount { get; init; }
    public Guid? FirstReceiptId { get; init; }
}

public sealed class FinanceEditViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "กรุณาเลือกประเภทรายการ")]
    [Display(Name = "ประเภทรายการ")]
    public TransactionType Type { get; set; } = TransactionType.Expense;

    [Required(ErrorMessage = "กรุณาเลือกวันที่รายการ")]
    [Display(Name = "วันที่รายการ")]
    public DateOnly TransactionDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required(ErrorMessage = "กรุณากรอกรายละเอียด")]
    [StringLength(500)]
    [Display(Name = "รายละเอียดรายการ")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "กรุณากรอกจำนวนเงิน")]
    [Range(typeof(decimal), "0.01", "9999999999999999", ErrorMessage = "จำนวนเงินต้องมากกว่า 0")]
    [Display(Name = "จำนวนเงิน (บาท)")]
    public decimal Amount { get; set; }

    [Display(Name = "ตึกนก")]
    public Guid? BuildingId { get; set; }

    [Required(ErrorMessage = "กรุณาเลือกหมวดหมู่")]
    [Display(Name = "หมวดหมู่")]
    public Guid? CategoryId { get; set; }

    [Display(Name = "วิธีชำระเงิน")]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.BankTransfer;

    [StringLength(200)]
    [Display(Name = "ผู้รับเงิน/ผู้จ่ายเงิน")]
    public string? Counterparty { get; set; }

    [StringLength(300)]
    [Display(Name = "สถานที่ขาย")]
    public string? SaleLocation { get; set; }

    [Range(typeof(decimal), "0.01", "9999999999999999", ErrorMessage = "ราคาเฉลี่ยต่อกิโลกรัมต้องมากกว่า 0")]
    [Display(Name = "ราคาเฉลี่ยต่อ กก. (บาท)")]
    public decimal? AveragePricePerKg { get; set; }
    [StringLength(100)]
    [Display(Name = "เลขที่เอกสาร/เลขอ้างอิง")]
    public string? ReferenceNumber { get; set; }

    [StringLength(2000)]
    [Display(Name = "หมายเหตุ")]
    public string? Notes { get; set; }

    [Display(Name = "แนบใบเสร็จหรือหลักฐาน")]
    public IFormFile? Receipt { get; set; }

    public IReadOnlyList<SelectListItem> Buildings { get; set; } = [];
    public IReadOnlyList<SelectListItem> Categories { get; set; } = [];
    public IReadOnlyList<FinanceCategoryOptionViewModel> CategoryOptions { get; set; } = [];
    public IReadOnlyList<ReceiptViewModel> ExistingReceipts { get; set; } = [];
    public bool IsEdit => Id.HasValue;
}

public sealed class FinanceCategoryOptionViewModel
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public TransactionType Type { get; init; }
}

public sealed class ReceiptViewModel
{
    public Guid Id { get; init; }
    public required string FileName { get; init; }
    public long SizeBytes { get; init; }
}

public sealed class FinanceCategoryPageViewModel
{
    public CategoryEditViewModel NewCategory { get; set; } = new();
    public IReadOnlyList<CategoryCardViewModel> Categories { get; init; } = [];
}

public sealed class CategoryEditViewModel
{
    [Required(ErrorMessage = "กรุณากรอกชื่อหมวดหมู่")]
    [StringLength(150)]
    [Display(Name = "ชื่อหมวดหมู่")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "ประเภท")]
    public TransactionType Type { get; set; } = TransactionType.Expense;
}

public sealed class CategoryCardViewModel
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public TransactionType Type { get; init; }
    public bool IsSystem { get; init; }
    public bool IsActive { get; init; }
}