using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Notenokand.Domain.Enums;

namespace Notenokand.Web.Models.Calendar;

public sealed class CalendarIndexViewModel
{
    public string Period { get; init; } = string.Empty;
    public string PeriodDisplay { get; init; } = string.Empty;
    public DateOnly MonthStart { get; init; }
    public string PreviousPeriod { get; init; } = string.Empty;
    public string NextPeriod { get; init; } = string.Empty;
    public IReadOnlyList<AppointmentCardViewModel> Appointments { get; init; } = [];
}

public sealed class AppointmentCardViewModel
{
    public Guid Id { get; init; }
    public AppointmentType Type { get; init; }
    public AppointmentStatus Status { get; init; }
    public DateOnly ScheduledDate { get; init; }
    public TimeOnly? ScheduledTime { get; init; }
    public required string Title { get; init; }
    public string BuildingName { get; init; } = "ทุกตึก/ส่วนกลาง";
    public string? Location { get; init; }
    public string? Notes { get; init; }
}

public sealed class AppointmentEditViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "กรุณาเลือกประเภทนัดหมาย")]
    [Display(Name = "ประเภทนัดหมาย")]
    public AppointmentType Type { get; set; } = AppointmentType.Maintenance;

    [Required(ErrorMessage = "กรุณากรอกหัวข้อนัดหมาย")]
    [StringLength(300)]
    [Display(Name = "หัวข้อนัดหมาย")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "กรุณาเลือกวันที่นัดหมาย")]
    [Display(Name = "วันที่นัดหมาย")]
    public DateOnly ScheduledDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Display(Name = "เวลา")]
    public TimeOnly? ScheduledTime { get; set; }

    [Display(Name = "ตึกนก")]
    public Guid? BuildingId { get; set; }

    [StringLength(300)]
    [Display(Name = "สถานที่")]
    public string? Location { get; set; }

    [StringLength(2000)]
    [Display(Name = "รายละเอียด/หมายเหตุ")]
    public string? Notes { get; set; }

    [Display(Name = "สถานะ")]
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

    public IReadOnlyList<SelectListItem> Buildings { get; set; } = [];
    public bool IsEdit => Id.HasValue;
}