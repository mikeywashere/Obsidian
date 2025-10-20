using Microsoft.EntityFrameworkCore;
using Obsidian.DataAccess.Entities;

namespace Obsidian.DataAccess;

public class ObsidianDbContext : DbContext
{
    public ObsidianDbContext(DbContextOptions<ObsidianDbContext> options)
        : base(options)
    {
    }

    public DbSet<ServerInfo> Servers { get; set; } = null!;
    public DbSet<ServerLog> ServerLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ServerInfo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Version).HasMaxLength(50);
            entity.Property(e => e.Status).HasConversion<string>();
        });

        modelBuilder.Entity<ServerLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ServerId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.Message).IsRequired();
            entity.Property(e => e.Level).HasConversion<string>();
            // This composite index optimizes queries for retrieving logs by server in chronological order.
            entity.HasIndex(e => new { e.ServerId, e.Timestamp });
            entity.HasOne<ServerInfo>()
                .WithMany()
                .HasForeignKey(e => e.ServerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
