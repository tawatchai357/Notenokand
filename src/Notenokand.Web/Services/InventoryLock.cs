using Microsoft.EntityFrameworkCore;
using Notenokand.Infrastructure.Persistence;
namespace Notenokand.Web.Services;

public static class InventoryLock
{
    public static Task AcquireAsync(NotenokandDbContext db, Guid accountId)
    {
        var resource = $"Notenokand:LotSales:{accountId:N}";
        return db.Database.ExecuteSqlInterpolatedAsync($@"
DECLARE @result int;
EXEC @result = sys.sp_getapplock @Resource={resource}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=10000;
IF @result < 0 THROW 51000, 'Stock operation busy. Retry.', 1;");
    }
}
