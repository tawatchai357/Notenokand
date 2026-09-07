namespace Notenokand.Domain.Entities;

public sealed class ThaiProvince
{
    public short Code { get; set; }
    public required string NameTh { get; set; }
    public required string NameEn { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<ThaiDistrict> Districts { get; set; } = [];
}

public sealed class ThaiDistrict
{
    public int Code { get; set; }
    public short ProvinceCode { get; set; }
    public required string NameTh { get; set; }
    public required string NameEn { get; set; }
    public bool IsActive { get; set; } = true;
    public ThaiProvince Province { get; set; } = null!;
    public ICollection<ThaiSubdistrict> Subdistricts { get; set; } = [];
}

public sealed class ThaiSubdistrict
{
    public int Code { get; set; }
    public int DistrictCode { get; set; }
    public required string NameTh { get; set; }
    public required string NameEn { get; set; }
    public bool IsActive { get; set; } = true;
    public ThaiDistrict District { get; set; } = null!;
    public ICollection<ThaiSubdistrictPostalCode> PostalCodes { get; set; } = [];
}

public sealed class ThaiSubdistrictPostalCode
{
    public int SubdistrictCode { get; set; }
    public required string PostalCode { get; set; }
    public bool IsPrimary { get; set; } = true;
    public ThaiSubdistrict Subdistrict { get; set; } = null!;
}