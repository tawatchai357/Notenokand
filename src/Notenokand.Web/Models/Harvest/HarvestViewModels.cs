using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Notenokand.Domain.Entities;
namespace Notenokand.Web.Models.Harvest;
public sealed class HarvestEditViewModel : IValidatableObject
{
    public Guid? Id { get; set; }
    public string? Version { get; set; }
    [Required(ErrorMessage = "กรุณาเลือกตึกนก")]
    public Guid? BuildingId { get; set; }
    [Required(ErrorMessage = "กรุณาเลือกวันที่เก็บรัง")]
    public DateOnly? HarvestedOn { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
    [StringLength(2000)]
    public string? Notes { get; set; }
    public List<HarvestLineViewModel> Items { get; set; } = [new()];
    public IReadOnlyList<SelectListItem> Buildings { get; set; } = [];
    public IReadOnlyList<SelectListItem> NestTypes { get; set; } = [];
    public IReadOnlyList<SelectListItem> Colors { get; set; } = [];
    public IReadOnlyList<SelectListItem> Conditions { get; set; } = [];
    public decimal TotalWeightKg => Items?.Sum(x => x.WeightKg ?? 0) ?? 0;
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (Items is null || Items.Count is < 1 or > 100)
            yield return new ValidationResult("กรุณาเพิ่มรายการ 1–100 รายการ", [nameof(Items)]);
        if (HarvestedOn > DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7)))
            yield return new ValidationResult("วันที่เก็บรังต้องไม่เป็นวันในอนาคต", [nameof(HarvestedOn)]);
    }
}
public sealed class HarvestLineViewModel : IValidatableObject
{
    [Required(ErrorMessage = "กรุณาเลือกลักษณะรัง")]
    public Guid? NestTypeId { get; set; }
    [Required(ErrorMessage = "กรุณาเลือกสีรัง")]
    public Guid? ColorId { get; set; }
    [Required(ErrorMessage = "กรุณาเลือกสภาพรัง")]
    public Guid? ConditionId { get; set; }
    [Required(ErrorMessage = "กรุณากรอกน้ำหนัก")]
    [Range(typeof(decimal), "0.001", "999999999", ErrorMessage = "น้ำหนักต้องมากกว่า 0 และไม่เกิน 999,999,999 กก.")]
    public decimal? WeightKg { get; set; }
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (WeightKg.HasValue && decimal.Round(WeightKg.Value, 3) != WeightKg.Value)
            yield return new ValidationResult("น้ำหนักรองรับทศนิยมไม่เกิน 3 ตำแหน่ง", [nameof(WeightKg)]);
    }
}
public sealed class HarvestIndexViewModel
{
    public IReadOnlyList<HarvestRound> Rounds { get; init; } = [];
    public IReadOnlyDictionary<Guid, string> OptionNames { get; init; } = new Dictionary<Guid, string>();
}
