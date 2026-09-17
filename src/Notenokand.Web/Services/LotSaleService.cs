using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Notenokand.Domain.Entities;
using Notenokand.Domain.Enums;
using Notenokand.Infrastructure.Persistence;
using Notenokand.Web.Models.Stock;

namespace Notenokand.Web.Services;

public sealed class LotSaleException(string message) : Exception(message);

public sealed class LotSaleService(NotenokandDbContext db)
{
    // Serializes sale/cancel commands within one account. Stock updates also check the balance in SQL.
    private async Task LockAccountAsync(Guid accountId)
    {
        var resource = $"Notenokand:LotSales:{accountId:N}";
        await db.Database.ExecuteSqlInterpolatedAsync($@"
DECLARE @result int;
EXEC @result = sys.sp_getapplock @Resource={resource}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=10000;
IF @result < 0 THROW 51000, 'Stock operation busy. Retry.', 1;");
    }

    public async Task<Guid> CreateAsync(Guid accountId, Guid userId, SaleCreateViewModel model)
    {
        var errors = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), errors, true);
        if (model.Items is not null)
            foreach (var line in model.Items)
                Validator.TryValidateObject(line, new ValidationContext(line), errors, true);
        if (errors.Count > 0) throw new LotSaleException(errors[0].ErrorMessage!);
        if (model.Items is null) throw new LotSaleException("กรุณาเลือกสินค้า");

