using System.ComponentModel.DataAnnotations;
namespace Notenokand.Web.Models.Account;

public sealed class ForgotPasswordModel
{
    [Required, EmailAddress] public string Email { get; set; } = "";
}
public sealed class ResetPasswordModel
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required] public string Code { get; set; } = "";
    [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 10)]
    public string Password { get; set; } = "";
    [Required, DataType(DataType.Password), Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = "";
}
public sealed class ProfileModel
{
    [Required, StringLength(200)] public string DisplayName { get; set; } = "";
    [Phone, StringLength(30)] public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}
public sealed class ChangePasswordModel
{
    [Required, DataType(DataType.Password)] public string CurrentPassword { get; set; } = "";
    [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 10)] public string Password { get; set; } = "";
    [Required, DataType(DataType.Password), Compare(nameof(Password))] public string ConfirmPassword { get; set; } = "";
}
