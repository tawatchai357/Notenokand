using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Notenokand.Infrastructure.Persistence;

public sealed class NotenokandDesignTimeDbContextFactory : IDesignTimeDbContextFactory<NotenokandDbContext>
{
    public NotenokandDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<NotenokandDbContext>()
            .UseSqlServer("Server=.\\SQL2016;Database=notenokand;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        return new NotenokandDbContext(options);
    }
}
