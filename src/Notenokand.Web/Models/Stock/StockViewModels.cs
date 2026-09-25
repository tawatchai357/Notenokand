using System.ComponentModel.DataAnnotations;
using Notenokand.Domain.Entities;
using Notenokand.Domain.Enums;
namespace Notenokand.Web.Models.Stock;

public sealed class StockPageViewModel
{
    public IReadOnlyList<HarvestItem> Items { get; init; } = [];
    public IReadOnlyDictionary<Guid, string> Names { get; init; } = new Dictionary<Guid, string>();
    public string Name(Guid? id) => id.HasValue && Names.TryGetValue(id.Value, out var name) ? name : "ไม่ระบุ";
    public decimal ReceivedKg => Items.Sum(x => x.WeightKg);
    public decimal RemainingKg => Items.Sum(x => x.RemainingWeightKg);
    public decimal SoldKg => ReceivedKg - RemainingKg;
    public string Label(HarvestItem item) => $"{item.HarvestRound.Building.Name} · {item.HarvestRound.HarvestedOn.ToString("dd/MM/yyyy", new System.Globalization.CultureInfo("th-TH"))} · {Name(item.NestTypeId)} / {Name(item.ColorId)} / {Name(item.ConditionId)} · เหลือ {item.RemainingWeightKg:N3} กก.";
}

public sealed class SaleCreateViewModel : IValidatableObject
{
    public Guid RequestId { get; set; } = Guid.NewGuid();
    [Required, StringLength(200)] public string BuyerName { get; set; } = "";
    [Required, StringLength(300)] public string SaleLocation { get; set; } = "";
    public DateOnly SaleDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
    [EnumDataType(typeof(PaymentMethod))] public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.BankTransfer;
    [StringLength(2000)] public string? Notes { get; set; }
    public bool ReceivedInFull { get; set; }
    [Range(typeof(decimal), "0", "99999999999999", ErrorMessage = "ยอดรับเงินไม่ถูกต้อง")] public decimal? InitialPaidAmount { get; set; }
    public DateOnly? DueDate { get; set; }
    public List<SaleLineInput> Items { get; set; } = [new()];
    public StockPageViewModel Stock { get; set; } = new();
    public decimal TotalKg => Items.Sum(x => x.WeightKg);
    public decimal TotalAmount => Items.Sum(x => x.Amount);
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (RequestId == Guid.Empty) yield return new("รหัสคำขอไม่ถูกต้อง กรุณาเปิดแบบฟอร์มใหม่");
        if (SaleDate == default || SaleDate > DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7)))
            yield return new("วันที่ขายต้องไม่อยู่ในอนาคต", [nameof(SaleDate)]);
        var paid = ReceivedInFull ? TotalAmount : InitialPaidAmount ?? 0;
        if (paid > TotalAmount) yield return new("ยอดรับเงินต้องไม่เกินยอดขายรวม", [nameof(InitialPaidAmount)]);
        if (paid < TotalAmount && !DueDate.HasValue) yield return new("กรุณาระบุวันครบกำหนดสำหรับยอดค้างชำระ", [nameof(DueDate)]);
        if (DueDate.HasValue && DueDate.Value < SaleDate) yield return new("วันครบกำหนดต้องไม่ก่อนวันที่ขาย", [nameof(DueDate)]);
        if (Items is null || Items.Count is < 1 or > 100)
            yield return new("เลือกสินค้าระหว่าง 1–100 รายการ", [nameof(Items)]);
        else
        {
            if (Items.Select(x => x.HarvestItemId).Distinct().Count() != Items.Count)
                yield return new("เลือกสินค้ารายการเดียวกันซ้ำ กรุณารวมน้ำหนักในแถวเดียว", [nameof(Items)]);
            if (TotalAmount > 99999999999999m) yield return new("ยอดขายรวมสูงเกินขีดจำกัด");
        }
    }
}

public sealed class SalePaymentViewModel : IValidatableObject
{
    public Guid RequestId { get; set; } = Guid.NewGuid();
    [Range(typeof(decimal), "0.01", "99999999999999", ErrorMessage = "ยอดรับชำระต้องมากกว่า 0")] public decimal Amount { get; set; }
    public DateOnly PaidOn { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
    [EnumDataType(typeof(PaymentMethod))] public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.BankTransfer;
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (RequestId == Guid.Empty) yield return new("รหัสคำขอไม่ถูกต้อง");
        if (PaidOn == default || PaidOn > DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7)))
            yield return new("วันที่รับเงินต้องไม่อยู่ในอนาคต", [nameof(PaidOn)]);
    }
}

public sealed class SaleLineInput : IValidatableObject
{
    public Guid HarvestItemId { get; set; }
    [Range(typeof(decimal), "0.001", "99999999")] public decimal WeightKg { get; set; }
    [Range(typeof(decimal), "0.01", "99999999")] public decimal PricePerKg { get; set; }
    public decimal Amount => WeightKg is > 0 and <= 99999999m && PricePerKg is > 0 and <= 99999999m
        ? decimal.Round(WeightKg * PricePerKg, 2, MidpointRounding.AwayFromZero) : 0m;
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (HarvestItemId == Guid.Empty) yield return new("กรุณาเลือกสินค้าจากล็อต", [nameof(HarvestItemId)]);
        if (WeightKg != decimal.Round(WeightKg, 3)) yield return new("น้ำหนักใช้ทศนิยมได้ไม่เกิน 3 ตำแหน่ง", [nameof(WeightKg)]);
        if (PricePerKg != decimal.Round(PricePerKg, 2)) yield return new("ราคาใช้ทศนิยมได้ไม่เกิน 2 ตำแหน่ง", [nameof(PricePerKg)]);
        if (Amount <= 0) yield return new("ยอดเงินต่อรายการต้องไม่น้อยกว่า 0.01 บาท");
    }
}

public sealed class SaleDetailsViewModel
{
    public required Sale Sale { get; init; }
    public StockPageViewModel Stock { get; init; } = new();
    public decimal TotalKg => Sale.Items.Sum(x => x.WeightKg);
    public decimal AveragePrice => TotalKg > 0 ? Sale.NetAmount / TotalKg : 0;
}
