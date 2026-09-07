using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Notenokand.Web.Models.Account;

public sealed class OnboardingViewModel
{
    [Required(ErrorMessage = "กรุณากรอกชื่อกิจการ")]
    [StringLength(200)]
    [Display(Name = "ชื่อกิจการหรือชื่อบัญชี")]
    public string AccountName { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "ประเภทธุรกิจ")]
    public string? BusinessType { get; set; }

    [StringLength(20)]
    [Display(Name = "เลขประจำตัวผู้เสียภาษี")]
    public string? TaxId { get; set; }

    [StringLength(500)]
    [Display(Name = "เลขที่ หมู่ ซอย ถนน")]
    public string? AddressLine { get; set; }

    [Required(ErrorMessage = "กรุณาเลือกจังหวัด")]
    [Display(Name = "จังหวัด")]
    public short? ProvinceCode { get; set; }

    [Required(ErrorMessage = "กรุณาเลือกอำเภอหรือเขต")]
    [Display(Name = "อำเภอหรือเขต")]
    public int? DistrictCode { get; set; }

    [Required(ErrorMessage = "กรุณาเลือกตำบลหรือแขวง")]
    [Display(Name = "ตำบลหรือแขวง")]
    public int? SubdistrictCode { get; set; }

    [Required(ErrorMessage = "ไม่พบรหัสไปรษณีย์")]
    [RegularExpression(@"^\d{5}$", ErrorMessage = "รหัสไปรษณีย์ต้องมี 5 หลัก")]
    [Display(Name = "รหัสไปรษณีย์")]
    public string PostalCode { get; set; } = string.Empty;

    [StringLength(200)]
    [Display(Name = "ชื่อตึกนกแห่งแรก (ไม่บังคับ)")]
    public string? BuildingName { get; set; }

    public IReadOnlyList<SelectListItem> Provinces { get; set; } = [];
}

public sealed class DashboardViewModel
{
    public required string AccountName { get; init; }
    public int BuildingCount { get; init; }
    public string DisplayName { get; init; } = string.Empty;
}