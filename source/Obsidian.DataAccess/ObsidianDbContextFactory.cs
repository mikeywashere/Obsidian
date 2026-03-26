using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Obsidian.DataAccess;

/// <summary>
/// Used by EF Core tooling (dotnet ef migrations) to instantiate ObsidianDbContext
/// without needing to run the full application startup.
/// </summary>
public class ObsidianDbContextFactory : IDesignTimeDbContextFactory<ObsidianDbContext>
{
    public ObsidianDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ObsidianDbContext>();
        optionsBuilder.UseSqlite("Data Source=obsidian.db");
        return new ObsidianDbContext(optionsBuilder.Options);
    }
}
