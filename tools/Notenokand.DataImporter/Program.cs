using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Notenokand.Domain.Entities;
using Notenokand.Infrastructure.Persistence;

const string defaultConnection = "Server=.\\SQL2016;Database=notenokand;Trusted_Connection=True;TrustServerCertificate=True";
var connectionString = GetOption(args, "--connection") ?? defaultConnection;
var dataDirectory = GetOption(args, "--data") ?? Path.Combine(Directory.GetCurrentDirectory(), "data", "thai-addresses");

var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
var provinces = ReadJson<ProvinceRow>(Path.Combine(dataDirectory, "provinces.json"), jsonOptions);
var districts = ReadJson<DistrictRow>(Path.Combine(dataDirectory, "districts.json"), jsonOptions);
var subdistricts = ReadJson<SubdistrictRow>(Path.Combine(dataDirectory, "subdistricts.json"), jsonOptions);

Validate(provinces, districts, subdistricts);

var options = new DbContextOptionsBuilder<NotenokandDbContext>()
    .UseSqlServer(connectionString)
    .Options;

await using var db = new NotenokandDbContext(options);
await db.Database.MigrateAsync();
await using var transaction = await db.Database.BeginTransactionAsync();

var existingProvinces = await db.ThaiProvinces.ToDictionaryAsync(x => x.Code);
foreach (var row in provinces)
{
    var code = checked((short)row.ProvinceCode);
    if (existingProvinces.TryGetValue(code, out var entity))
    {
        entity.NameTh = row.ProvinceNameTh.Trim();
        entity.NameEn = row.ProvinceNameEn.Trim();
        entity.IsActive = true;
    }
    else
    {
        db.ThaiProvinces.Add(new ThaiProvince { Code = code, NameTh = row.ProvinceNameTh.Trim(), NameEn = row.ProvinceNameEn.Trim() });
    }
}
await db.SaveChangesAsync();

var existingDistricts = await db.ThaiDistricts.ToDictionaryAsync(x => x.Code);
foreach (var row in districts)
{
    if (existingDistricts.TryGetValue(row.DistrictCode, out var entity))
    {
        entity.ProvinceCode = checked((short)row.ProvinceCode);
        entity.NameTh = row.DistrictNameTh.Trim();
        entity.NameEn = row.DistrictNameEn.Trim();
        entity.IsActive = true;
    }
    else
    {
        db.ThaiDistricts.Add(new ThaiDistrict
        {
            Code = row.DistrictCode,
            ProvinceCode = checked((short)row.ProvinceCode),
            NameTh = row.DistrictNameTh.Trim(),
            NameEn = row.DistrictNameEn.Trim()
        });
    }
}
await db.SaveChangesAsync();

var existingSubdistricts = await db.ThaiSubdistricts.ToDictionaryAsync(x => x.Code);
foreach (var row in subdistricts)
{
    if (existingSubdistricts.TryGetValue(row.SubdistrictCode, out var entity))
    {
        entity.DistrictCode = row.DistrictCode;
        entity.NameTh = row.SubdistrictNameTh.Trim();
        entity.NameEn = row.SubdistrictNameEn.Trim();
        entity.IsActive = true;
    }
    else
    {
        db.ThaiSubdistricts.Add(new ThaiSubdistrict
        {
            Code = row.SubdistrictCode,
            DistrictCode = row.DistrictCode,
            NameTh = row.SubdistrictNameTh.Trim(),
            NameEn = row.SubdistrictNameEn.Trim()
        });
    }
}
await db.SaveChangesAsync();

var existingPostalCodes = await db.ThaiSubdistrictPostalCodes
    .Select(x => new { x.SubdistrictCode, x.PostalCode })
    .ToHashSetAsync();
foreach (var row in subdistricts)
{
    var postalCode = row.PostalCode.ToString("00000");
    if (existingPostalCodes.Add(new { row.SubdistrictCode, PostalCode = postalCode }))
    {
        db.ThaiSubdistrictPostalCodes.Add(new ThaiSubdistrictPostalCode
        {
            SubdistrictCode = row.SubdistrictCode,
            PostalCode = postalCode,
            IsPrimary = true
        });
    }
}
await db.SaveChangesAsync();
await transaction.CommitAsync();

Console.WriteLine($"Imported {provinces.Count:N0} provinces, {districts.Count:N0} districts, {subdistricts.Count:N0} subdistricts and {subdistricts.Count:N0} postal-code mappings.");

static string? GetOption(string[] arguments, string name)
{
    var index = Array.IndexOf(arguments, name);
    return index >= 0 && index + 1 < arguments.Length ? arguments[index + 1] : null;
}

static List<T> ReadJson<T>(string path, JsonSerializerOptions options)
{
    if (!File.Exists(path))
        throw new FileNotFoundException("Address dataset was not found.", path);

    return JsonSerializer.Deserialize<List<T>>(File.ReadAllText(path), options)
        ?? throw new InvalidDataException($"Cannot deserialize {path}.");
}

static void Validate(List<ProvinceRow> provinces, List<DistrictRow> districts, List<SubdistrictRow> subdistricts)
{
    if (provinces.Count != 77 || districts.Count != 928 || subdistricts.Count != 7436)
        throw new InvalidDataException($"Unexpected dataset size: {provinces.Count} provinces, {districts.Count} districts, {subdistricts.Count} subdistricts.");

    if (provinces.Select(x => x.ProvinceCode).Distinct().Count() != provinces.Count ||
        districts.Select(x => x.DistrictCode).Distinct().Count() != districts.Count ||
        subdistricts.Select(x => x.SubdistrictCode).Distinct().Count() != subdistricts.Count)
        throw new InvalidDataException("The dataset contains duplicate administrative codes.");

    var provinceCodes = provinces.Select(x => x.ProvinceCode).ToHashSet();
    var districtCodes = districts.Select(x => x.DistrictCode).ToHashSet();
    if (districts.Any(x => !provinceCodes.Contains(x.ProvinceCode)) ||
        subdistricts.Any(x => !districtCodes.Contains(x.DistrictCode)) ||
        subdistricts.Any(x => x.PostalCode is < 10000 or > 99999))
        throw new InvalidDataException("The dataset contains an invalid parent code or postal code.");
}

internal sealed record ProvinceRow(int ProvinceCode, string ProvinceNameEn, string ProvinceNameTh);
internal sealed record DistrictRow(int ProvinceCode, int DistrictCode, string DistrictNameEn, string DistrictNameTh, int PostalCode);
internal sealed record SubdistrictRow(int ProvinceCode, int DistrictCode, int SubdistrictCode, string SubdistrictNameEn, string SubdistrictNameTh, int PostalCode);