using Notenokand.Domain.Common;
using Notenokand.Domain.Enums;

namespace Notenokand.Domain.Entities;

public sealed class BirdBuilding : Entity
{
    public Guid AccountId { get; set; }
    public required Guid OwnerUserId { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Province { get; set; }
    public string? Address { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public DateOnly? StartedOn { get; set; }
    public int? FloorCount { get; set; }
    public int? RoomCount { get; set; }
    public decimal? AreaSquareMeters { get; set; }
    public string? Notes { get; set; }
    public BuildingStatus Status { get; set; } = BuildingStatus.Active;
    public ICollection<BuildingUser> Users { get; set; } = [];
}

public sealed class BuildingUser : Entity
{
    public Guid BuildingId { get; set; }
    public Guid UserId { get; set; }
    public BirdBuilding Building { get; set; } = null!;
}

public sealed class DailyLog : Entity
{
    public Guid BuildingId { get; set; }
    public DateOnly LogDate { get; set; }
    public DailyLogType Type { get; set; }
    public required string Title { get; set; }
    public string? Details { get; set; }
    public Guid? HarvestRoundId { get; set; }
    public Guid? FinancialTransactionId { get; set; }
    public Guid? MaintenanceJobId { get; set; }
    public BirdBuilding Building { get; set; } = null!;
    public ICollection<StoredFile> Files { get; set; } = [];
}

public sealed class HarvestRound : Entity
{
    public Guid BuildingId { get; set; }
    public required string RoundNumber { get; set; }
    public string? HarvestArea { get; set; }
    public DateOnly StartedOn { get; set; }
    public DateOnly HarvestedOn { get; set; }
    public decimal TotalWeightKg { get; set; }
    public int NestCount { get; set; }
    public int SampleCount { get; set; }
    public Guid? CollectorUserId { get; set; }
    public Guid? InspectorUserId { get; set; }
    public decimal LaborCost { get; set; }
    public string? EnvironmentNotes { get; set; }
    public HarvestStatus Status { get; set; }
    public BirdBuilding Building { get; set; } = null!;
    public ICollection<HarvestItem> Items { get; set; } = [];
    public ICollection<HarvestSample> Samples { get; set; } = [];
}

public sealed class HarvestItem : Entity
{
    public Guid HarvestRoundId { get; set; }
    public Guid NestTypeId { get; set; }
    public Guid? GradeId { get; set; }
    public Guid? ColorId { get; set; }
    public Guid? NestSourceId { get; set; }
    public decimal WeightKg { get; set; }
    public decimal RemainingWeightKg { get; set; }
    public int? NestCount { get; set; }
    public HarvestRound HarvestRound { get; set; } = null!;
}

public sealed class HarvestSample : Entity
{
    public Guid HarvestRoundId { get; set; }
    public required string SampleNumber { get; set; }
    public decimal? WeightGrams { get; set; }
    public Guid? NestTypeId { get; set; }
    public Guid? GradeId { get; set; }
    public Guid? ColorId { get; set; }
    public Guid? NestSourceId { get; set; }
    public Guid? BellyConditionId { get; set; }
    public Guid? FeatherLevelId { get; set; }
    public Guid? CleanlinessLevelId { get; set; }
    public string? InspectorNotes { get; set; }
    public HarvestRound HarvestRound { get; set; } = null!;
    public ICollection<SampleCharacteristic> Characteristics { get; set; } = [];
}

public sealed class SampleCharacteristic : Entity
{
    public Guid HarvestSampleId { get; set; }
    public Guid CharacteristicId { get; set; }
    public HarvestSample HarvestSample { get; set; } = null!;
}

public sealed class MasterOption : Entity
{
    public required string Category { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class StoredFile : Entity
{
    public Guid BuildingId { get; set; }
    public Guid? DailyLogId { get; set; }
    public Guid? HarvestRoundId { get; set; }
    public Guid? HarvestSampleId { get; set; }
    public Guid? MaintenanceJobId { get; set; }
    public Guid? SaleId { get; set; }
    public ImageKind Kind { get; set; }
    public required string StorageKey { get; set; }
    public required string OriginalFileName { get; set; }
    public required string ContentType { get; set; }
    public long SizeBytes { get; set; }
    public string? Sha256 { get; set; }
    public DailyLog? DailyLog { get; set; }
}
