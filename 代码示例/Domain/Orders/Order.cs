using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Common;
using Domain.Orders.Events;

// Domain/Orders/Order.cs
namespace Domain.Orders
{
    public class Order : Entity, IAggregateRoot
    {
        // 唯一标识
        public OrderId Id { get; private set; }
        public CustomerId CustomerId { get; private set; }
        public OrderStatus Status { get; private set; }

        // 值对象
        public Address ShippingAddress { get; private set; }
        public Money TotalAmount { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        // 内部实体集合
        private readonly List<OrderItem> _orderItems = new List<OrderItem>();
        public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

        // ✅ 移除了重复的领域事件定义！
        // Entity基类已经提供了：
        // - _domainEvents 字段
        // - DomainEvents 属性
        // - AddDomainEvent() 方法
        // - RemoveDomainEvent() 方法  
        // - ClearDomainEvents() 方法

        // EF Core需要的私有构造函数
        private Order() { }

        // 工厂方法 - 创建新订单
        public static Order Create(CustomerId customerId, Address shippingAddress, List<OrderItem> items)
        {
            var order = new Order
            {
                Id = OrderId.New(),
                CustomerId = customerId,
                ShippingAddress = shippingAddress,
                Status = OrderStatus.Created
                ,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            order._orderItems.AddRange(items);
            order.TotalAmount = CalculateTotalAmount(items);

            // 业务规则验证
            order.ValidateOrder();

            // ✅ 使用基类的 AddDomainEvent 方法
            order.AddDomainEvent(new OrderCreatedEvent(order.Id, order.CustomerId, order.TotalAmount));

            return order;
        }

        // 业务方法 - 添加订单项
        public void AddOrderItem(ProductId productId, string productName, int quantity, Money price)
        {
            if (Status != OrderStatus.Created)
            {
                throw new InvalidOperationException("只能向待支付订单添加商品");
            }

            var existingItem = _orderItems.FirstOrDefault(item => item.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.IncreaseQuantity(quantity);
            }
            else
            {
                var newItem = OrderItem.Create(productId, productName, quantity, price);
                _orderItems.Add(newItem);
            }

            RecalculateTotalAmount();
            UpdatedAt = DateTime.UtcNow;
        }

        // 核心业务方法：修改配送地址
        public void ChangeShippingAddress(Address newAddress)
        {
            if (Status == OrderStatus.Shipped || Status == OrderStatus.Delivered)
            {
                throw new InvalidOperationException("已发货的订单不能修改配送地址");
            }

            if (ShippingAddress.Equals(newAddress))
            {
                throw new InvalidOperationException("新地址与当前地址相同");
            }

            var oldAddress = ShippingAddress;

            // 整体替换值对象
            ShippingAddress = newAddress;
            UpdatedAt = DateTime.UtcNow;

            // ✅ 使用基类的 AddDomainEvent 方法
            AddDomainEvent(new ShippingAddressChangedEvent(Id, oldAddress, newAddress, DateTime.UtcNow));
        }

        // 业务方法：移除订单项
        public void RemoveOrderItem(ProductId productId)
        {
            if (Status != OrderStatus.Created)
            {
                throw new InvalidOperationException("只能从待支付订单移除商品");
            }

            var removed = _orderItems.RemoveAll(item => item.ProductId == productId);
            if (removed > 0)
            {
                RecalculateTotalAmount();
                UpdatedAt = DateTime.UtcNow;
            }
        }

        // 业务方法：支付订单
        public void Pay()
        {
            if (Status != OrderStatus.Created)
            {
                throw new InvalidOperationException("只能支付待支付订单");
            }

            Status = OrderStatus.Paid;
            UpdatedAt = DateTime.UtcNow;

            // ✅ 使用基类的 AddDomainEvent 方法
            AddDomainEvent(new OrderPaidEvent(Id, TotalAmount));
        }

        // 业务方法：发货
        public void Ship()
        {
            if (Status != OrderStatus.Paid)
            {
                throw new InvalidOperationException("只能对已支付订单进行发货");
            }

            Status = OrderStatus.Shipped;
            UpdatedAt = DateTime.UtcNow;
        }

        // 业务查询方法
        public bool CanModify()
        {
            return Status == OrderStatus.Created;
        }

        public bool HasProduct(ProductId productId)
        {
            return _orderItems.Any(item => item.ProductId == productId);
        }

        public int GetTotalItemCount()
        {
            return _orderItems.Sum(item => item.Quantity);
        }

        // 内部辅助方法
        private static Money CalculateTotalAmount(List<OrderItem> items)
        {
            return items.Aggregate(Money.Zero, (total, item) => total + item.SubTotal);
        }

        private void RecalculateTotalAmount()
        {
            TotalAmount = CalculateTotalAmount(_orderItems);
        }

        private void ValidateOrder()
        {
            if (!_orderItems.Any())
            {
                throw new InvalidOperationException("订单不能没有商品");
            }
            if (ShippingAddress == null)
            {
                throw new InvalidOperationException("订单必须指定配送地址");
            }
        }
    }

    // 聚合根标记接口
    public interface IAggregateRoot { }
}