using Microsoft.EntityFrameworkCore;
using EShoppingZone.Models;

namespace EShoppingZone.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Wallet> Wallets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Product configuration
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);

        // Order configuration
        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasPrecision(18, 2);

        // OrderItem configuration
        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.UnitPrice)
            .HasPrecision(18, 2);

        // Wallet configuration
        modelBuilder.Entity<Wallet>()
            .Property(w => w.Balance)
            .HasPrecision(18, 2);

        // Seed data
        modelBuilder.Entity<Category>().HasData(
            new Category { CategoryId = 1, Name = "Electronics" },
            new Category { CategoryId = 2, Name = "Books" },
            new Category { CategoryId = 3, Name = "Apparel" },
            new Category { CategoryId = 4, Name = "Personal Care" }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product { ProductId = 1, Name = "Laptop", Description = "Gaming Laptop", Price = 999.99m, Stock = 10, CategoryId = 1 },
            new Product { ProductId = 2, Name = "Smartphone", Description = "Latest Smartphone", Price = 699.99m, Stock = 15, CategoryId = 1 },
            new Product { ProductId = 3, Name = "Programming Book", Description = "Learn C#", Price = 49.99m, Stock = 20, CategoryId = 2 },
            new Product { ProductId = 4, Name = "T-Shirt", Description = "Cotton T-Shirt", Price = 19.99m, Stock = 50, CategoryId = 3 }
        );
    }
}