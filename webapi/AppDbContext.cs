using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PriceBridge.Entities;

namespace PriceBridge;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<OzonProduct> OzonProducts => Set<OzonProduct>();
    public DbSet<TaobaoProduct> TaobaoProducts => Set<TaobaoProduct>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OzonProduct>(entity =>
        {
            entity.HasIndex(p => p.Sku).IsUnique();
            entity.Property(p => p.Price).HasPrecision(18, 2);
            entity.Property(p => p.OriginalPrice).HasPrecision(18, 2);
            entity
                .Property(p => p.Characteristics)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                    v =>
                        JsonSerializer.Deserialize<Dictionary<string, string>>(
                            v,
                            JsonSerializerOptions.Default
                        ),
                    new ValueComparer<Dictionary<string, string>?>(
                        (a, b) => a == b || (a != null && b != null && a.SequenceEqual(b)),
                        v => v == null ? 0 : v.Aggregate(0, (h, kv) => HashCode.Combine(h, kv)),
                        v => v == null ? null : new Dictionary<string, string>(v)
                    )
                );
            entity
                .HasMany(p => p.TaobaoProducts)
                .WithOne(t => t.OzonProduct)
                .HasForeignKey(t => t.OzonProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TaobaoProduct>(entity =>
        {
            entity.Property(t => t.Price).HasPrecision(18, 2);
            entity.Property(t => t.Currency).HasMaxLength(3);
        });
    }
}
