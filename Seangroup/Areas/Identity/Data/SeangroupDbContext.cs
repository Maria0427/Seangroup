using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Seangroup.Areas.Identity.Data;
using Seangroup.Models;

namespace Seangroup.Data;

public class SeangroupDbContext : IdentityDbContext<ApplicationUser>
{
    public SeangroupDbContext(DbContextOptions<SeangroupDbContext> options)
         : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<ProductDetail> ProductDetails { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Важно для Identity

        // Настройка связи Order -> User
        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders) // Используем навигационное свойство из ApplicationUser
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict); // Или .Cascade, если нужно удалять заказы при удалении пользователя

        // Остальные настройки
        modelBuilder.Entity<Product>().ToTable("Products");
        modelBuilder.Entity<ProductDetail>().ToTable("ProductDetails");
        modelBuilder.Entity<CartItem>().ToTable("CartItems");

        modelBuilder.Entity<CartItem>()
            .Property(c => c.Price)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<CartItem>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