        await using var transaction = await db.Database.BeginTransactionAsync();
        await LockAccountAsync(accountId);
        var previous = await db.Sales.AsNoTracking().SingleOrDefaultAsync(x => x.Id == model.RequestId);
        if (previous is not null)
        {
            if (previous.AccountId != accountId) throw new LotSaleException("รหัสคำขอซ้ำ กรุณาเปิดแบบฟอร์มใหม่");
            return previous.Id; // Includes cancelled sales: retrying must never re-sell them.
        }
        var ids = model.Items!.Select(x => x.HarvestItemId).ToArray();
        var stock = await db.HarvestItems.AsNoTracking().Include(x => x.HarvestRound).ThenInclude(x => x.Building)
            .Where(x => ids.Contains(x.Id) && !x.IsDeleted && !x.HarvestRound.IsDeleted &&
                x.HarvestRound.Status != HarvestStatus.Draft && !x.HarvestRound.Building.IsDeleted &&
                x.HarvestRound.Building.AccountId == accountId)
            .ToDictionaryAsync(x => x.Id);
        if (stock.Count != ids.Length) throw new LotSaleException("มีสินค้าที่ไม่พร้อมขายหรือไม่อยู่ในบัญชีนี้");
        foreach (var line in model.Items.OrderBy(x => x.HarvestItemId))
        {
            var item = stock[line.HarvestItemId];
            if (model.SaleDate < item.HarvestRound.HarvestedOn) throw new LotSaleException("วันที่ขายต้องไม่ก่อนวันที่เก็บรังนก");
            var changed = await db.HarvestItems.Where(x => x.Id == line.HarvestItemId && !x.IsDeleted &&
                    x.RemainingWeightKg >= line.WeightKg && x.HarvestRound.Building.AccountId == accountId)
                .ExecuteUpdateAsync(set => set.SetProperty(x => x.RemainingWeightKg, x => x.RemainingWeightKg - line.WeightKg)
                    .SetProperty(x => x.UpdatedAt, DateTimeOffset.UtcNow).SetProperty(x => x.UpdatedByUserId, (Guid?)userId));
            if (changed != 1) throw new LotSaleException("สต็อกไม่เพียงพอหรือถูกขายไปแล้ว กรุณาตรวจยอดคงเหลืออีกครั้ง");
        }
        var buyer = new Buyer { OwnerUserId = userId, Name = model.BuyerName.Trim(), CreatedByUserId = userId };
        var sale = new Sale
        {
            Id = model.RequestId, AccountId = accountId, OwnerUserId = userId,
            DocumentNumber = $"SL-{model.SaleDate:yyyyMMdd}-{model.RequestId:N}",
            SaleDate = model.SaleDate, Buyer = buyer, SaleLocation = model.SaleLocation.Trim(), Notes = model.Notes?.Trim(),
            Subtotal = model.TotalAmount, NetAmount = model.TotalAmount, PaidAmount = model.TotalAmount,
            PaymentMethod = model.PaymentMethod.ToString(), PaymentStatus = PaymentStatus.Paid,
            Status = SaleStatus.Confirmed, CreatedByUserId = userId,
            Items = model.Items.Select(x => new SaleItem
            {
                HarvestItemId = x.HarvestItemId, WeightKg = x.WeightKg, PricePerKg = x.PricePerKg,
                NetAmount = x.Amount, CreatedByUserId = userId
            }).ToList()
        };
        db.Sales.Add(sale);
        var categoryId = await db.ExpenseCategories.Where(x => x.AccountId == accountId && !x.IsDeleted &&
            x.Type == TransactionType.Income && x.Name == "ขายรังนก").Select(x => (Guid?)x.Id).FirstOrDefaultAsync();
        foreach (var group in model.Items.GroupBy(x => stock[x.HarvestItemId].HarvestRound.BuildingId))
        {
            var amount = group.Sum(x => x.Amount);
            var weight = group.Sum(x => x.WeightKg);
            db.FinancialTransactions.Add(new FinancialTransaction
            {
                AccountId = accountId, OwnerUserId = userId, BuildingId = group.Key, SaleId = sale.Id,
                ExpenseCategoryId = categoryId, Type = TransactionType.Income, TransactionDate = model.SaleDate, PaidOn = model.SaleDate,
                Description = $"ขายรังนกจากล็อต {sale.DocumentNumber}", Amount = amount,
                PaymentMethod = model.PaymentMethod, Counterparty = buyer.Name, SaleLocation = sale.SaleLocation,
                AveragePricePerKg = decimal.Round(amount / weight, 2, MidpointRounding.AwayFromZero),
                ReferenceNumber = sale.DocumentNumber, Notes = sale.Notes, CreatedByUserId = userId
            });
        }
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return sale.Id;
    }

    public async Task CancelAsync(Guid accountId, Guid userId, Guid id)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        await LockAccountAsync(accountId);
        var sale = await db.Sales.Include(x => x.Items).SingleOrDefaultAsync(x => x.Id == id && x.AccountId == accountId && !x.IsDeleted);
        if (sale is null) throw new LotSaleException("ไม่พบรายการขายในบัญชีนี้");
        if (sale.Status == SaleStatus.Cancelled) return;
        if (sale.Status != SaleStatus.Confirmed) throw new LotSaleException("สถานะรายการขายไม่พร้อมยกเลิก");
        foreach (var line in sale.Items.OrderBy(x => x.HarvestItemId))
        {
            var changed = await db.HarvestItems.Where(x => x.Id == line.HarvestItemId &&
                    x.HarvestRound.Building.AccountId == accountId && x.RemainingWeightKg + line.WeightKg <= x.WeightKg)
                .ExecuteUpdateAsync(set => set.SetProperty(x => x.RemainingWeightKg, x => x.RemainingWeightKg + line.WeightKg)
                    .SetProperty(x => x.UpdatedAt, DateTimeOffset.UtcNow).SetProperty(x => x.UpdatedByUserId, (Guid?)userId));
            if (changed != 1) throw new LotSaleException("คืนสต็อกไม่สำเร็จ กรุณาตรวจข้อมูลล็อต");
        }
        await db.FinancialTransactions.Where(x => x.SaleId == id && x.AccountId == accountId && !x.IsDeleted)
            .ExecuteUpdateAsync(set => set.SetProperty(x => x.IsDeleted, true)
                .SetProperty(x => x.UpdatedAt, DateTimeOffset.UtcNow).SetProperty(x => x.UpdatedByUserId, (Guid?)userId));
        sale.Status = SaleStatus.Cancelled;
        sale.UpdatedAt = DateTimeOffset.UtcNow; sale.UpdatedByUserId = userId;
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}
