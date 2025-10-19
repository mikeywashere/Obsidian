using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Obsidian.DataAccess;

public class ObsidianDbContextFactory : IDesignTimeDbContextFactory<ObsidianDbContext>
{
    public ObsidianDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ObsidianDbContext>();
        
        // Default to SQLite for migrations
        // To generate PostgreSQL migrations, set environment variable:
        // DB_PROVIDER=PostgreSQL
        var provider = Environment.GetEnvironmentVariable("DB_PROVIDER") ?? "SQLite";
        
        if (provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
        {
            optionsBuilder.UseNpgsql("Host=localhost;Database=obsidian;Username=postgres;Password=postgres");
        }
        else
        {
            optionsBuilder.UseSqlite("Data Source=obsidian.db");
        }
        
        return new ObsidianDbContext(optionsBuilder.Options);
    }
}
