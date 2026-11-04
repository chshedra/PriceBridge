using Microsoft.EntityFrameworkCore;
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
