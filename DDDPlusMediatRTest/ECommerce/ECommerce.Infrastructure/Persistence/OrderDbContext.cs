// Infrastructure/Persistence/OrderDbContext.cs

using Microsoft.EntityFrameworkCore;
using ECommerce.Domain.Entities;
namespace ECommerce.Infrastructure.Persistence
{
    public class OrderDbContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }

        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
            .HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey("OrderId"); // 👈 外键字段

            modelBuilder.Entity<OrderItem>()
                .HasKey(oi => oi.Id); // 👈 主键 Guid

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Id)
                .ValueGeneratedNever(); // 👈 Guid 不自增
        }
    }
}