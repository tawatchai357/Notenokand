using System.ComponentModel.DataAnnotations;
using Notenokand.Web.Models.Stock;
namespace Notenokand.Tests;

public sealed class LotSaleValidationTests
{
    [Fact]
    public void RoundsEachLineThenSumsAndPreservesGrams()
    {
        var model = new SaleCreateViewModel { Items = [
            new() { HarvestItemId = Guid.NewGuid(), WeightKg = 1.001m, PricePerKg = 12.50m },
            new() { HarvestItemId = Guid.NewGuid(), WeightKg = 0.001m, PricePerKg = 15m }] };
        Assert.Equal(1.002m, model.TotalKg);
        Assert.Equal(12.53m, model.TotalAmount);
    }
    [Theory]
    [InlineData("0", "10")]
    [InlineData("1.0001", "10")]
    [InlineData("1", "10.001")]
    [InlineData("0.001", "0.01")]
    public void RejectsInvalidWeightPriceOrZeroRoundedAmount(string kg, string price)
    {
        var item = new SaleLineInput { HarvestItemId = Guid.NewGuid(), WeightKg = decimal.Parse(kg, System.Globalization.CultureInfo.InvariantCulture), PricePerKg = decimal.Parse(price, System.Globalization.CultureInfo.InvariantCulture) };
        Assert.False(Validator.TryValidateObject(item, new ValidationContext(item), [], true));
    }
    [Fact]
    public void RejectsDuplicateLotsAndMissingDueDateForOutstandingBalance()
    {
        var id = Guid.NewGuid();
        var model = new SaleCreateViewModel { Items = [
            new() { HarvestItemId = id, WeightKg = 1m, PricePerKg = 10m },
            new() { HarvestItemId = id, WeightKg = 1m, PricePerKg = 10m }] };
        var errors = model.Validate(new ValidationContext(model)).ToList();
        Assert.Contains(errors, x => x.MemberNames.Contains(nameof(model.DueDate)));
        Assert.Contains(errors, x => x.MemberNames.Contains(nameof(model.Items)));
    }
    [Fact]
    public void ExtremeInputDoesNotOverflowPreview()
    {
        Assert.Equal(0m, new SaleLineInput { WeightKg = decimal.MaxValue, PricePerKg = decimal.MaxValue }.Amount);
    }
}
