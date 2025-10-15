using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Application/Services/EventHandlers/OrderCreatedEventHandler.cs
using Domain.Orders.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Services.EventHandlers
{
    public class OrderCreatedEventHandler : INotificationHandler<OrderCreatedEvent>
    {
        private readonly ILogger<OrderCreatedEventHandler> _logger;

        public OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger)
        {
            _logger = logger;
        }

        public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "处理订单创建事件 - 订单ID: {OrderId}, 客户ID: {CustomerId}",
                notification.OrderId, notification.CustomerId);

            // 这里可以添加业务逻辑，比如：
            // - 发送邮件通知
            // - 更新库存
            // - 触发后续工作流

            await Task.CompletedTask;
        }
    }
}