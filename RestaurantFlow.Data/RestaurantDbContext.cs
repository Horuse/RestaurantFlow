using Microsoft.EntityFrameworkCore;
using RestaurantFlow.Data.Entities;
using RestaurantFlow.Shared.Enums;

namespace RestaurantFlow.Data;

public class RestaurantDbContext : DbContext
{
    public DbSet<Staff> Staff { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<Ingredient> Ingredients { get; set; }
    public DbSet<MenuItemIngredient> MenuItemIngredients { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<InventoryLog> InventoryLogs { get; set; }
    
    public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options) 
        : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // MenuItemIngredient many-to-many
        modelBuilder.Entity<MenuItemIngredient>()
            .HasKey(mi => new { mi.MenuItemId, mi.IngredientId });
            
        modelBuilder.Entity<MenuItemIngredient>()
            .HasOne(mi => mi.MenuItem)
            .WithMany(m => m.MenuItemIngredients)
            .HasForeignKey(mi => mi.MenuItemId);
            
        modelBuilder.Entity<MenuItemIngredient>()
            .HasOne(mi => mi.Ingredient)
            .WithMany(i => i.MenuItemIngredients)
            .HasForeignKey(mi => mi.IngredientId);
        
        // Decimal precision для цін та інвентарю
        modelBuilder.Entity<MenuItem>()
            .Property(m => m.Price)
            .HasPrecision(18, 2);
            
        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasPrecision(18, 2);
            
        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.Price)
            .HasPrecision(18, 2);
            
        modelBuilder.Entity<Ingredient>()
            .Property(i => i.CurrentStock)
            .HasPrecision(18, 3);
            
        modelBuilder.Entity<Ingredient>()
            .Property(i => i.MinimumStock)
            .HasPrecision(18, 3);
            
        modelBuilder.Entity<MenuItemIngredient>()
            .Property(mi => mi.Quantity)
            .HasPrecision(18, 3);
            
        modelBuilder.Entity<InventoryLog>()
            .Property(il => il.QuantityChanged)
            .HasPrecision(18, 3);
            
        modelBuilder.Entity<InventoryLog>()
            .Property(il => il.StockAfter)
            .HasPrecision(18, 3);
        
        // String lengths
        modelBuilder.Entity<Order>()
            .Property(o => o.OrderNumber)
            .HasMaxLength(20)
            .IsRequired();
            
        modelBuilder.Entity<Staff>()
            .Property(s => s.Name)
            .HasMaxLength(100)
            .IsRequired();
            
        modelBuilder.Entity<Category>()
            .Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();
            
        modelBuilder.Entity<MenuItem>()
            .Property(m => m.Name)
            .HasMaxLength(200)
            .IsRequired();
            
        // Ігноруємо обчислювану властивість
        modelBuilder.Entity<MenuItem>()
            .Ignore(m => m.IsCurrentlyAvailable);
            
        modelBuilder.Entity<Ingredient>()
            .Property(i => i.Name)
            .HasMaxLength(100)
            .IsRequired();
            
        modelBuilder.Entity<Ingredient>()
            .Property(i => i.Unit)
            .HasMaxLength(20)
            .IsRequired();
        
        // Indexes для оптимізації
        modelBuilder.Entity<Order>()
            .HasIndex(o => o.OrderNumber)
            .IsUnique();
            
        modelBuilder.Entity<Order>()
            .HasIndex(o => o.Status);
            
        modelBuilder.Entity<Order>()
            .HasIndex(o => new { o.OrderType, o.TableNumber });
            
        modelBuilder.Entity<OrderItem>()
            .HasIndex(oi => oi.Status);
            
        modelBuilder.Entity<MenuItem>()
            .HasIndex(m => m.IsAvailable);
            
        modelBuilder.Entity<Staff>()
            .HasIndex(s => new { s.Role, s.IsActive });
            
        // Table number validation (1-10)
        modelBuilder.Entity<Order>()
            .ToTable(o => o.HasCheckConstraint("CK_Order_TableNumber", 
                "[TableNumber] IS NULL OR ([TableNumber] >= 1 AND [TableNumber] <= 10)"));
    }
}