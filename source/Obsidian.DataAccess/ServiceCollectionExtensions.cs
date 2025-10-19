using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Obsidian.DataAccess;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddObsidianDbContext(
        this IServiceCollection services,
        string connectionString,
        DatabaseProvider provider = DatabaseProvider.SQLite)
    {
        services.AddDbContext<ObsidianDbContext>(options =>
        {
            switch (provider)
            {
                case DatabaseProvider.PostgreSQL:
                    options.UseNpgsql(connectionString);
                    break;
                case DatabaseProvider.SQLite:
                default:
                    options.UseSqlite(connectionString);
                    break;
            }
        });

        return services;
    }
}

public enum DatabaseProvider
{
    SQLite,
    PostgreSQL
}
