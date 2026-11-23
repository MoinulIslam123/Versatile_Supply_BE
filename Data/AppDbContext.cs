using Microsoft.EntityFrameworkCore;
using VersatileApi.Models;

namespace VersatileApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ContentItem> ContentItems => Set<ContentItem>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<ContentItem>().HasIndex(i => i.Collection);
        b.Entity<AdminUser>().HasIndex(u => u.Username).IsUnique();
    }
}
