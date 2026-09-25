using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Notenokand.Domain.Entities;
using Notenokand.Domain.Enums;
using Notenokand.Infrastructure.Persistence;
using Notenokand.Web.Models.Stock;
using Notenokand.Web.Services;

namespace Notenokand.Tests;

public sealed class LotSaleSqlFactAttribute : FactAttribute
{
    public LotSaleSqlFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("NOTENOKAND_TEST_SQLSERVER")))
            Skip = "Set NOTENOKAND_TEST_SQLSERVER to run against a newly created disposable test database.";
    }
}

public sealed class LotSaleSqlTests
{
    [LotSaleSqlFact]
    public async Task SaleLifecycleIsAtomicTenantScopedIdempotentAndConcurrencySafe()
    {
        var databaseName = "Notenokand_LotSaleTests_" + Guid.NewGuid().ToString("N");
        var connection = new SqlConnectionStringBuilder
        {
            DataSource = Environment.GetEnvironmentVariable("NOTENOKAND_TEST_SQLSERVER")!,
            InitialCatalog = databaseName, IntegratedSecurity = true, TrustServerCertificate = true
        }.ConnectionString;
        var options = new DbContextOptionsBuilder<NotenokandDbContext>().UseSqlServer(connection).Options;
        NotenokandDbContext Open() => new(options);
        await using var setup = Open();
        try
        {
            await setup.Database.EnsureCreatedAsync();
            var owner = Guid.NewGuid();
            var account = new Account { Name = "Stock test" };
            var outsider = new Account { Name = "Outside account" };
            setup.Accounts.AddRange(account, outsider);
            var building1 = new BirdBuilding { AccountId = account.Id, OwnerUserId = owner, Name = "One", Code = "B1" };
            var building2 = new BirdBuilding { AccountId = account.Id, OwnerUserId = owner, Name = "Two", Code = "B2" };
            setup.BirdBuildings.AddRange(building1, building2);
            var type = new MasterOption { Category = "HarvestNestType", Code = "TEST", Name = "Test" };
            setup.MasterOptions.Add(type);
            var date = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7)).AddDays(-1);
            var round1 = new HarvestRound { BuildingId = building1.Id, RoundNumber = "L1", StartedOn = date, HarvestedOn = date, Status = HarvestStatus.Confirmed, TotalWeightKg = 2m };
            var round2 = new HarvestRound { BuildingId = building2.Id, RoundNumber = "L2", StartedOn = date, HarvestedOn = date, Status = HarvestStatus.Confirmed, TotalWeightKg = 2m };
            var first = new HarvestItem { HarvestRound = round1, NestTypeId = type.Id, WeightKg = 2m, RemainingWeightKg = 2m };
            var second = new HarvestItem { HarvestRound = round2, NestTypeId = type.Id, WeightKg = 2m, RemainingWeightKg = 2m };
            setup.HarvestItems.AddRange(first, second);
            await setup.SaveChangesAsync();

            SaleCreateViewModel Request(params (Guid Id, decimal Kg)[] rows) => new()
            {
                BuyerName = "Test buyer", SaleLocation = "Test market", ReceivedInFull = true,
                Items = rows.Select(x => new SaleLineInput { HarvestItemId = x.Id, WeightKg = x.Kg, PricePerKg = 100m }).ToList()
            };
            async Task<Guid> Create(SaleCreateViewModel model, Guid? tenant = null)
            {
                await using var db = Open();
                return await new LotSaleService(db).CreateAsync(tenant ?? account.Id, owner, model);
            }
            async Task<decimal> Balance(Guid id)
            {
                await using var db = Open();
                return await db.HarvestItems.Where(x => x.Id == id).Select(x => x.RemainingWeightKg).SingleAsync();
            }
            var request = Request((first.Id, 0.5m), (second.Id, 1m));
            var id = await Create(request);
            Assert.Equal(1.5m, await Balance(first.Id));
            Assert.Equal(1m, await Balance(second.Id));
            Assert.Equal(id, await Create(request));
            Assert.Equal(1.5m, await Balance(first.Id));
            await using (var check = Open())
            {
                var income = await check.FinancialTransactions.Where(x => x.SaleId == id).ToListAsync();
                Assert.Equal(2, income.Count);
                Assert.Equal(150m, income.Sum(x => x.Amount));
                Assert.Equal(50m, income.Single(x => x.BuildingId == building1.Id).Amount);
            }
            await Assert.ThrowsAsync<LotSaleException>(() => Create(Request((first.Id, .1m)), outsider.Id));
            await Assert.ThrowsAsync<LotSaleException>(() => Create(request, outsider.Id));
            // The first update succeeds, the second fails; the complete transaction must roll back.
            var ordered = new[] { first.Id, second.Id }.OrderBy(x => x).ToArray();
            await Assert.ThrowsAsync<LotSaleException>(() => Create(Request((ordered[0], .1m), (ordered[1], 99m))));
            Assert.Equal(1.5m, await Balance(first.Id));
            Assert.Equal(1m, await Balance(second.Id));
            await using (var db = Open())
                await Assert.ThrowsAsync<LotSaleException>(() => new LotSaleService(db).CancelAsync(outsider.Id, owner, id));
            for (var attempt = 0; attempt < 2; attempt++)
            {
                await using var db = Open();
                await new LotSaleService(db).CancelAsync(account.Id, owner, id);
            }
            Assert.Equal(2m, await Balance(first.Id));
            Assert.Equal(2m, await Balance(second.Id));
            await using (var check = Open())
                Assert.Empty(await check.FinancialTransactions.Where(x => x.SaleId == id && !x.IsDeleted).ToListAsync());
            Assert.Equal(id, await Create(request));
            Assert.Equal(2m, await Balance(first.Id));

            async Task<bool> TrySell()
            {
                try { await Create(Request((first.Id, 1.5m))); return true; }
                catch (LotSaleException) { return false; }
            }
            var attempts = await Task.WhenAll(TrySell(), TrySell());
            Assert.Single(attempts, x => x);
            Assert.Equal(.5m, await Balance(first.Id));
            var duplicate = Request((second.Id, .25m));
            var duplicateIds = await Task.WhenAll(Create(duplicate), Create(duplicate));
            Assert.Equal(duplicateIds[0], duplicateIds[1]);
            Assert.Equal(1.75m, await Balance(second.Id));
            var partial = Request((second.Id, .25m));
            partial.ReceivedInFull = false;
            partial.InitialPaidAmount = 10m;
            partial.DueDate = date.AddDays(30);
            var partialId = await Create(partial);
            await using (var paymentDb = Open())
                await new LotSaleService(paymentDb).RecordPaymentAsync(account.Id, owner, partialId,
                    new SalePaymentViewModel { Amount = 15m, PaidOn = partial.SaleDate, PaymentMethod = PaymentMethod.Cash });
            await using (var paymentCheck = Open())
            {
                var paidSale = await paymentCheck.Sales.SingleAsync(x => x.Id == partialId);
                Assert.Equal(25m, paidSale.PaidAmount);
                Assert.Equal(PaymentStatus.Paid, paidSale.PaymentStatus);
                Assert.Equal(25m, await paymentCheck.FinancialTransactions.Where(x => x.SaleId == partialId && !x.IsDeleted).SumAsync(x => x.Amount));
            }
        }
        finally
        {
            // Never delete a configured user database: only this generated disposable database.
            if (databaseName.StartsWith("Notenokand_LotSaleTests_", StringComparison.Ordinal) &&
                Guid.TryParseExact(databaseName["Notenokand_LotSaleTests_".Length..], "N", out _))
                await setup.Database.EnsureDeletedAsync();
        }
    }
}
