using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Obsidian.DataAccess;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddObsidianDbContext(
        this IServiceCollection services,
        string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string cannot be null or whitespace.", nameof(connectionString));
        }
        services.AddDbContext<ObsidianDbContext>(options =>
        {
            options.UseSqlite(connectionString);
        });

        return services;
    }
}
