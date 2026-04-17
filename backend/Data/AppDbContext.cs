using Microsoft.EntityFrameworkCore;
using ShadowWallet.Models;

namespace ShadowWallet.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<TrustedIssuer> TrustedIssuers => Set<TrustedIssuer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TrustedIssuer>(entity =>
        {
            entity.HasIndex(e => e.Did).IsUnique();
        });
    }
}
