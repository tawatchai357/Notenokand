using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notenokand.Domain.Entities;
using Notenokand.Infrastructure.Identity;

namespace Notenokand.Infrastructure.Persistence;

public sealed class BirdBuildingConfiguration : IEntityTypeConfiguration<BirdBuilding>
{
    public void Configure(EntityTypeBuilder<BirdBuilding> b)
    {
        b.HasIndex(x => new { x.AccountId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(30);
        b.Property(x => x.Name).HasMaxLength(200);
        b.Property(x => x.Latitude).HasPrecision(10, 7);
        b.Property(x => x.Longitude).HasPrecision(10, 7);
        b.Property(x => x.AreaSquareMeters).HasPrecision(12, 2);
        b.Property(x => x.WidthMeters).HasPrecision(10, 2);
        b.Property(x => x.DepthMeters).HasPrecision(10, 2);
        b.Property(x => x.ConstructionBudget).HasPrecision(18, 2);
        b.Property(x => x.BuiltOrPurchasedYear).HasColumnType("smallint");
        b.Property(x => x.PostalCode).HasColumnType("char(5)");
        b.Property(x => x.PhotoStorageKey).HasMaxLength(500);
        b.Property(x => x.PhotoOriginalFileName).HasMaxLength(255);
        b.Property(x => x.PhotoContentType).HasMaxLength(100);
        b.Property(x => x.PhotoSha256).HasMaxLength(64).IsFixedLength();
        b.HasOne<ThaiProvince>().WithMany().HasForeignKey(x => x.ProvinceCode).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ThaiDistrict>().WithMany().HasForeignKey(x => x.DistrictCode).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ThaiSubdistrict>().WithMany().HasForeignKey(x => x.SubdistrictCode).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Account>().WithMany().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class BuildingUserConfiguration : IEntityTypeConfiguration<BuildingUser>
{
    public void Configure(EntityTypeBuilder<BuildingUser> b)
    {
        b.HasIndex(x => new { x.BuildingId, x.UserId }).IsUnique();
        b.HasOne(x => x.Building).WithMany(x => x.Users).HasForeignKey(x => x.BuildingId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class HarvestRoundConfiguration : IEntityTypeConfiguration<HarvestRound>
{
    public void Configure(EntityTypeBuilder<HarvestRound> b)
    {
        b.HasIndex(x => new { x.BuildingId, x.RoundNumber }).IsUnique();
        b.Property(x => x.RoundNumber).HasMaxLength(50);
        b.Property(x => x.TotalWeightKg).HasPrecision(14, 3);
        b.Property(x => x.LaborCost).HasPrecision(18, 2);
        b.HasOne(x => x.Building).WithMany().HasForeignKey(x => x.BuildingId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class HarvestSampleConfiguration : IEntityTypeConfiguration<HarvestSample>
{
    public void Configure(EntityTypeBuilder<HarvestSample> b) => b.Property(x => x.WeightGrams).HasPrecision(10, 3);
}

public sealed class HarvestItemConfiguration : IEntityTypeConfiguration<HarvestItem>
{
    public void Configure(EntityTypeBuilder<HarvestItem> b)
    {
        b.Property(x => x.WeightKg).HasPrecision(14, 3);
        b.Property(x => x.RemainingWeightKg).HasPrecision(14, 3);
        b.ToTable(t => t.HasCheckConstraint("CK_HarvestItems_Weight", "[WeightKg] >= 0 AND [RemainingWeightKg] >= 0 AND [RemainingWeightKg] <= [WeightKg]"));
    }
}

public sealed class MasterOptionConfiguration : IEntityTypeConfiguration<MasterOption>
{
    public void Configure(EntityTypeBuilder<MasterOption> b)
    {
        b.HasIndex(x => new { x.Category, x.Code }).IsUnique();
        b.Property(x => x.Category).HasMaxLength(50);
        b.Property(x => x.Code).HasMaxLength(50);
        b.Property(x => x.Name).HasMaxLength(200);
    }
}

public sealed class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> b)
    {
        b.HasIndex(x => new { x.OwnerUserId, x.DocumentNumber }).IsUnique();
        b.Property(x => x.DocumentNumber).HasMaxLength(50);
        foreach (var name in new[] { nameof(Sale.Subtotal), nameof(Sale.DiscountAmount), nameof(Sale.ReturnAmount), nameof(Sale.NetAmount), nameof(Sale.PaidAmount) })
            b.Property<decimal>(name).HasPrecision(18, 2);
    }
}

public sealed class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> b)
    {
        b.Property(x => x.WeightKg).HasPrecision(14, 3);
        b.Property(x => x.PricePerKg).HasPrecision(18, 2);
        b.Property(x => x.DiscountAmount).HasPrecision(18, 2);
        b.Property(x => x.NetAmount).HasPrecision(18, 2);
        b.ToTable(t => t.HasCheckConstraint("CK_SaleItems_Weight", "[WeightKg] > 0"));
    }
}

public sealed class FinancialTransactionConfiguration : IEntityTypeConfiguration<FinancialTransaction>
{
    public void Configure(EntityTypeBuilder<FinancialTransaction> b) => b.Property(x => x.Amount).HasPrecision(18, 2);
}

public sealed class QualityStandardConfiguration : IEntityTypeConfiguration<QualityStandard>
{
    public void Configure(EntityTypeBuilder<QualityStandard> b) => b.HasIndex(x => new { x.OwnerUserId, x.Name, x.Version }).IsUnique();
}

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).UseIdentityColumn();
        b.Property(x => x.Action).HasMaxLength(50);
        b.Property(x => x.EntityName).HasMaxLength(200);
        b.Property(x => x.EntityId).HasMaxLength(100);
        b.Property(x => x.OccurredAt).HasPrecision(0);
        b.HasIndex(x => x.OccurredAt);
    }
}

public sealed class ThaiProvinceConfiguration : IEntityTypeConfiguration<ThaiProvince>
{
    public void Configure(EntityTypeBuilder<ThaiProvince> b)
    {
        b.HasKey(x => x.Code);
        b.Property(x => x.Code).ValueGeneratedNever();
        b.Property(x => x.NameTh).HasMaxLength(100);
        b.Property(x => x.NameEn).HasMaxLength(100);
        b.HasIndex(x => x.NameTh);
    }
}

public sealed class ThaiDistrictConfiguration : IEntityTypeConfiguration<ThaiDistrict>
{
    public void Configure(EntityTypeBuilder<ThaiDistrict> b)
    {
        b.HasKey(x => x.Code);
        b.Property(x => x.Code).ValueGeneratedNever();
        b.Property(x => x.NameTh).HasMaxLength(100);
        b.Property(x => x.NameEn).HasMaxLength(100);
        b.HasOne(x => x.Province).WithMany(x => x.Districts).HasForeignKey(x => x.ProvinceCode).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.ProvinceCode, x.NameTh });
    }
}

public sealed class ThaiSubdistrictConfiguration : IEntityTypeConfiguration<ThaiSubdistrict>
{
    public void Configure(EntityTypeBuilder<ThaiSubdistrict> b)
    {
        b.HasKey(x => x.Code);
        b.Property(x => x.Code).ValueGeneratedNever();
        b.Property(x => x.NameTh).HasMaxLength(100);
        b.Property(x => x.NameEn).HasMaxLength(100);
        b.HasOne(x => x.District).WithMany(x => x.Subdistricts).HasForeignKey(x => x.DistrictCode).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.DistrictCode, x.NameTh });
    }
}

