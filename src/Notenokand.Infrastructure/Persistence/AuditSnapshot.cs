using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
namespace Notenokand.Infrastructure.Persistence;

public static class AuditSnapshot
{
    private static readonly HashSet<string> Entities = [
        "Account", "AccountUser", "BirdBuilding", "BuildingUser", "HarvestRound", "HarvestItem",
        "Sale", "SaleItem", "FinancialTransaction", "ExpenseCategory", "CalendarAppointment",
        "Asset", "MaintenanceJob", "QualityStandard", "QualityCriterion", "QualityBand", "QualityScore"
    ];
    public static bool Supported(string entity) => Entities.Contains(entity);
    public static string Changes(EntityEntry entry)
    {
        var fields = entry.Properties.Where(x => x.Metadata.Name is not ("CreatedAt" or "UpdatedAt" or "CreatedByUserId" or "UpdatedByUserId") &&
            !x.Metadata.Name.Contains("StorageKey") && !x.Metadata.Name.Contains("Sha256") &&
            (entry.State != EntityState.Modified || x.IsModified)).ToArray();
        return JsonSerializer.Serialize(fields.ToDictionary(x => x.Metadata.Name, x => new {
            Before = entry.State == EntityState.Added ? null : x.OriginalValue,
            After = entry.State == EntityState.Deleted ? null : x.CurrentValue
        }));
    }
}
