using Notenokand.Domain.Common;
using Notenokand.Domain.Enums;

namespace Notenokand.Domain.Entities;

public sealed class Buyer : Entity
{
    public required Guid OwnerUserId { get; set; }
    public required string Name { get; set; }
    public string? TaxId { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
}

public sealed class Sale : Entity
{
    public required Guid OwnerUserId { get; set; }
    public required string DocumentNumber { get; set; }
    public DateOnly SaleDate { get; set; }
    public Guid BuyerId { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ReturnAmount { get; set; }
    public decimal NetAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string? PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public DateOnly? DueDate { get; set; }
    public SaleStatus Status { get; set; }
    public Buyer Buyer { get; set; } = null!;
    public ICollection<SaleItem> Items { get; set; } = [];
}

public sealed class SaleItem : Entity
{
    public Guid SaleId { get; set; }
    public Guid HarvestItemId { get; set; }
    public decimal WeightKg { get; set; }
    public decimal PricePerKg { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal NetAmount { get; set; }
    public Sale Sale { get; set; } = null!;
    public HarvestItem HarvestItem { get; set; } = null!;
}

public sealed class ExpenseCategory : Entity
{
    public required Guid OwnerUserId { get; set; }
    public required string Name { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class FinancialTransaction : Entity
{
    public required Guid OwnerUserId { get; set; }
    public Guid? BuildingId { get; set; }
    public Guid? HarvestRoundId { get; set; }
    public Guid? SaleId { get; set; }
    public Guid? MaintenanceJobId { get; set; }
    public Guid? ExpenseCategoryId { get; set; }
    public TransactionType Type { get; set; }
    public DateOnly TransactionDate { get; set; }
    public DateOnly? PaidOn { get; set; }
    public required string Description { get; set; }
    public decimal Amount { get; set; }
    public string? ReferenceNumber { get; set; }
}