public sealed class ThaiSubdistrictPostalCodeConfiguration : IEntityTypeConfiguration<ThaiSubdistrictPostalCode>
{
    public void Configure(EntityTypeBuilder<ThaiSubdistrictPostalCode> b)
    {
        b.HasKey(x => new { x.SubdistrictCode, x.PostalCode });
        b.Property(x => x.PostalCode).HasColumnType("char(5)");
        b.HasOne(x => x.Subdistrict).WithMany(x => x.PostalCodes).HasForeignKey(x => x.SubdistrictCode).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => x.PostalCode);
    }
}
public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> b)
    {
        b.Property(x => x.Name).HasMaxLength(200);
        b.Property(x => x.BusinessType).HasMaxLength(100);
        b.Property(x => x.TaxId).HasMaxLength(20);
        b.Property(x => x.AddressLine).HasMaxLength(500);
        b.Property(x => x.PostalCode).HasColumnType("char(5)");
        b.Property(x => x.TimeZoneId).HasMaxLength(100);
        b.Property(x => x.CurrencyCode).HasColumnType("char(3)");
        b.HasOne<ThaiProvince>().WithMany().HasForeignKey(x => x.ProvinceCode).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ThaiDistrict>().WithMany().HasForeignKey(x => x.DistrictCode).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ThaiSubdistrict>().WithMany().HasForeignKey(x => x.SubdistrictCode).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class AccountUserConfiguration : IEntityTypeConfiguration<AccountUser>
{
    public void Configure(EntityTypeBuilder<AccountUser> b)
    {
        b.Property(x => x.RoleName).HasMaxLength(50);
        b.HasIndex(x => new { x.AccountId, x.UserId }).IsUnique();
        b.HasOne(x => x.Account).WithMany(x => x.Members).HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class AccountInvitationConfiguration : IEntityTypeConfiguration<AccountInvitation>
{
    public void Configure(EntityTypeBuilder<AccountInvitation> b)
    {
        b.Property(x => x.Email).HasMaxLength(256);
        b.Property(x => x.RoleName).HasMaxLength(50);
        b.Property(x => x.TokenHash).HasMaxLength(128);
        b.HasIndex(x => new { x.AccountId, x.Email });
        b.HasOne<Account>().WithMany().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class UserConsentConfiguration : IEntityTypeConfiguration<UserConsent>
{
    public void Configure(EntityTypeBuilder<UserConsent> b)
    {
        b.Property(x => x.ConsentType).HasMaxLength(50);
        b.Property(x => x.Version).HasMaxLength(30);
        b.Property(x => x.IpAddress).HasMaxLength(64);
        b.HasIndex(x => new { x.UserId, x.ConsentType, x.Version }).IsUnique();
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class EmailVerificationLogConfiguration : IEntityTypeConfiguration<EmailVerificationLog>
{
    public void Configure(EntityTypeBuilder<EmailVerificationLog> b)
    {
        b.Property(x => x.IpAddress).HasMaxLength(64);
        b.HasIndex(x => new { x.UserId, x.RequestedAt });
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
public sealed class MaintenanceJobConfiguration : IEntityTypeConfiguration<MaintenanceJob>
{
    public void Configure(EntityTypeBuilder<MaintenanceJob> b)
    {
        b.Property(x => x.PartsCost).HasPrecision(18, 2);
        b.Property(x => x.LaborCost).HasPrecision(18, 2);
        b.Property(x => x.DowntimeHours).HasPrecision(10, 2);
    }
}

public sealed class QualityCriterionConfiguration : IEntityTypeConfiguration<QualityCriterion>
{
    public void Configure(EntityTypeBuilder<QualityCriterion> b) => b.Property(x => x.MaxScore).HasPrecision(9, 3);
}

public sealed class QualityBandConfiguration : IEntityTypeConfiguration<QualityBand>
{
    public void Configure(EntityTypeBuilder<QualityBand> b)
    {
        b.Property(x => x.MinimumScore).HasPrecision(9, 3);
        b.Property(x => x.MaximumScore).HasPrecision(9, 3);
    }
}

public sealed class QualityScoreConfiguration : IEntityTypeConfiguration<QualityScore>
{
    public void Configure(EntityTypeBuilder<QualityScore> b) => b.Property(x => x.TotalScore).HasPrecision(9, 3);
}