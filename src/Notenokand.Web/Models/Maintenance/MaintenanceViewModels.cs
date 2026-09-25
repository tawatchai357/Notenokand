using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Notenokand.Domain.Enums;

namespace Notenokand.Web.Models.Maintenance;

public sealed class MaintenanceEditViewModel : IValidatableObject
{
    public Guid? Id { get; set; }
    [Required(ErrorMessage = "กรุณาเลือกตึกนก")] public Guid? BuildingId { get; set; }
    [Required(ErrorMessage = "กรุณาระบุปัญหาหรืองานที่ต้องทำ"), StringLength(1000)] public string Issue { get; set; } = "";
    public MaintenanceType Type { get; set; } = MaintenanceType.Preventive;
    public MaintenancePriority Priority { get; set; } = MaintenancePriority.Normal;
    public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Reported;
    [Range(typeof(decimal), "0", "999999999", ErrorMessage = "ค่าอะไหล่ไม่ถูกต้อง")] public decimal PartsCost { get; set; }
    [Range(typeof(decimal), "0", "999999999", ErrorMessage = "ค่าแรงไม่ถูกต้อง")] public decimal LaborCost { get; set; }
    [Range(typeof(decimal), "0", "999999", ErrorMessage = "ชั่วโมงหยุดทำงานไม่ถูกต้อง")] public decimal DowntimeHours { get; set; }
    [StringLength(1000)] public string? AcceptanceResult { get; set; }
    public DateOnly? NextInspectionOn { get; set; }
    public IReadOnlyList<SelectListItem> Buildings { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Status == MaintenanceStatus.Completed && string.IsNullOrWhiteSpace(AcceptanceResult))
            yield return new ValidationResult("กรุณาระบุผลการตรวจรับเมื่อปิดงาน", [nameof(AcceptanceResult)]);
    }
}

public sealed class MaintenanceIndexViewModel
{
    public IReadOnlyList<Notenokand.Domain.Entities.MaintenanceJob> Jobs { get; init; } = [];
    public IReadOnlyDictionary<Guid, string> BuildingNames { get; init; } = new Dictionary<Guid, string>();
    public MaintenanceStatus? Status { get; init; }
    public int OpenCount { get; init; }
    public decimal TotalCost { get; init; }
}
