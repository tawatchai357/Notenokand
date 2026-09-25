using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;
using Notenokand.Infrastructure.Identity;
using Notenokand.Web.Models.Account;
using Notenokand.Web.Services;
namespace Notenokand.Web.Controllers;

[Authorize]
[Route("account")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class ProfileController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn, AccountEmailService email) : Controller
{
    [HttpGet("profile")]
    public async Task<IActionResult> Index()
    {
        var user = await users.GetUserAsync(User); if (user is null) return Challenge();
        return View(new ProfileModel { DisplayName = user.DisplayName, PhoneNumber = user.PhoneNumber, Email = user.Email });
    }
    [HttpPost("profile"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProfileModel model)
    {
        var user = await users.GetUserAsync(User); if (user is null) return Challenge();
        model.Email = user.Email;
        if (!ModelState.IsValid) return View(model);
        user.DisplayName = model.DisplayName.Trim(); user.PhoneNumber = model.PhoneNumber?.Trim();
        var result = await users.UpdateAsync(user);
        if (!result.Succeeded) { ModelState.AddModelError("", "บันทึกไม่สำเร็จ กรุณาตรวจข้อมูล"); return View(model); }
        await signIn.RefreshSignInAsync(user);
        TempData["SuccessMessage"] = "บันทึกโปรไฟล์แล้ว"; return RedirectToAction(nameof(Index));
    }
    [HttpGet("change-password")]
    public IActionResult ChangePassword() => View(new ChangePasswordModel());
    [HttpPost("change-password"), ValidateAntiForgeryToken, EnableRateLimiting("auth")]
    public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await users.GetUserAsync(User); if (user is null) return Challenge();
        var result = await users.ChangePasswordAsync(user, model.CurrentPassword, model.Password);
        if (!result.Succeeded) { ModelState.AddModelError("", "เปลี่ยนรหัสผ่านไม่ได้ ตรวจรหัสเดิมและเงื่อนไขรหัสใหม่"); return View(model); }
        await signIn.RefreshSignInAsync(user);
        TempData["SuccessMessage"] = "เปลี่ยนรหัสผ่านแล้ว"; return RedirectToAction(nameof(Index));
    }
    [AllowAnonymous, HttpGet("forgot-password")]
    public IActionResult ForgotPassword() { ViewBag.EmailReady = email.IsConfigured; return View(new ForgotPasswordModel()); }
    [AllowAnonymous, HttpPost("forgot-password"), ValidateAntiForgeryToken, EnableRateLimiting("auth")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
    {
        ViewBag.EmailReady = email.IsConfigured;
        if (!email.IsConfigured) { ModelState.AddModelError("", "ยังไม่ได้ตั้งค่าบริการอีเมล กรุณาติดต่อผู้ดูแล"); return View(model); }
        if (!ModelState.IsValid) return View(model);
        var user = await users.FindByEmailAsync(model.Email.Trim());
        if (user is not null && user.IsActive && await users.IsEmailConfirmedAsync(user))
        {
            var token = await users.GeneratePasswordResetTokenAsync(user);
            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var link = email.Link("account/reset-password") + "?email=" + Uri.EscapeDataString(user.Email!) + "&code=" + Uri.EscapeDataString(code);
            await email.SendLinkAsync(user.Email!, "ตั้งรหัสผ่านใหม่สำหรับ Notenokand", link);
        }
        return View("ResetRequested");
    }
    [AllowAnonymous, HttpGet("reset-password")]
    public IActionResult ResetPassword(string? email, string? code) => View(new ResetPasswordModel { Email = email ?? "", Code = code ?? "" });
    [AllowAnonymous, HttpPost("reset-password"), ValidateAntiForgeryToken, EnableRateLimiting("auth")]
    public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await users.FindByEmailAsync(model.Email.Trim());
        string token;
        try { token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code)); }
        catch (FormatException) { token = ""; }
        var result = user is not null && user.IsActive && user.EmailConfirmed && token.Length > 0
            ? await users.ResetPasswordAsync(user, token, model.Password) : IdentityResult.Failed();
        if (!result.Succeeded) { ModelState.AddModelError("", "ลิงก์ไม่ถูกต้อง/หมดอายุ หรือรหัสผ่านไม่ผ่านเงื่อนไข กรุณาขอลิงก์ใหม่"); return View(model); }
        await signIn.SignOutAsync();
        TempData["SuccessMessage"] = "ตั้งรหัสผ่านใหม่แล้ว กรุณาเข้าสู่ระบบ";
        return RedirectToAction("Login", "Account");
    }
}
