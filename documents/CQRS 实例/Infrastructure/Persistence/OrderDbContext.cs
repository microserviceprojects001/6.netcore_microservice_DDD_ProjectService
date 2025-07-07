// Infrastructure/Persistence/OrderDbContext.cs
using ECommerce.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence
{
    public class OrderDbContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }

        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.OwnsMany(o => o.Items, i =>
                {
                    i.WithOwner().HasForeignKey("OrderId");
                    i.Property<int>("Id").ValueGeneratedOnAdd();
                });
            });
        }
    }
}