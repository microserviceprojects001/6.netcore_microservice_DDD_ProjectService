using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace 代码示例
{
    // 使用示例
    public class OrderController
    {
        private readonly OrderService _orderService;

        public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
        {
            var command = new CreateOrderCommand(
                CustomerId.From(request.CustomerId),
                new Address(request.Province, request.City, request.District, request.Street, request.ZipCode),
                request.Items.Select(item => OrderItem.Create(
                    ProductId.From(item.ProductId),
                    item.ProductName,
                    item.Quantity,
                    new Money(item.UnitPrice)
                )).ToList()
            );

            var orderId = await _orderService.CreateOrderAsync(command);

            // 此时：
            // 1. 订单已保存到数据库
            // 2. OrderCreatedEvent 已自动发布
            // 3. OrderCreatedEventHandler 会自动处理该事件

            return Ok(new { OrderId = orderId });
        }

        // 2. Application Service 处理 Command → 返回 DTO
        public async Task<OrderDto> GetOrderDetailsAsync(OrderId orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            return OrderDto.FromOrder(order); // 领域对象 → 查询DTO
        }
    }

    // Controller 输入
    public class CreateOrderRequest
    {
        public Guid CustomerId { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Street { get; set; }
        public string ZipCode { get; set; }
        public List<OrderItemRequest> Items { get; set; }
    }

    public class OrderItemRequest
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    // Controller 输出
    public class CreateOrderResponse
    {
        public OrderId OrderId { get; set; }
    }
}
