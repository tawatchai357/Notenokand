using Notenokand.Domain.Entities;

namespace Notenokand.Web.Models.Buildings;

public sealed class BuildingDetailsViewModel
{
    public required BirdBuilding Building { get; init; }
    public string Location { get; init; } = string.Empty;
    public decimal TotalIncome { get; init; }
    public decimal TotalExpense { get; init; }
    public decimal Net => TotalIncome - TotalExpense;
    public decimal OperatingExpense { get; init; }
    public decimal CapitalExpense { get; init; }
    public decimal InvestmentBase => (Building.InitialInvestmentAmount ?? 0m) + CapitalExpense;
    public decimal OperatingProfit => TotalIncome - OperatingExpense;
    public decimal ReturnPercent => InvestmentBase > 0 ? OperatingProfit / InvestmentBase * 100m : 0m;
    public decimal RemainingToPayback => Math.Max(0m, InvestmentBase - OperatingProfit);
    public decimal TotalHarvestKg { get; init; }
    public int HarvestCount { get; init; }
    public IReadOnlyList<FinancialTransaction> Transactions { get; init; } = [];
    public IReadOnlyList<HarvestRound> Harvests { get; init; } = [];
    public IReadOnlyList<CalendarAppointment> Appointments { get; init; } = [];
    public IReadOnlyDictionary<Guid, string> OptionNames { get; init; } = new Dictionary<Guid, string>();
}
