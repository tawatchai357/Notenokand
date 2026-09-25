namespace Notenokand.Web.Models.Reports;
public sealed class ReportSummaryViewModel
{
    public DateOnly From { get; init; }
    public DateOnly To { get; init; }
    public decimal Income { get; init; }
    public decimal Expense { get; init; }
    public decimal HarvestKg { get; init; }
    public decimal StockKg { get; init; }
    public decimal SalesAmount { get; init; }
    public decimal SalesKg { get; init; }
}
