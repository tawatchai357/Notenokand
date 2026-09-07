using Notenokand.Domain.Enums;

namespace Notenokand.Domain.Services;

public static class QualityTrendCalculator
{
    public static QualityTrend Calculate(IReadOnlyList<decimal> scores)
    {
        if (scores.Count < 6)
            return QualityTrend.InsufficientData;

        var previousAverage = scores.Skip(scores.Count - 6).Take(3).Average();
        var latestAverage = scores.TakeLast(3).Average();
        var difference = latestAverage - previousAverage;

        return difference >= 5m
            ? QualityTrend.Better
            : difference <= -5m
                ? QualityTrend.Worse
                : QualityTrend.Stable;
    }
}
