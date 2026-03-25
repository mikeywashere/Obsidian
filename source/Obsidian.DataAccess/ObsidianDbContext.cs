using Microsoft.EntityFrameworkCore;
using Obsidian.Models;

namespace Obsidian.DataAccess;

public class ObsidianDbContext : DbContext
{
    public ObsidianDbContext(DbContextOptions<ObsidianDbContext> options)
        : base(options)
    {
    }

    public DbSet<ServerInfo> Servers { get; set; }
    public DbSet<ServerLog> ServerLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ServerInfo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Version).HasMaxLength(50);
            entity.Property(e => e.Status).IsRequired();
        });

        modelBuilder.Entity<ServerLog>(entity =>
        {
            entity.HasKey(e => new { e.Timestamp, e.Message });
            entity.Property(e => e.Level).IsRequired();
            entity.Property(e => e.Message).IsRequired();
        });
    }
}
