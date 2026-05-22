using Microsoft.EntityFrameworkCore;
using System.Linq;
using UrbanGadgets.Models;
using UrbanGadgetsMS.Models;

namespace UrbanGadgets.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Sale> Sales { get; set; }

        public DbSet<SaleItem> SaleItems { get; set; }

        public DbSet<Expense> Expenses { get; set; }

        public DbSet<RestockReport> RestockReports { get; set; }

        public DbSet<RestockItem> RestockItems { get; set; }

        public DbSet<AppSetting> AppSettings { get; set; }

        public DbSet<StockMovement> StockMovements { get; set; }

        public DbSet<Role> Roles { get; set; }

        public DbSet<Target> Targets { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Product
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("numeric(18,2)");

            // Sale
            modelBuilder.Entity<Sale>()
                .Property(s => s.TotalAmount)
                .HasColumnType("numeric(18,2)");

            modelBuilder.Entity<Sale>()
                .Property(s => s.Discount)
                .HasColumnType("numeric(18,2)");

            // SaleItem
            modelBuilder.Entity<SaleItem>()
                .Property(s => s.UnitPrice)
                .HasColumnType("numeric(18,2)");

            modelBuilder.Entity<SaleItem>()
                .Property(s => s.TotalPrice)
                .HasColumnType("numeric(18,2)");

            // Expense
            modelBuilder.Entity<Expense>()
                .Property(e => e.Amount)
                .HasColumnType("numeric(18,2)");

            modelBuilder.Entity<Expense>()
                .Property(e => e.Category)
                .HasConversion<string>();

            // App Settings
            modelBuilder.Entity<AppSetting>()
                .Property(a => a.MonthlyExpenseLimit)
                .HasColumnType("numeric(18,2)");

            // Restock Item
            modelBuilder.Entity<RestockItem>()
                .Property(r => r.BuyingPrice)
                .HasColumnType("numeric(18,2)");

            modelBuilder.Entity<RestockItem>()
                .Property(r => r.Price)
                .HasColumnType("numeric(18,2)");

            // Restock Report
            modelBuilder.Entity<RestockReport>()
                .Property(r => r.TotalAmount)
                .HasColumnType("numeric(18,2)");

            // Configure all DateTime properties as UTC
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var dateTimeProperties = entityType.ClrType
                    .GetProperties()
                    .Where(p => p.PropertyType == typeof(DateTime) ||
                                p.PropertyType == typeof(DateTime?));

                foreach (var property in dateTimeProperties)
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property(property.Name)
                        .HasColumnType("timestamp with time zone");
                }
            }
        }
    }
}