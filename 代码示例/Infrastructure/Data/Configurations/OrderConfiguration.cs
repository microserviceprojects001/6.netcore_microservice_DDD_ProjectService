using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Infrastructure/Data/Configurations/OrderConfiguration.cs
namespace Infrastructure.Data.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            // 配置主键
            builder.HasKey(o => o.Id);

            // 配置强类型ID的转换
            builder.Property(o => o.Id)
                .HasConversion(
                    id => id.Value,
                    value => OrderId.From(value))
                .ValueGeneratedNever();

            // 配置CustomerId的转换
            builder.Property(o => o.CustomerId)
                .HasConversion(
                    id => id.Value,
                    value => CustomerId.From(value));

            // 配置值对象 - 地址
            builder.OwnsOne(o => o.ShippingAddress, address =>
            {
                address.Property(a => a.Province).HasColumnName("ShippingProvince");
                address.Property(a => a.City).HasColumnName("ShippingCity");
                address.Property(a => a.District).HasColumnName("ShippingDistrict");
                address.Property(a => a.Street).HasColumnName("ShippingStreet");
                address.Property(a => a.ZipCode).HasColumnName("ShippingZipCode");
            });

            // 配置内部实体集合
            builder.OwnsMany(o => o.OrderItems, item =>
            {
                item.WithOwner().HasForeignKey("OrderId");
                item.ToTable("OrderItems");

                item.HasKey(i => i.Id);

                item.Property(i => i.Id)
                    .HasConversion(
                        id => id.Value,
                        value => OrderItemId.From(value))
                    .ValueGeneratedNever();

                item.Property(i => i.ProductId)
                    .HasConversion(
                        id => id.Value,
                        value => ProductId.From(value));

                item.Property(i => i.UnitPrice)
                    .HasConversion(
                        money => money.Amount,
                        amount => new Money(amount))
                    .HasColumnType("decimal(10,2)");

                item.Property(i => i.SubTotal)
                    .HasConversion(
                        money => money.Amount,
                        amount => new Money(amount))
                    .HasColumnType("decimal(10,2)");
            });

            // 忽略领域事件集合（不持久化）
            builder.Ignore(o => o.DomainEvents);
        }
    }
}