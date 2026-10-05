// ============================================================
// FILE: Data/AppDbContext.cs
// This is the EF Core "bridge" between your C# code and SQL Server.
// Think of it as your database manager class.
// When you run Scaffold-DbContext, this file is auto-generated.
// ============================================================

using BurgerShopPOS.Models;
using Microsoft.EntityFrameworkCore;

namespace BurgerShopPOS.Data
{
    public class AppDbContext : DbContext
    {
        // Constructor: ASP.NET passes database settings in automatically
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Each DbSet = one table in SQL Server
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        // Configure table relationships and column rules here
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User table settings
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserId);
                entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.DateCreated).HasDefaultValueSql("GETDATE()");
            });

            // Product table settings
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.ProductId);
                entity.Property(p => p.ItemName).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Category).IsRequired().HasMaxLength(50);
                entity.Property(p => p.Price).HasColumnType("decimal(10,2)");
            });

            // Order table — linked to User (Customer)
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.OrderId);
                entity.Property(o => o.TotalAmount).HasColumnType("decimal(10,2)");
                entity.HasOne(o => o.Customer)
                      .WithMany(u => u.Orders)
                      .HasForeignKey(o => o.CustomerId);
            });

            // OrderDetail table — linked to both Order and Product
            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasKey(od => od.OrderDetailId);
                entity.Property(od => od.UnitPrice).HasColumnType("decimal(10,2)");
                entity.Property(od => od.SubTotal).HasColumnType("decimal(10,2)");

                entity.HasOne(od => od.Order)
                      .WithMany(o => o.OrderDetails)
                      .HasForeignKey(od => od.OrderId);

                entity.HasOne(od => od.Product)
                      .WithMany(p => p.OrderDetails)
                      .HasForeignKey(od => od.ProductId);
            });

            // ========== SEED DATA ==========
            // Seed users: admin (Role 2 = SuperAdmin), customer1 (Role 0 = Customer)
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    Username = "admin",
                    PasswordHash = "admin123",  // In production, use BCrypt!
                    FullName = "Administrator",
                    Role = 2,  // SuperAdmin
                    DateCreated = DateTime.Now
                },
                new User
                {
                    UserId = 2,
                    Username = "customer1",
                    PasswordHash = "customer123",  // In production, use BCrypt!
                    FullName = "Ahmed Khan",
                    Role = 0,  // Customer
                    DateCreated = DateTime.Now
                },
                new User
                {
                    UserId = 3,
                    Username = "customer2",
                    PasswordHash = "customer123",
                    FullName = "Fatima Ali",
                    Role = 0,  // Customer
                    DateCreated = DateTime.Now
                }
            );

            // Seed products in Pakistani Rupees (Rs)
            // 1 USD ≈ 278 PKR (approximate)
            modelBuilder.Entity<Product>().HasData(
                // Burgers
                new Product
                {
                    ProductId = 1,
                    ItemName = "Classic Cheeseburger",
                    Category = "Burgers",
                    Price = 499m,  // Rs 499
                    StockQuantity = 50,
                    ExpiryDate = DateTime.Now.AddDays(30)
                },
                new Product
                {
                    ProductId = 2,
                    ItemName = "Double Burger",
                    Category = "Burgers",
                    Price = 749m,  // Rs 749
                    StockQuantity = 35,
                    ExpiryDate = DateTime.Now.AddDays(30)
                },
                new Product
                {
                    ProductId = 3,
                    ItemName = "Spicy Burger",
                    Category = "Burgers",
                    Price = 599m,  // Rs 599
                    StockQuantity = 40,
                    ExpiryDate = DateTime.Now.AddDays(30)
                },
                // Sides
                new Product
                {
                    ProductId = 4,
                    ItemName = "French Fries",
                    Category = "Sides",
                    Price = 249m,  // Rs 249
                    StockQuantity = 100,
                    ExpiryDate = DateTime.Now.AddDays(7)
                },
                new Product
                {
                    ProductId = 5,
                    ItemName = "Onion Rings",
                    Category = "Sides",
                    Price = 329m,  // Rs 329
                    StockQuantity = 60,
                    ExpiryDate = DateTime.Now.AddDays(7)
                },
                new Product
                {
                    ProductId = 6,
                    ItemName = "Coleslaw",
                    Category = "Sides",
                    Price = 199m,  // Rs 199
                    StockQuantity = 30,
                    ExpiryDate = DateTime.Now.AddDays(3)
                },
                // Drinks
                new Product
                {
                    ProductId = 7,
                    ItemName = "Soft Drink (330ml)",
                    Category = "Drinks",
                    Price = 149m,  // Rs 149
                    StockQuantity = 200,
                    ExpiryDate = DateTime.Now.AddDays(90)
                },
                new Product
                {
                    ProductId = 8,
                    ItemName = "Iced Tea (500ml)",
                    Category = "Drinks",
                    Price = 199m,  // Rs 199
                    StockQuantity = 80,
                    ExpiryDate = DateTime.Now.AddDays(60)
                },
                new Product
                {
                    ProductId = 9,
                    ItemName = "Milkshake",
                    Category = "Drinks",
                    Price = 349m,  // Rs 349
                    StockQuantity = 40,
                    ExpiryDate = DateTime.Now.AddDays(1)
                }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}
