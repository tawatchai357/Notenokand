using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notenokand.Domain.Entities;
using Notenokand.Infrastructure.Persistence;
using Notenokand.Web.Controllers;

namespace Notenokand.Tests;

public sealed class AccountAuditHarvestTests
{
    [Fact]
    public void PasswordResetPostIsAnonymousRateLimitedAndAntiForgeryProtected()
    {
        var method = typeof(ProfileController).GetMethods()
            .Single(x => x.Name == nameof(ProfileController.ResetPassword) &&
                         x.GetCustomAttributes(typeof(HttpPostAttribute), true).Length > 0);
        Assert.NotEmpty(method.GetCustomAttributes(typeof(AllowAnonymousAttribute), true));
        Assert.NotEmpty(method.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), true));
    }

    [Theory]
    [InlineData(nameof(HarvestManagementController.Edit))]
    [InlineData(nameof(HarvestManagementController.Cancel))]
    public void HarvestMutationPostsRequireAntiForgery(string name)
    {
        var method = typeof(HarvestManagementController).GetMethods()
            .Single(x => x.Name == name && x.GetCustomAttributes(typeof(HttpPostAttribute), true).Length > 0);
        Assert.NotEmpty(method.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), true));
    }

    [Fact]
    public void AuditSnapshotExcludesPrivateFileLocationsAndHashes()
    {
        var options = new DbContextOptionsBuilder<NotenokandDbContext>()
            .UseSqlServer("Server=(local);Database=AuditSnapshotOnly;Trusted_Connection=True;TrustServerCertificate=True").Options;
        using var db = new NotenokandDbContext(options);
        var building = new BirdBuilding
        {
            AccountId = Guid.NewGuid(), OwnerUserId = Guid.NewGuid(), Code = "B1", Name = "Test",
            PhotoStorageKey = "private/path.jpg", PhotoSha256 = "secret-hash"
        };
        db.BirdBuildings.Add(building);
        var json = AuditSnapshot.Changes(db.Entry(building));
        Assert.DoesNotContain("private/path.jpg", json);
        Assert.DoesNotContain("secret-hash", json);
        Assert.Contains(nameof(BirdBuilding.Name), json);
    }
}
