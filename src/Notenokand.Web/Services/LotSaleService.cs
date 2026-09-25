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
        var initialPaid = model.ReceivedInFull ? model.TotalAmount : model.InitialPaidAmount ?? 0m;
        var sale = new Sale
        {
            Id = model.RequestId, AccountId = accountId, OwnerUserId = userId,
            DocumentNumber = $"SL-{model.SaleDate:yyyyMMdd}-{model.RequestId:N}",
            SaleDate = model.SaleDate, Buyer = buyer, SaleLocation = model.SaleLocation.Trim(), Notes = model.Notes?.Trim(),
            Subtotal = model.TotalAmount, NetAmount = model.TotalAmount, PaidAmount = initialPaid, DueDate = initialPaid < model.TotalAmount ? model.DueDate : null,
            PaymentMethod = model.PaymentMethod.ToString(), PaymentStatus = initialPaid == 0 ? PaymentStatus.Unpaid : initialPaid < model.TotalAmount ? PaymentStatus.PartiallyPaid : PaymentStatus.Paid,
            Status = SaleStatus.Confirmed, CreatedByUserId = userId,
            Items = model.Items.Select(x => new SaleItem
            {
                HarvestItemId = x.HarvestItemId, WeightKg = x.WeightKg, PricePerKg = x.PricePerKg,
                NetAmount = x.Amount, CreatedByUserId = userId
            }).ToList()
        };
        db.Sales.Add(sale);
        if (initialPaid > 0)
        {
            var groups = model.Items.GroupBy(x => stock[x.HarvestItemId].HarvestRound.BuildingId)
                .Select(x => (BuildingId: x.Key, SaleAmount: x.Sum(i => i.Amount), Weight: x.Sum(i => i.WeightKg))).ToList();
            await AddPaymentTransactionsAsync(accountId, userId, sale, buyer.Name, model.PaymentMethod, model.SaleDate, initialPaid, groups, sale.DocumentNumber);
        }
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return sale.Id;
    }

    public async Task RecordPaymentAsync(Guid accountId, Guid userId, Guid saleId, SalePaymentViewModel model)
    {
        var errors = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), errors, true);
        if (errors.Count > 0) throw new LotSaleException(errors[0].ErrorMessage!);
        await using var transaction = await db.Database.BeginTransactionAsync();
        await LockAccountAsync(accountId);
        var reference = $"PY-{model.RequestId:N}";
        if (await db.FinancialTransactions.AnyAsync(x => x.AccountId == accountId && x.SaleId == saleId && x.ReferenceNumber == reference))
            return;
        var sale = await db.Sales.Include(x => x.Buyer).Include(x => x.Items).ThenInclude(x => x.HarvestItem).ThenInclude(x => x.HarvestRound)
            .SingleOrDefaultAsync(x => x.Id == saleId && x.AccountId == accountId && !x.IsDeleted);
        if (sale is null || sale.Status != SaleStatus.Confirmed) throw new LotSaleException("รายการขายไม่พร้อมรับชำระ");
        var outstanding = sale.NetAmount - sale.PaidAmount;
        if (model.Amount > outstanding) throw new LotSaleException("ยอดรับชำระมากกว่ายอดค้าง");
        if (model.PaidOn < sale.SaleDate) throw new LotSaleException("วันที่รับเงินต้องไม่ก่อนวันที่ขาย");
        var groups = sale.Items.GroupBy(x => x.HarvestItem.HarvestRound.BuildingId)
            .Select(x => (BuildingId: x.Key, SaleAmount: x.Sum(i => i.NetAmount), Weight: x.Sum(i => i.WeightKg))).ToList();
        await AddPaymentTransactionsAsync(accountId, userId, sale, sale.Buyer.Name, model.PaymentMethod, model.PaidOn, model.Amount, groups, reference);
        sale.PaidAmount += model.Amount;
        sale.PaymentMethod = model.PaymentMethod.ToString();
        sale.PaymentStatus = sale.PaidAmount >= sale.NetAmount ? PaymentStatus.Paid : PaymentStatus.PartiallyPaid;
        if (sale.PaymentStatus == PaymentStatus.Paid) sale.DueDate = null;
        sale.UpdatedAt=DateTimeOffset.UtcNow; sale.UpdatedByUserId=userId;
        await db.SaveChangesAsync(); await transaction.CommitAsync();
    }

    private async Task AddPaymentTransactionsAsync(Guid accountId, Guid userId, Sale sale, string buyerName,
        PaymentMethod method, DateOnly paidOn, decimal paymentAmount,
        IReadOnlyList<(Guid BuildingId, decimal SaleAmount, decimal Weight)> groups, string reference)
    {
        var categoryId = await db.ExpenseCategories.Where(x => x.AccountId == accountId && !x.IsDeleted &&
            x.Type == TransactionType.Income && x.Name == "ขายรังนก").Select(x => (Guid?)x.Id).FirstOrDefaultAsync();
        var remaining = paymentAmount;
        for (var index = 0; index < groups.Count; index++)
        {
            var group = groups[index];
            var amount = index == groups.Count - 1 ? remaining :
                decimal.Round(paymentAmount * group.SaleAmount / sale.NetAmount, 2, MidpointRounding.AwayFromZero);
            remaining -= amount;
            if (amount <= 0) continue;
            db.FinancialTransactions.Add(new FinancialTransaction
            {
                AccountId=accountId, OwnerUserId=userId, BuildingId=group.BuildingId, SaleId=sale.Id,
                ExpenseCategoryId=categoryId, Type=TransactionType.Income, TransactionDate=paidOn, PaidOn=paidOn,
                Description=$"รับชำระค่าขายรังนก {sale.DocumentNumber}", Amount=amount, PaymentMethod=method,
                Counterparty=buyerName, SaleLocation=sale.SaleLocation,
                AveragePricePerKg=group.Weight > 0 ? decimal.Round(group.SaleAmount / group.Weight, 2, MidpointRounding.AwayFromZero) : null,
                ReferenceNumber=reference, Notes=sale.Notes, CreatedByUserId=userId
            });
        }
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
