using System.ComponentModel.DataAnnotations;
using Notenokand.Web.Models.Harvest;
namespace Notenokand.Tests;
public sealed class HarvestValidationTests
{
    [Fact]
    public void SumsDifferentTypesWithoutRoundingAwayGrams()
    {
        var model = new HarvestEditViewModel { Items = [new() { WeightKg = 2.500m }, new() { WeightKg = 1.251m }] };
        Assert.Equal(3.751m, model.TotalWeightKg);
    }
    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("1.0001")]
    public void RejectsInvalidWeights(string value)
    {
        var line = new HarvestLineViewModel { NestTypeId = Guid.NewGuid(), ColorId = Guid.NewGuid(), ConditionId = Guid.NewGuid(), WeightKg = decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture) };
        Assert.False(Validator.TryValidateObject(line, new ValidationContext(line), new List<ValidationResult>(), true));
    }
    [Fact]
    public void RejectsEmptyRoundAndFutureDate()
    {
        var model = new HarvestEditViewModel { BuildingId = Guid.NewGuid(), Items = [], HarvestedOn = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7)).AddDays(1) };
        var errors = model.Validate(new ValidationContext(model)).ToList();
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(model.Items)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(model.HarvestedOn)));
    }
}
