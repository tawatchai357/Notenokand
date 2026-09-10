using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Notenokand.Domain.Entities;
using Notenokand.Domain.Enums;
using Notenokand.Infrastructure.Identity;
using Notenokand.Infrastructure.Persistence;
using Notenokand.Web.Models.Buildings;
using Notenokand.Web.Services;

namespace Notenokand.Web.Controllers;

[Authorize]
[Route("app/buildings")]
public sealed class BuildingsController(NotenokandDbContext db, UserManager<ApplicationUser> userManager, BuildingPhotoStorage photoStorage) : Controller
{
    [HttpGet("create")]
    public async Task<IActionResult> Create()
    {
        var accountId = await GetAccountIdAsync();
        if (accountId is null) return RedirectToAction("Index", "Onboarding");
        return View("Edit", new BuildingEditViewModel { Code = await NextCodeAsync(accountId.Value), Provinces = await LoadProvincesAsync() });
    }

    [HttpPost("create")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BuildingEditViewModel model)
    {
        var accountId = await GetAccountIdAsync(); var userId = GetUserId();
        if (accountId is null || userId is null) return Forbid();
        await ValidateModelAsync(model, accountId.Value);
        ValidatePhoto(model.Photo, nameof(model.Photo));
        if (!ModelState.IsValid) { model.Provinces = await LoadProvincesAsync(); return View("Edit", model); }

        var provinceName = await db.ThaiProvinces.Where(x => x.Code == model.ProvinceCode).Select(x => x.NameTh).SingleAsync();
        var building = new BirdBuilding
        {
            AccountId = accountId.Value, OwnerUserId = userId.Value, Code = model.Code.Trim().ToUpperInvariant(), Name = model.Name.Trim(),
            Address = NullIfWhiteSpace(model.Address), Province = provinceName, ProvinceCode = model.ProvinceCode, DistrictCode = model.DistrictCode,
            SubdistrictCode = model.SubdistrictCode, PostalCode = model.PostalCode, Latitude = model.Latitude, Longitude = model.Longitude,
            StartedOn = model.StartedOn, BuiltOrPurchasedYear = ToGregorianYear(model.BuiltOrPurchasedYearBuddhist), FloorCount = model.FloorCount,
            RoomCount = model.RoomCount, AreaSquareMeters = model.AreaSquareMeters, WidthMeters = model.WidthMeters, DepthMeters = model.DepthMeters,
            ConstructionBudget = model.ConstructionBudget, Notes = NullIfWhiteSpace(model.Notes), Status = model.Status, CreatedByUserId = userId
        };
        string? newStorageKey = null;
        try
        {
            if (model.Photo is { Length: > 0 }) { var photo = await photoStorage.SaveAsync(model.Photo, accountId.Value, building.Id); ApplyPhoto(building, photo); newStorageKey = photo.StorageKey; }
            db.BirdBuildings.Add(building); await db.SaveChangesAsync();
        }
        catch { photoStorage.Delete(newStorageKey); throw; }
        TempData["SuccessMessage"] = "เพิ่มตึกนกเรียบร้อยแล้ว";
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var accountId = await GetAccountIdAsync(); if (accountId is null) return RedirectToAction("Index", "Onboarding");
        var building = await db.BirdBuildings.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.AccountId == accountId && !x.IsDeleted);
        if (building is null) return NotFound();
        return View(new BuildingEditViewModel
        {
            Id = building.Id, Code = building.Code, Name = building.Name, Address = building.Address, ProvinceCode = building.ProvinceCode,
            DistrictCode = building.DistrictCode, SubdistrictCode = building.SubdistrictCode, PostalCode = building.PostalCode ?? string.Empty,
            Latitude = building.Latitude, Longitude = building.Longitude, StartedOn = building.StartedOn,
            BuiltOrPurchasedYearBuddhist = ToBuddhistYear(building.BuiltOrPurchasedYear), FloorCount = building.FloorCount, RoomCount = building.RoomCount,
            AreaSquareMeters = building.AreaSquareMeters, WidthMeters = building.WidthMeters, DepthMeters = building.DepthMeters,
            ConstructionBudget = building.ConstructionBudget, Notes = building.Notes, Status = building.Status,
            HasPhoto = !string.IsNullOrWhiteSpace(building.PhotoStorageKey), Provinces = await LoadProvincesAsync()
        });
    }

    [HttpPost("{id:guid}/edit")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, BuildingEditViewModel model)
    {
        var accountId = await GetAccountIdAsync(); var userId = GetUserId();
        if (accountId is null || userId is null) return Forbid();
        var building = await db.BirdBuildings.SingleOrDefaultAsync(x => x.Id == id && x.AccountId == accountId && !x.IsDeleted);
        if (building is null) return NotFound();
        model.Id = id; model.HasPhoto = !string.IsNullOrWhiteSpace(building.PhotoStorageKey);
        await ValidateModelAsync(model, accountId.Value, id); ValidatePhoto(model.Photo, nameof(model.Photo));
        if (!ModelState.IsValid) { model.Provinces = await LoadProvincesAsync(); return View(model); }

        building.Code = model.Code.Trim().ToUpperInvariant(); building.Name = model.Name.Trim(); building.Address = NullIfWhiteSpace(model.Address);
        building.Province = await db.ThaiProvinces.Where(x => x.Code == model.ProvinceCode).Select(x => x.NameTh).SingleAsync();
        building.ProvinceCode = model.ProvinceCode; building.DistrictCode = model.DistrictCode; building.SubdistrictCode = model.SubdistrictCode;
        building.PostalCode = model.PostalCode; building.Latitude = model.Latitude; building.Longitude = model.Longitude; building.StartedOn = model.StartedOn;
        building.BuiltOrPurchasedYear = ToGregorianYear(model.BuiltOrPurchasedYearBuddhist); building.FloorCount = model.FloorCount;
        building.RoomCount = model.RoomCount; building.AreaSquareMeters = model.AreaSquareMeters; building.WidthMeters = model.WidthMeters;
        building.DepthMeters = model.DepthMeters; building.ConstructionBudget = model.ConstructionBudget; building.Notes = NullIfWhiteSpace(model.Notes);
        building.Status = model.Status; building.UpdatedAt = DateTimeOffset.UtcNow; building.UpdatedByUserId = userId;

        var oldStorageKey = building.PhotoStorageKey; string? newStorageKey = null;
        try
        {
            if (model.Photo is { Length: > 0 }) { var photo = await photoStorage.SaveAsync(model.Photo, accountId.Value, building.Id); ApplyPhoto(building, photo); newStorageKey = photo.StorageKey; }
            await db.SaveChangesAsync();
        }
        catch { photoStorage.Delete(newStorageKey); throw; }
        if (newStorageKey is not null) photoStorage.Delete(oldStorageKey);
        TempData["SuccessMessage"] = "บันทึกข้อมูลตึกนกเรียบร้อยแล้ว";
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet("{id:guid}/photo")]
    public async Task<IActionResult> Photo(Guid id)
    {
        var accountId = await GetAccountIdAsync(); if (accountId is null) return Forbid();
        var photo = await db.BirdBuildings.AsNoTracking().Where(x => x.Id == id && x.AccountId == accountId && !x.IsDeleted && x.PhotoStorageKey != null)
            .Select(x => new { x.PhotoStorageKey, x.PhotoContentType }).SingleOrDefaultAsync();
        if (photo is null) return NotFound();
        var path = photoStorage.ResolvePath(photo.PhotoStorageKey!); if (!System.IO.File.Exists(path)) return NotFound();
        Response.Headers.CacheControl = "private, no-cache"; Response.Headers.XContentTypeOptions = "nosniff";
        return PhysicalFile(path, photo.PhotoContentType ?? "application/octet-stream", enableRangeProcessing: true);
    }

    [HttpPost("{id:guid}/toggle-status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var accountId = await GetAccountIdAsync(); var userId = GetUserId(); if (accountId is null || userId is null) return Forbid();
        var building = await db.BirdBuildings.SingleOrDefaultAsync(x => x.Id == id && x.AccountId == accountId && !x.IsDeleted); if (building is null) return NotFound();
        building.Status = building.Status == BuildingStatus.Active ? BuildingStatus.Inactive : BuildingStatus.Active;
        building.UpdatedAt = DateTimeOffset.UtcNow; building.UpdatedByUserId = userId; await db.SaveChangesAsync();
        TempData["SuccessMessage"] = building.Status == BuildingStatus.Active ? "เปิดใช้งานตึกนกแล้ว" : "ปิดใช้งานตึกนกแล้ว ข้อมูลย้อนหลังยังคงอยู่";
        return RedirectToAction("Index", "Dashboard");
    }

    private void ValidatePhoto(IFormFile? photo, string field) { var error = photoStorage.Validate(photo); if (error is not null) ModelState.AddModelError(field, error); }
    private static void ApplyPhoto(BirdBuilding building, StoredBuildingPhoto photo) { building.PhotoStorageKey = photo.StorageKey; building.PhotoOriginalFileName = photo.OriginalFileName; building.PhotoContentType = photo.ContentType; building.PhotoSizeBytes = photo.SizeBytes; building.PhotoSha256 = photo.Sha256; }
    private async Task ValidateModelAsync(BuildingEditViewModel model, Guid accountId, Guid? excludedId = null)
    {
        var code = model.Code?.Trim().ToUpperInvariant() ?? string.Empty;
        if (await db.BirdBuildings.AnyAsync(x => x.AccountId == accountId && x.Code == code && !x.IsDeleted && x.Id != excludedId)) ModelState.AddModelError(nameof(model.Code), "รหัสตึกนี้ถูกใช้งานแล้ว");
        var districtValid = model.DistrictCode.HasValue && model.ProvinceCode.HasValue && await db.ThaiDistricts.AnyAsync(x => x.Code == model.DistrictCode && x.ProvinceCode == model.ProvinceCode && x.IsActive);
        var subdistrictValid = model.SubdistrictCode.HasValue && model.DistrictCode.HasValue && await db.ThaiSubdistricts.AnyAsync(x => x.Code == model.SubdistrictCode && x.DistrictCode == model.DistrictCode && x.IsActive);
        var postalValid = model.SubdistrictCode.HasValue && !string.IsNullOrWhiteSpace(model.PostalCode) && await db.ThaiSubdistrictPostalCodes.AnyAsync(x => x.SubdistrictCode == model.SubdistrictCode && x.PostalCode == model.PostalCode);
        if (!districtValid) ModelState.AddModelError(nameof(model.DistrictCode), "อำเภอหรือเขตไม่ตรงกับจังหวัด"); if (!subdistrictValid) ModelState.AddModelError(nameof(model.SubdistrictCode), "ตำบลหรือแขวงไม่ตรงกับอำเภอ"); if (!postalValid) ModelState.AddModelError(nameof(model.PostalCode), "รหัสไปรษณีย์ไม่ตรงกับตำบล");
    }
    private Guid? GetUserId() => Guid.TryParse(userManager.GetUserId(User), out var id) ? id : null;
    private async Task<Guid?> GetAccountIdAsync() { var userId = GetUserId(); return userId is null ? null : await db.AccountUsers.Where(x => x.UserId == userId && x.IsActive && !x.IsDeleted).Select(x => (Guid?)x.AccountId).FirstOrDefaultAsync(); }
    private async Task<string> NextCodeAsync(Guid accountId) { var used = await db.BirdBuildings.Where(x => x.AccountId == accountId).Select(x => x.Code).ToListAsync(); var number = 1; while (used.Contains($"BLD-{number:000}", StringComparer.OrdinalIgnoreCase)) number++; return $"BLD-{number:000}"; }
    private async Task<IReadOnlyList<SelectListItem>> LoadProvincesAsync() => await db.ThaiProvinces.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.NameTh).Select(x => new SelectListItem(x.NameTh, x.Code.ToString())).ToListAsync();
    private static short? ToGregorianYear(int? buddhistYear) => buddhistYear.HasValue ? checked((short)(buddhistYear.Value - 543)) : null;
    private static int? ToBuddhistYear(short? gregorianYear) => gregorianYear.HasValue ? gregorianYear.Value + 543 : null;
    private static string? NullIfWhiteSpace(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}