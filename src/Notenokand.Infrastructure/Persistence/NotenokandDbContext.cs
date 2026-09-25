using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Notenokand.Domain.Entities;
using Notenokand.Infrastructure.Identity;

namespace Notenokand.Infrastructure.Persistence;

public sealed class NotenokandDbContext(DbContextOptions<NotenokandDbContext> options, Microsoft.AspNetCore.Http.IHttpContextAccessor? http = null)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<BirdBuilding> BirdBuildings => Set<BirdBuilding>();
    public DbSet<BuildingUser> BuildingUsers => Set<BuildingUser>();
    public DbSet<DailyLog> DailyLogs => Set<DailyLog>();
    public DbSet<HarvestRound> HarvestRounds => Set<HarvestRound>();
    public DbSet<HarvestItem> HarvestItems => Set<HarvestItem>();
    public DbSet<HarvestSample> HarvestSamples => Set<HarvestSample>();
    public DbSet<SampleCharacteristic> SampleCharacteristics => Set<SampleCharacteristic>();
    public DbSet<MasterOption> MasterOptions => Set<MasterOption>();
    public DbSet<StoredFile> StoredFiles => Set<StoredFile>();
    public DbSet<Buyer> Buyers => Set<Buyer>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
    public DbSet<FinancialTransaction> FinancialTransactions => Set<FinancialTransaction>();
    public DbSet<TransactionReceipt> TransactionReceipts => Set<TransactionReceipt>();
    public DbSet<CalendarAppointment> CalendarAppointments => Set<CalendarAppointment>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<MaintenanceJob> MaintenanceJobs => Set<MaintenanceJob>();
    public DbSet<QualityStandard> QualityStandards => Set<QualityStandard>();
    public DbSet<QualityCriterion> QualityCriteria => Set<QualityCriterion>();
    public DbSet<QualityBand> QualityBands => Set<QualityBand>();
    public DbSet<QualityScore> QualityScores => Set<QualityScore>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Account> Accounts => Set<Account>();
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var claim = http?.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(claim, out var userId))
        {
            ChangeTracker.DetectChanges();
            var entries = ChangeTracker.Entries().Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                .Where(x => AuditSnapshot.Supported(x.Metadata.ClrType.Name)).ToArray();
            if (entries.Length > 0)
            {
                var accountId = await AccountUsers.AsNoTracking().Where(x => x.UserId == userId && x.IsActive && !x.IsDeleted)
                    .Select(x => (Guid?)x.AccountId).FirstOrDefaultAsync(cancellationToken);
                if (accountId.HasValue)
                    foreach (var entry in entries)
                        AuditLogs.Add(new AuditLog
                        {
                            AccountId = accountId, UserId = userId, EntityName = entry.Metadata.ClrType.Name,
                            EntityId = entry.Property("Id").CurrentValue?.ToString() ?? "",
                            Action = entry.State.ToString(), ChangesJson = AuditSnapshot.Changes(entry),
                            OccurredAt = DateTimeOffset.UtcNow
                        });
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }
    public DbSet<AccountUser> AccountUsers => Set<AccountUser>();
    public DbSet<AccountInvitation> AccountInvitations => Set<AccountInvitation>();
    public DbSet<UserConsent> UserConsents => Set<UserConsent>();
    public DbSet<EmailVerificationLog> EmailVerificationLogs => Set<EmailVerificationLog>();
    public DbSet<ThaiProvince> ThaiProvinces => Set<ThaiProvince>();
    public DbSet<ThaiDistrict> ThaiDistricts => Set<ThaiDistrict>();
    public DbSet<ThaiSubdistrict> ThaiSubdistricts => Set<ThaiSubdistrict>();
    public DbSet<ThaiSubdistrictPostalCode> ThaiSubdistrictPostalCodes => Set<ThaiSubdistrictPostalCode>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Keep ASP.NET Identity composite keys below SQL Server's 900-byte clustered-index limit.
        builder.Entity<IdentityUserLogin<Guid>>(entity =>
        {
            entity.Property(x => x.LoginProvider).HasMaxLength(128);
            entity.Property(x => x.ProviderKey).HasMaxLength(128);
        });
        builder.Entity<IdentityUserToken<Guid>>(entity =>
        {
            entity.Property(x => x.LoginProvider).HasMaxLength(128);
            entity.Property(x => x.Name).HasMaxLength(128);
        });

        builder.ApplyConfigurationsFromAssembly(typeof(NotenokandDbContext).Assembly);
        builder.Entity<HarvestRound>().Property(x => x.RowVersion).IsRowVersion();

        foreach (var entityType in builder.Model.GetEntityTypes()
                     .Where(x => typeof(Notenokand.Domain.Common.Entity).IsAssignableFrom(x.ClrType)))
        {
            builder.Entity(entityType.ClrType).Property(nameof(Notenokand.Domain.Common.Entity.CreatedAt)).HasPrecision(0);
        }
    }
}
