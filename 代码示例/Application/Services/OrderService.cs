using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Orders;
using Domain.Products;
using Domain.Customers;
using Domain.Shared;

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
            // 参数验证
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (command.Items == null || !command.Items.Any())
                throw new ArgumentException("订单项不能为空", nameof(command.Items));

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
                    ProductId.From(item.ProductId),
                    item.ProductName,
                    item.Quantity,
                    new Money(item.UnitPrice)
                )).ToList();

            // 创建订单聚合
            var order = Order.Create(
                CustomerId.From(command.CustomerId),
                shippingAddress,
                orderItems
            );

            // 保存聚合
            await _orderRepository.AddAsync(order, cancellationToken);
            await _orderRepository.SaveChangesAsync(cancellationToken);

            return order.Id;
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
        public async Task ChangeShippingAddressAsync(ChangeShippingAddressCommand command, CancellationToken cancellationToken = default)
        {
            // 参数验证
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var order = await _orderRepository.GetByIdAsync(command.OrderId, cancellationToken);
            if (order == null)
                throw new OrderNotFoundException($"订单不存在: {command.OrderId}");

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
            // 注意：不需要显式调用 UpdateAsync，EF Core 会自动跟踪变更
            await _orderRepository.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAddressPartiallyAsync(UpdateAddressPartialCommand command, CancellationToken cancellationToken = default)
        {
            // 参数验证
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var order = await _orderRepository.GetByIdAsync(command.OrderId, cancellationToken);
            if (order == null)
                throw new OrderNotFoundException($"订单不存在: {command.OrderId}");

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

            // 保存聚合（会自动发布领域事件）
            await _orderRepository.SaveChangesAsync(cancellationToken);
        }

        public async Task AddOrderItemAsync(AddOrderItemCommand command, CancellationToken cancellationToken = default)
        {
            // 参数验证
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var order = await _orderRepository.GetByIdAsync(command.OrderId, cancellationToken);
            if (order == null)
                throw new OrderNotFoundException($"订单不存在: {command.OrderId}");

            // 通过聚合根的方法添加商品
            order.AddOrderItem(
                ProductId.From(command.ProductId),
                command.ProductName,
                command.Quantity,
                new Money(command.UnitPrice)
            );

            // 保存变更
            await _orderRepository.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveOrderItemAsync(RemoveOrderItemCommand command, CancellationToken cancellationToken = default)
        {
            // 参数验证
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var order = await _orderRepository.GetByIdAsync(command.OrderId, cancellationToken);
            if (order == null)
                throw new OrderNotFoundException($"订单不存在: {command.OrderId}");

            // 通过聚合根的方法移除商品
            order.RemoveOrderItem(ProductId.From(command.ProductId));

            // 保存变更
            await _orderRepository.SaveChangesAsync(cancellationToken);
        }

        public async Task PayOrderAsync(PayOrderCommand command, CancellationToken cancellationToken = default)
        {
            // 参数验证
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var order = await _orderRepository.GetByIdAsync(command.OrderId, cancellationToken);
            if (order == null)
                throw new OrderNotFoundException($"订单不存在: {command.OrderId}");

            // 通过聚合根的方法支付订单
            order.Pay();

            // 保存变更
            await _orderRepository.SaveChangesAsync(cancellationToken);
        }

        public async Task<OrderDto> GetOrderDetailsAsync(OrderId orderId, CancellationToken cancellationToken = default)
        {
            var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
            if (order == null)
                throw new OrderNotFoundException($"订单不存在: {orderId}");

            return OrderDto.FromOrder(order);
        }

        public async Task<bool> OrderExistsAsync(OrderId orderId, CancellationToken cancellationToken = default)
        {
            var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
            return order != null;
        }
    }

    // DTOs
    // 修改 CreateOrderCommand 定义
    public record CreateOrderCommand(
        CustomerId CustomerId,                // 使用 CustomerId 值对象
        Address ShippingAddress,              // 使用 Address 值对象
        List<OrderItem> Items                 // 使用 OrderItem 实体
    )
    {
        public void Validate()
        {
            if (CustomerId == null)
                throw new ArgumentException("客户ID不能为空");

            if (ShippingAddress == null)
                throw new ArgumentException("配送地址不能为空");

            if (Items == null || !Items.Any())
                throw new ArgumentException("订单项不能为空");
        }
    }

    public record OrderItemDto(
        Guid ProductId,
        string ProductName,
        int Quantity,
        decimal UnitPrice
    )
    {
        public void Validate()
        {
            if (ProductId == Guid.Empty)
                throw new ArgumentException("产品ID不能为空");

            if (string.IsNullOrWhiteSpace(ProductName))
                throw new ArgumentException("产品名称不能为空");

            if (Quantity <= 0)
                throw new ArgumentException("数量必须大于0");

            if (UnitPrice <= 0)
                throw new ArgumentException("单价必须大于0");
        }
    }

    public record ChangeShippingAddressCommand(
        OrderId OrderId,
        string Province,
        string City,
        string District,
        string Street,
        string ZipCode
    )
    {
        public void Validate()
        {
            if (OrderId == null)
                throw new ArgumentException("订单ID不能为空");

            if (string.IsNullOrWhiteSpace(Province))
                throw new ArgumentException("省份不能为空");

            if (string.IsNullOrWhiteSpace(City))
                throw new ArgumentException("城市不能为空");

            if (string.IsNullOrWhiteSpace(Street))
                throw new ArgumentException("街道不能为空");
        }
    }

    public record UpdateAddressPartialCommand(
        OrderId OrderId,
        string? Province,
        string? City,
        string? District,
        string? Street,
        string? ZipCode
    )
    {
        public void Validate()
        {
            if (OrderId == null)
                throw new ArgumentException("订单ID不能为空");

            // 至少提供一个要更新的字段
            if (Province == null && City == null && District == null && Street == null && ZipCode == null)
                throw new ArgumentException("至少需要提供一个地址字段进行更新");
        }
    }

    public record AddOrderItemCommand(
        OrderId OrderId,
        Guid ProductId,
        string ProductName,
        int Quantity,
        decimal UnitPrice
    )
    {
        public void Validate()
        {
            if (OrderId == null)
                throw new ArgumentException("订单ID不能为空");

            if (ProductId == Guid.Empty)
                throw new ArgumentException("产品ID不能为空");

            if (string.IsNullOrWhiteSpace(ProductName))
                throw new ArgumentException("产品名称不能为空");

            if (Quantity <= 0)
                throw new ArgumentException("数量必须大于0");

            if (UnitPrice <= 0)
                throw new ArgumentException("单价必须大于0");
        }
    }

    public record RemoveOrderItemCommand(
        OrderId OrderId,
        Guid ProductId
    )
    {
        public void Validate()
        {
            if (OrderId == null)
                throw new ArgumentException("订单ID不能为空");

            if (ProductId == Guid.Empty)
                throw new ArgumentException("产品ID不能为空");
        }
    }

    public record PayOrderCommand(
        OrderId OrderId
    )
    {
        public void Validate()
        {
            if (OrderId == null)
                throw new ArgumentException("订单ID不能为空");
        }
    }

    // 查询DTO
    public class OrderDto
    {
        public OrderId OrderId { get; set; }
        public CustomerId CustomerId { get; set; }
        public OrderStatus Status { get; set; }
        public Address ShippingAddress { get; set; }
        public Money TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = new();

        public static OrderDto FromOrder(Order order)
        {
            return new OrderDto
            {
                OrderId = order.Id,
                CustomerId = order.CustomerId,
                Status = order.Status,
                ShippingAddress = order.ShippingAddress,
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                OrderItems = order.OrderItems.Select(item => new OrderItemDto
                {
                    ProductId = item.ProductId.Value,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice.Amount
                }).ToList()
            };
        }
    }
}

// 异常定义
public class OrderNotFoundException : Exception
{
    public OrderNotFoundException(string message) : base(message) { }
    public OrderNotFoundException(OrderId orderId) : base($"订单不存在: {orderId}") { }
}

public class InvalidOrderOperationException : Exception
{
    public InvalidOrderOperationException(string message) : base(message) { }
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
