// Domain/Models/Order.cs
using ECommerce.Domain.Enums;
using System.Collections.ObjectModel;
using ECommerce.Domain.Events;

namespace ECommerce.Domain.Entities
{
    // 值对象
    public class OrderItem
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid ProductId { get; private set; }
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }

        public OrderItem(Guid productId, int quantity, decimal unitPrice)
        {
            ProductId = productId;
            Quantity = quantity > 0 ? quantity : throw new ArgumentException("数量必须大于0");
            UnitPrice = unitPrice > 0 ? unitPrice : throw new ArgumentException("单价必须大于0");
        }

        // 实现值对象的相等性比较
        public override bool Equals(object? obj)
        {
            if (obj is not OrderItem other)
                return false;

            return ProductId == other.ProductId &&
                   Quantity == other.Quantity &&
                   UnitPrice == other.UnitPrice;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ProductId, Quantity, UnitPrice);
        }
    }
}
