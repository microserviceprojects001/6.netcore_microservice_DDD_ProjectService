// Application/Handlers/Events/OrderCreatedEventHandler.cs
using MediatR;
using ECommerce.Domain.Events;
using ECommerce.Domain.Interfaces;

namespace ECommerce.Application.Handlers.Events
{
    public class OrderCreatedEventHandler : INotificationHandler<OrderCreatedEvent>
    {
        private readonly IEmailService _emailService;

        public OrderCreatedEventHandler(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
        {
            // 模拟发邮件通知
            await _emailService.SendOrderConfirmationAsync(notification.OrderId);

            // 你还可以加入其他处理逻辑，例如写日志
            Console.WriteLine($"收到订单创建事件，订单ID: {notification.OrderId}");
        }
    }
}
