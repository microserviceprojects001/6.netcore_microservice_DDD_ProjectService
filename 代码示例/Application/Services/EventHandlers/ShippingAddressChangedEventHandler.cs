using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Application/Services/EventHandlers/ShippingAddressChangedEventHandler.cs
using Domain.Orders.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Services.EventHandlers
{
    public class ShippingAddressChangedEventHandler : INotificationHandler<ShippingAddressChangedEvent>
    {
        private readonly ILogger<ShippingAddressChangedEventHandler> _logger;

        public ShippingAddressChangedEventHandler(ILogger<ShippingAddressChangedEventHandler> logger)
        {
            _logger = logger;
        }

        public async Task Handle(ShippingAddressChangedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "处理配送地址变更事件 - 订单ID: {OrderId}, 旧地址: {OldAddress}, 新地址: {NewAddress}",
                notification.OrderId, notification.OldAddress, notification.NewAddress);

            // 这里可以添加业务逻辑，比如：
            // - 通知物流系统
            // - 更新配送计划
            // - 发送地址变更确认邮件

            await Task.CompletedTask;
        }
    }
}