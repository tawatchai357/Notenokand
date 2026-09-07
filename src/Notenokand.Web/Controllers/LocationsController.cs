using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notenokand.Infrastructure.Persistence;

namespace Notenokand.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/locations")]
public sealed class LocationsController(NotenokandDbContext db) : ControllerBase
{
    [HttpGet("provinces")]
    public async Task<IActionResult> Provinces() => Ok(await db.ThaiProvinces.AsNoTracking()
        .Where(x => x.IsActive).OrderBy(x => x.NameTh)
        .Select(x => new { code = x.Code, name = x.NameTh }).ToListAsync());

    [HttpGet("districts")]
    public async Task<IActionResult> Districts(short provinceCode) => Ok(await db.ThaiDistricts.AsNoTracking()
        .Where(x => x.IsActive && x.ProvinceCode == provinceCode).OrderBy(x => x.NameTh)
        .Select(x => new { code = x.Code, name = x.NameTh }).ToListAsync());

    [HttpGet("subdistricts")]
    public async Task<IActionResult> Subdistricts(int districtCode) => Ok(await db.ThaiSubdistricts.AsNoTracking()
        .Where(x => x.IsActive && x.DistrictCode == districtCode).OrderBy(x => x.NameTh)
        .Select(x => new { code = x.Code, name = x.NameTh }).ToListAsync());

    [HttpGet("postal-codes")]
    public async Task<IActionResult> PostalCodes(int subdistrictCode) => Ok(await db.ThaiSubdistrictPostalCodes.AsNoTracking()
        .Where(x => x.SubdistrictCode == subdistrictCode).OrderByDescending(x => x.IsPrimary)
        .ThenBy(x => x.PostalCode).Select(x => x.PostalCode).ToListAsync());
}