using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Obsidian.DataAccess;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddObsidianDbContext(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<ObsidianDbContext>(options =>
        {
            options.UseSqlite(connectionString);
        });

        return services;
    }
}
