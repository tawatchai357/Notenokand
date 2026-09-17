using Notenokand.Domain.Entities;

namespace Notenokand.Web.Models.Buildings;

public sealed class BuildingDetailsViewModel
{
    public required BirdBuilding Building { get; init; }
    public string Location { get; init; } = string.Empty;
    public decimal TotalIncome { get; init; }
    public decimal TotalExpense { get; init; }
    public decimal Net => TotalIncome - TotalExpense;
    public decimal TotalHarvestKg { get; init; }
    public int HarvestCount { get; init; }
    public IReadOnlyList<FinancialTransaction> Transactions { get; init; } = [];
    public IReadOnlyList<HarvestRound> Harvests { get; init; } = [];
    public IReadOnlyList<CalendarAppointment> Appointments { get; init; } = [];
    public IReadOnlyDictionary<Guid, string> OptionNames { get; init; } = new Dictionary<Guid, string>();
}
