using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notenokand.Domain.Entities;
using Notenokand.Web.Controllers;
using Notenokand.Web.Models.Buildings;

namespace Notenokand.Tests;

public sealed class BuildingDetailsTests
{
    [Theory]
    [InlineData(500, 200, 300)]
    [InlineData(100, 250, -150)]
    [InlineData(0, 0, 0)]
    public void NetUsesRecordedTransactionsAndDoesNotSubtractBudget(int income, int expense, int expected)
    {
        var model = new BuildingDetailsViewModel
        {
            Building = new BirdBuilding { OwnerUserId = Guid.NewGuid(), Code = "TEST", Name = "Test", ConstructionBudget = 1500000m },
            TotalIncome = income, TotalExpense = expense
        };
        Assert.Equal((decimal)expected, model.Net);
    }

    [Fact]
    public void DetailsRouteRequiresAuthenticatedControllerAndGuidId()
    {
        var type = typeof(BuildingsController);
        Assert.NotEmpty(type.GetCustomAttributes(typeof(AuthorizeAttribute), true));
        var route = Assert.Single(type.GetMethod(nameof(BuildingsController.Details))!.GetCustomAttributes(typeof(HttpGetAttribute), true).Cast<HttpGetAttribute>());
        Assert.Equal("{id:guid}", route.Template);
    }
}
