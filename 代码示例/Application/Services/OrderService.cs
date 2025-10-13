using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Application/Services/OrderService.cs
namespace Application.Services
{
    public class OrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<OrderId> CreateOrderAsync(CreateOrderCommand command, CancellationToken cancellationToken = default)
        {
            // 创建地址值对象
            var shippingAddress = new Address(
                command.Province,
                command.City,
                command.District,
                command.Street,
                command.ZipCode
            );

            // 创建订单项
            var orderItems = command.Items.Select(item =>
                OrderItem.Create(
                    new ProductId(item.ProductId),
                    item.ProductName,
                    item.Quantity,
                    new Money(item.UnitPrice)
                )).ToList();

            // 创建订单聚合
            var order = Order.Create(
                new CustomerId(command.CustomerId),
                shippingAddress,
                orderItems
            );

            // 保存聚合
            await _orderRepository.AddAsync(order, cancellationToken);
            await _orderRepository.SaveChangesAsync(cancellationToken);

            return order.Id;
        }

        public async Task ChangeShippingAddressAsync(ChangeShippingAddressCommand command, CancellationToken cancellationToken = default)
        {
            var order = await _orderRepository.GetByIdAsync(command.OrderId, cancellationToken);
            if (order == null)
                throw new OrderNotFoundException(command.OrderId.ToString());

            // 创建新地址值对象
            var newAddress = new Address(
                command.Province,
                command.City,
                command.District,
                command.Street,
                command.ZipCode
            );

            // 调用聚合根的业务方法
            order.ChangeShippingAddress(newAddress);

            // 保存聚合（会自动发布领域事件）
            await _orderRepository.UpdateAsync(order, cancellationToken);
            await _orderRepository.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAddressPartiallyAsync(UpdateAddressPartialCommand command, CancellationToken cancellationToken = default)
        {
            var order = await _orderRepository.GetByIdAsync(command.OrderId, cancellationToken);
            if (order == null)
                throw new OrderNotFoundException(command.OrderId.ToString());

            var currentAddress = order.ShippingAddress;

            // 基于当前地址创建新地址（整体替换）
            var newAddress = currentAddress
                .WithProvince(command.Province ?? currentAddress.Province)
                .WithCity(command.City ?? currentAddress.City)
                .WithDistrict(command.District ?? currentAddress.District)
                .WithStreet(command.Street ?? currentAddress.Street)
                .WithZipCode(command.ZipCode ?? currentAddress.ZipCode);

            // 整体替换地址
            order.ChangeShippingAddress(newAddress);

            await _orderRepository.UpdateAsync(order, cancellationToken);
            await _orderRepository.SaveChangesAsync(cancellationToken);
        }
        //🚫 第三步：什么不能做 - 错误示例
        // 错误做法1：直接操作内部实体
        // ❌ 错误！绕过聚合根直接操作内部实体
        // public void AddItemToOrder(OrderId orderId, OrderItem item)
        // {
        //     var order = _orderRepository.GetById(orderId);

        //     // 直接操作内部集合 - 破坏了封装性
        //     order.OrderItems.Add(item);  // 编译错误！因为OrderItems是只读的

        //     _orderRepository.Save(order);
        // }

        // ✅ 第四步：正确做法 - 通过聚合根
        // 正确做法1：通过聚合根的方法
        public void AddItemToOrder(OrderId orderId, ProductId productId, string productName, int quantity, Money price)
        {
            var order = _orderRepository.GetById(orderId);

            // 通过聚合根的方法添加商品
            order.AddOrderItem(productId, productName, quantity, price);

            _orderRepository.Save(order);
        }
    }

    // DTOs
    public record CreateOrderCommand(
        Guid CustomerId,
        string Province,
        string City,
        string District,
        string Street,
        string ZipCode,
        List<OrderItemDto> Items
    );

    public record OrderItemDto(
        Guid ProductId,
        string ProductName,
        int Quantity,
        decimal UnitPrice
    );

    public record ChangeShippingAddressCommand(
        OrderId OrderId,
        string Province,
        string City,
        string District,
        string Street,
        string ZipCode
    );

    public record UpdateAddressPartialCommand(
        OrderId OrderId,
        string? Province,
        string? City,
        string? District,
        string? Street,
        string? ZipCode
    );
}