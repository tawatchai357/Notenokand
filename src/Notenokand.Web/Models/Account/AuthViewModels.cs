using System.ComponentModel.DataAnnotations;

namespace Notenokand.Web.Models.Account;

public sealed class RegisterViewModel
{
    [Required(ErrorMessage = "กรุณากรอกชื่อและนามสกุล")]
    [StringLength(150)]
    [Display(Name = "ชื่อและนามสกุล")]
    public string DisplayName { get; set; } = string.Empty;

    [Required(ErrorMessage = "กรุณากรอกอีเมล")]
    [EmailAddress(ErrorMessage = "รูปแบบอีเมลไม่ถูกต้อง")]
    [Display(Name = "อีเมล")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "รูปแบบเบอร์โทรศัพท์ไม่ถูกต้อง")]
    [Display(Name = "เบอร์โทรศัพท์")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "กรุณากรอกรหัสผ่าน")]
    [StringLength(100, MinimumLength = 10, ErrorMessage = "รหัสผ่านต้องมีอย่างน้อย 10 ตัวอักษร")]
    [DataType(DataType.Password)]
    [Display(Name = "รหัสผ่าน")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "กรุณายืนยันรหัสผ่าน")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "รหัสผ่านทั้งสองช่องไม่ตรงกัน")]
    [Display(Name = "ยืนยันรหัสผ่าน")]
    public string ConfirmPassword { get; set; } = string.Empty;
    public bool AcceptTerms { get; set; }
}

public sealed class LoginViewModel
{
    [Required(ErrorMessage = "กรุณากรอกอีเมล")]
    [EmailAddress(ErrorMessage = "รูปแบบอีเมลไม่ถูกต้อง")]
    [Display(Name = "อีเมล")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "กรุณากรอกรหัสผ่าน")]
    [DataType(DataType.Password)]
    [Display(Name = "รหัสผ่าน")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "จดจำฉัน")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}