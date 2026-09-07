using Notenokand.Domain.Common;
using Notenokand.Domain.Enums;

namespace Notenokand.Domain.Entities;

public sealed class Asset : Entity
{
    public Guid BuildingId { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Location { get; set; }
    public DateOnly? InstalledOn { get; set; }
    public DateOnly? NextInspectionOn { get; set; }
    public bool IsActive { get; set; } = true;
    public BirdBuilding Building { get; set; } = null!;
}

public sealed class MaintenanceJob : Entity
{
    public Guid BuildingId { get; set; }
    public Guid? AssetId { get; set; }
    public MaintenanceType Type { get; set; }
    public MaintenancePriority Priority { get; set; }
    public MaintenanceStatus Status { get; set; }
    public required string Issue { get; set; }
    public DateTimeOffset ReportedAt { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public Guid? AssignedUserId { get; set; }
    public decimal PartsCost { get; set; }
    public decimal LaborCost { get; set; }
    public decimal DowntimeHours { get; set; }
    public string? AcceptanceResult { get; set; }
    public DateOnly? NextInspectionOn { get; set; }
}

public sealed class QualityStandard : Entity
{
    public required Guid OwnerUserId { get; set; }
    public required string Name { get; set; }
    public int Version { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public bool IsActive { get; set; }
    public ICollection<QualityCriterion> Criteria { get; set; } = [];
    public ICollection<QualityBand> Bands { get; set; } = [];
}

public sealed class QualityCriterion : Entity
{
    public Guid QualityStandardId { get; set; }
    public required string Name { get; set; }
    public decimal MaxScore { get; set; }
    public int SortOrder { get; set; }
    public QualityStandard QualityStandard { get; set; } = null!;
}

public sealed class QualityBand : Entity
{
    public Guid QualityStandardId { get; set; }
    public required string Name { get; set; }
    public decimal MinimumScore { get; set; }
    public decimal MaximumScore { get; set; }
    public int SortOrder { get; set; }
}

public sealed class QualityScore : Entity
{
    public Guid HarvestRoundId { get; set; }
    public Guid QualityStandardId { get; set; }
    public decimal TotalScore { get; set; }
    public required string ResultLabel { get; set; }
    public Guid ConfirmedByUserId { get; set; }
    public DateTimeOffset ConfirmedAt { get; set; }
    public string? Explanation { get; set; }
    public HarvestRound HarvestRound { get; set; } = null!;
}

public sealed class AuditLog
{
    public long Id { get; set; }
    public Guid? UserId { get; set; }
    public required string Action { get; set; }
    public required string EntityName { get; set; }
    public required string EntityId { get; set; }
    public string? ChangesJson { get; set; }
    public string? IpAddress { get; set; }
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
}
