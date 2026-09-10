using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Notenokand.Domain.Entities;
using Notenokand.Infrastructure.Identity;
using Notenokand.Infrastructure.Persistence;
using Notenokand.Web.Models.Account;

namespace Notenokand.Web.Controllers;

[Authorize]
[Route("onboarding")]
public sealed class OnboardingController(
    NotenokandDbContext db,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    Notenokand.Web.Services.BuildingPhotoStorage photoStorage) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var userId = Guid.Parse(userManager.GetUserId(User)!);
        if (await db.AccountUsers.AnyAsync(x => x.UserId == userId && x.IsActive))
            return RedirectToAction("Index", "Dashboard");

        return View(new OnboardingViewModel { Provinces = await LoadProvincesAsync() });
    }

    [HttpPost("")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(OnboardingViewModel model)
    {
        var userId = Guid.Parse(userManager.GetUserId(User)!);
        if (await db.AccountUsers.AnyAsync(x => x.UserId == userId && x.IsActive))
            return RedirectToAction("Index", "Dashboard");

        var photoError = photoStorage.Validate(model.BuildingPhoto);
        if (photoError is not null) ModelState.AddModelError(nameof(model.BuildingPhoto), photoError);
        if (model.BuildingPhoto is { Length: > 0 } && string.IsNullOrWhiteSpace(model.BuildingName))
            ModelState.AddModelError(nameof(model.BuildingName), "กรุณากรอกชื่อตึกก่อนเพิ่มรูป");

        var province = model.ProvinceCode is null ? null : await db.ThaiProvinces.FindAsync(model.ProvinceCode.Value);
        var district = model.DistrictCode is null ? null : await db.ThaiDistricts.FindAsync(model.DistrictCode.Value);
        var subdistrict = model.SubdistrictCode is null ? null : await db.ThaiSubdistricts.FindAsync(model.SubdistrictCode.Value);
        var postalCodeMatches = model.SubdistrictCode is not null && !string.IsNullOrWhiteSpace(model.PostalCode) &&
            await db.ThaiSubdistrictPostalCodes.AnyAsync(x => x.SubdistrictCode == model.SubdistrictCode && x.PostalCode == model.PostalCode);

        if (district is not null && district.ProvinceCode != model.ProvinceCode)
            ModelState.AddModelError(nameof(model.DistrictCode), "อำเภอหรือเขตไม่อยู่ในจังหวัดที่เลือก");
        if (subdistrict is not null && subdistrict.DistrictCode != model.DistrictCode)
            ModelState.AddModelError(nameof(model.SubdistrictCode), "ตำบลหรือแขวงไม่อยู่ในอำเภอที่เลือก");
        if (!postalCodeMatches)
            ModelState.AddModelError(nameof(model.PostalCode), "รหัสไปรษณีย์ไม่ตรงกับตำบลที่เลือก");

        if (!ModelState.IsValid || province is null || district is null || subdistrict is null)
        {
            model.Provinces = await LoadProvincesAsync();
            return View(model);
        }

        var account = new Account
        {
            Name = model.AccountName.Trim(),
            BusinessType = model.BusinessType?.Trim(),
            TaxId = model.TaxId?.Trim(),
            AddressLine = model.AddressLine?.Trim(),
            ProvinceCode = model.ProvinceCode,
            DistrictCode = model.DistrictCode,
            SubdistrictCode = model.SubdistrictCode,
            PostalCode = model.PostalCode,
            CreatedByUserId = userId
        };

                await using var transaction = await db.Database.BeginTransactionAsync();
        string? savedPhotoKey = null;
        try
        {
            db.Accounts.Add(account);
            db.AccountUsers.Add(new AccountUser
            {
                Account = account,
                UserId = userId,
                RoleName = "Owner",
                CreatedByUserId = userId
            });

            BirdBuilding? building = null;
            if (!string.IsNullOrWhiteSpace(model.BuildingName))
            {
                building = new BirdBuilding
                {
                    AccountId = account.Id,
                    OwnerUserId = userId,
                    Code = "BLD-001",
                    Name = model.BuildingName.Trim(),
                    Province = province.NameTh,
                    Address = model.AddressLine?.Trim(),
                    ProvinceCode = model.ProvinceCode,
                    DistrictCode = model.DistrictCode,
                    SubdistrictCode = model.SubdistrictCode,
                    PostalCode = model.PostalCode,
                    CreatedByUserId = userId
                };
                if (model.BuildingPhoto is { Length: > 0 })
                {
                    var photo = await photoStorage.SaveAsync(model.BuildingPhoto, account.Id, building.Id);
                    savedPhotoKey = photo.StorageKey;
                    building.PhotoStorageKey = photo.StorageKey;
                    building.PhotoOriginalFileName = photo.OriginalFileName;
                    building.PhotoContentType = photo.ContentType;
                    building.PhotoSizeBytes = photo.SizeBytes;
                    building.PhotoSha256 = photo.Sha256;
                }
                db.BirdBuildings.Add(building);
            }
            await db.SaveChangesAsync();

            const string ownerRole = "Owner";
            if (!await roleManager.RoleExistsAsync(ownerRole)) await roleManager.CreateAsync(new IdentityRole<Guid>(ownerRole));
            var user = await userManager.GetUserAsync(User);
            if (user is not null && !await userManager.IsInRoleAsync(user, ownerRole)) await userManager.AddToRoleAsync(user, ownerRole);

            await transaction.CommitAsync();
        }
        catch
        {
            photoStorage.Delete(savedPhotoKey);
            await transaction.RollbackAsync();
            throw;
        }
TempData["SuccessMessage"] = "ตั้งค่ากิจการเรียบร้อยแล้ว";
        return RedirectToAction("Index", "Dashboard");
    }

    private async Task<IReadOnlyList<SelectListItem>> LoadProvincesAsync() => await db.ThaiProvinces
        .AsNoTracking()
        .Where(x => x.IsActive)
        .OrderBy(x => x.NameTh)
        .Select(x => new SelectListItem(x.NameTh, x.Code.ToString()))
        .ToListAsync();
}