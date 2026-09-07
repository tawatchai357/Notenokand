using Notenokand.Domain.Enums;
using Notenokand.Domain.Services;

namespace Notenokand.Tests;

public sealed class QualityTrendCalculatorTests
{
    [Fact]
    public void ReturnsInsufficientData_WhenFewerThanSixRoundsExist()
    {
        var result = QualityTrendCalculator.Calculate([70, 72, 75, 80, 82]);
        Assert.Equal(QualityTrend.InsufficientData, result);
    }

    [Theory]
    [InlineData(new double[] { 60, 60, 60, 65, 65, 65 }, QualityTrend.Better)]
    [InlineData(new double[] { 80, 80, 80, 75, 75, 75 }, QualityTrend.Worse)]
    [InlineData(new double[] { 70, 70, 70, 74.99, 74.99, 74.99 }, QualityTrend.Stable)]
    public void UsesTwoConsecutiveThreeRoundAverages(double[] values, QualityTrend expected)
    {
        var scores = values.Select(Convert.ToDecimal).ToArray();
        Assert.Equal(expected, QualityTrendCalculator.Calculate(scores));
    }
}
