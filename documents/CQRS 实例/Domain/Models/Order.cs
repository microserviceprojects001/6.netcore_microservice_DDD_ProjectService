// Domain/Models/Order.cs
using ECommerce.Domain.Enums;
using System.Collections.ObjectModel;
using ECommerce.Domain.Events;

namespace ECommerce.Domain.Models
{
    public class Order
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public OrderStatus Status { get; private set; } = OrderStatus.Pending;  // 修改为枚举类型
        public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();
        private readonly List<OrderItem> _items = new();

        // 私有构造函数（EF Core 需要）
        private Order() { }

        // 工厂方法
        public static Order Create(Guid userId, List<OrderItem> items)
        {
            if (items == null || items.Count == 0)
                throw new ArgumentException("订单项不能为空");

            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
            };
            order._items.AddRange(items);
            order.AddDomainEvent(new OrderCreatedEvent(order.Id));
            return order;
        }

        // 领域方法
        public void UpdateStatus(OrderStatus status)  // 修改为枚举参数
        {
            Status = status;
        }

        // 领域事件相关
        private readonly List<INotification> _domainEvents = new();
        public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();
        public void AddDomainEvent(INotification @event) => _domainEvents.Add(@event);
        public void ClearDomainEvents() => _domainEvents.Clear();
    }

    // 值对象
    public class OrderItem
    {
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
