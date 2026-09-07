using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Notenokand.Domain.Enums;

namespace Notenokand.Web.Models.Buildings;

public sealed class BuildingEditViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "กรุณากรอกรหัสตึก")]
    [StringLength(30)]
    [Display(Name = "รหัสตึก")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "กรุณากรอกชื่อตึก")]
    [StringLength(200)]
    [Display(Name = "ชื่อตึกนก")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "เลขที่ หมู่ ซอย ถนน")]
    public string? Address { get; set; }

    [Required(ErrorMessage = "กรุณาเลือกจังหวัด")]
    [Display(Name = "จังหวัด")]
    public short? ProvinceCode { get; set; }

    [Required(ErrorMessage = "กรุณาเลือกอำเภอหรือเขต")]
    [Display(Name = "อำเภอหรือเขต")]
    public int? DistrictCode { get; set; }

    [Required(ErrorMessage = "กรุณาเลือกตำบลหรือแขวง")]
    [Display(Name = "ตำบลหรือแขวง")]
    public int? SubdistrictCode { get; set; }

    [Required(ErrorMessage = "กรุณาเลือกรหัสไปรษณีย์")]
    [RegularExpression(@"^\d{5}$", ErrorMessage = "รหัสไปรษณีย์ต้องมี 5 หลัก")]
    [Display(Name = "รหัสไปรษณีย์")]
    public string PostalCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "กรุณาปักหมุดตำแหน่งตึกบนแผนที่")]
    [Range(-90, 90, ErrorMessage = "ละติจูดไม่ถูกต้อง")]
    public decimal? Latitude { get; set; }

    [Required(ErrorMessage = "กรุณาปักหมุดตำแหน่งตึกบนแผนที่")]
    [Range(-180, 180, ErrorMessage = "ลองจิจูดไม่ถูกต้อง")]
    public decimal? Longitude { get; set; }

    [Display(Name = "วันที่เริ่มใช้งาน")]
    [DataType(DataType.Date)]
    public DateOnly? StartedOn { get; set; }

    [Range(1, 999, ErrorMessage = "จำนวนชั้นต้องมากกว่า 0")]
    [Display(Name = "จำนวนชั้น")]
    public int? FloorCount { get; set; }

    [Range(1, 99999, ErrorMessage = "จำนวนห้องต้องมากกว่า 0")]
    [Display(Name = "จำนวนห้อง")]
    public int? RoomCount { get; set; }

    [Range(typeof(decimal), "0.01", "9999999999", ErrorMessage = "พื้นที่ต้องมากกว่า 0")]
    [Display(Name = "พื้นที่โดยประมาณ (ตร.ม.)")]
    public decimal? AreaSquareMeters { get; set; }

    [StringLength(2000)]
    [Display(Name = "หมายเหตุ")]
    public string? Notes { get; set; }

    [Display(Name = "สถานะ")]
    public BuildingStatus Status { get; set; } = BuildingStatus.Active;

    public IReadOnlyList<SelectListItem> Provinces { get; set; } = [];
    public bool IsEdit => Id.HasValue;
}

public sealed class BuildingCardViewModel
{
    public Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public string Location { get; init; } = "ยังไม่ได้ระบุที่อยู่";
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public BuildingStatus Status { get; init; }
    public int? FloorCount { get; init; }
    public int? RoomCount { get; init; }
}