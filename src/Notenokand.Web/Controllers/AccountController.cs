using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Notenokand.Domain.Entities;
using Notenokand.Infrastructure.Identity;
using Notenokand.Infrastructure.Persistence;
using Notenokand.Web.Models.Account;

namespace Notenokand.Web.Controllers;

[EnableRateLimiting("auth")]
[Route("account")]
public sealed class AccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    NotenokandDbContext db,
    IWebHostEnvironment environment) : Controller
{
    [HttpGet("register")]
    public IActionResult Register() => User.Identity?.IsAuthenticated == true
        ? RedirectToAction("Index", "Dashboard")
        : View(new RegisterViewModel());

    [HttpPost("register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!model.AcceptTerms)
            ModelState.AddModelError(nameof(model.AcceptTerms), "กรุณายอมรับเงื่อนไขและนโยบายความเป็นส่วนตัว");

        if (!ModelState.IsValid)
            return View(model);

        var normalizedEmail = model.Email.Trim().ToLowerInvariant();
        await using var transaction = await db.Database.BeginTransactionAsync();
        var user = new ApplicationUser
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            DisplayName = model.DisplayName.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim()
        };

        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, TranslateIdentityError(error.Code));
            return View(model);
        }

        var now = DateTimeOffset.UtcNow;
        db.UserConsents.Add(new UserConsent
        {
            UserId = user.Id,
            ConsentType = "TermsAndPrivacy",
            Version = "2026-09-07",
            AcceptedAt = now,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        });
        db.EmailVerificationLogs.Add(new EmailVerificationLog
        {
            UserId = user.Id,
            RequestedAt = now,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        });
        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var confirmationUrl = Url.Action(nameof(ConfirmEmail), "Account", new { userId = user.Id, code }, Request.Scheme)!;
        if (environment.IsDevelopment())
            TempData["DevelopmentConfirmationUrl"] = confirmationUrl;

        return RedirectToAction(nameof(CheckEmail), new { email = normalizedEmail });
    }

    [HttpGet("check-email")]
    public IActionResult CheckEmail(string email)
    {
        ViewBag.Email = email;
        ViewBag.DevelopmentConfirmationUrl = TempData["DevelopmentConfirmationUrl"] as string;
        return View();
    }

    [HttpPost("resend-confirmation")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendConfirmation(string email)
    {
        var safeEmail = email?.Trim() ?? string.Empty;
        var user = string.IsNullOrWhiteSpace(safeEmail) ? null : await userManager.FindByEmailAsync(safeEmail);
        if (user is not null && !await userManager.IsEmailConfirmedAsync(user))
        {
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var confirmationUrl = Url.Action(nameof(ConfirmEmail), "Account", new { userId = user.Id, code }, Request.Scheme)!;
            db.EmailVerificationLogs.Add(new EmailVerificationLog
            {
                UserId = user.Id,
                RequestedAt = DateTimeOffset.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            });
            await db.SaveChangesAsync();
            if (environment.IsDevelopment())
                TempData["DevelopmentConfirmationUrl"] = confirmationUrl;
        }

        return RedirectToAction(nameof(CheckEmail), new { email = safeEmail });
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(Guid userId, string code)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null || string.IsNullOrWhiteSpace(code))
            return View("ConfirmationResult", false);

        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        }
        catch (FormatException)
        {
            return View("ConfirmationResult", false);
        }

        var result = await userManager.ConfirmEmailAsync(user, decodedToken);
        if (result.Succeeded)
        {
            var verification = await db.EmailVerificationLogs
                .Where(x => x.UserId == user.Id && x.ConfirmedAt == null)
                .OrderByDescending(x => x.RequestedAt)
                .FirstOrDefaultAsync();
            if (verification is not null)
            {
                verification.ConfirmedAt = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync();
            }
            await signInManager.SignInAsync(user, isPersistent: false);
        }

        return View("ConfirmationResult", result.Succeeded);
    }

    [HttpGet("login")]
    public IActionResult Login(string? returnUrl = null) => User.Identity?.IsAuthenticated == true
        ? RedirectToAction("Index", "Dashboard")
        : View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await signInManager.PasswordSignInAsync(
            model.Email.Trim().ToLowerInvariant(), model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
            return LocalRedirect(Url.IsLocalUrl(model.ReturnUrl) ? model.ReturnUrl : "/app");

        ModelState.AddModelError(string.Empty, result.IsNotAllowed
            ? "กรุณายืนยันอีเมลก่อนเข้าสู่ระบบ"
            : result.IsLockedOut
                ? "บัญชีถูกล็อกชั่วคราว กรุณาลองใหม่ภายหลัง"
                : "อีเมลหรือรหัสผ่านไม่ถูกต้อง");
        return View(model);
    }

    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet("access-denied")]
    public IActionResult AccessDenied() => View();

    private static string TranslateIdentityError(string code) => code switch
    {
        "DuplicateEmail" or "DuplicateUserName" => "อีเมลนี้ถูกใช้งานแล้ว",
        "PasswordTooShort" => "รหัสผ่านสั้นเกินไป",
        "PasswordRequiresDigit" => "รหัสผ่านต้องมีตัวเลขอย่างน้อยหนึ่งตัว",
        "PasswordRequiresNonAlphanumeric" => "รหัสผ่านต้องมีอักขระพิเศษอย่างน้อยหนึ่งตัว",
        "PasswordRequiresUpper" => "รหัสผ่านต้องมีตัวอักษรภาษาอังกฤษตัวใหญ่อย่างน้อยหนึ่งตัว",
        "PasswordRequiresLower" => "รหัสผ่านต้องมีตัวอักษรภาษาอังกฤษตัวเล็กอย่างน้อยหนึ่งตัว",
        _ => "ไม่สามารถสร้างบัญชีได้ กรุณาตรวจข้อมูลแล้วลองใหม่"
    };
}