using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Obsidian.DataAccess;

public class ObsidianDbContextFactory : IDesignTimeDbContextFactory<ObsidianDbContext>
{
    public ObsidianDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ObsidianDbContext>();
        optionsBuilder.UseSqlite("Data Source=obsidian.db");
        
        return new ObsidianDbContext(optionsBuilder.Options);
    }
}
