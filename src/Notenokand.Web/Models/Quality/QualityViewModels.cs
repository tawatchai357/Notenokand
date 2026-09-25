using System.ComponentModel.DataAnnotations;
using Notenokand.Domain.Entities;
using Notenokand.Domain.Enums;

namespace Notenokand.Web.Models.Quality;

public sealed class QualityScoreEditViewModel
{
    [Required] public Guid HarvestRoundId { get; set; }
    [Range(typeof(decimal), "0", "100", ErrorMessage = "คะแนนต้องอยู่ระหว่าง 0 ถึง 100")] public decimal TotalScore { get; set; }
    [StringLength(2000)] public string? Explanation { get; set; }
}

public sealed class QualityRoundRow
{
    public required HarvestRound Round { get; init; }
    public QualityScore? Score { get; init; }
}

public sealed class QualityIndexViewModel
{
    public IReadOnlyList<QualityRoundRow> Rounds { get; init; } = [];
    public QualityTrend Trend { get; init; }
    public decimal? AverageScore { get; init; }
}
