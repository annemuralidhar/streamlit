using Microsoft.EntityFrameworkCore;
using OWASPVulnerableApp.Models;

namespace OWASPVulnerableApp.Data;

public class VulnerableDbContext : DbContext
{
    public VulnerableDbContext(DbContextOptions<VulnerableDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed data with hardcoded credentials (A07 - Authentication Failures)
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Username = "admin", Password = "admin123", Email = "admin@vulnerable.com", Role = "admin", SensitiveData = "SSN: 123-45-6789" },
            new User { Id = 2, Username = "user", Password = "password", Email = "user@vulnerable.com", Role = "user", SensitiveData = "CC: 1234-5678-9012" }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Laptop", Description = "High-end laptop", Price = 999.99m, StockQuantity = 10, CreatedBy = 1 },
            new Product { Id = 2, Name = "Mouse", Description = "Wireless mouse", Price = 29.99m, StockQuantity = 100, CreatedBy = 1 }
        );
    }
}
