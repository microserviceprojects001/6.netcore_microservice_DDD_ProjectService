// Infrastructure/Services/EmailService.cs
using ECommerce.Domain.Interfaces;

namespace ECommerce.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        public async Task SendOrderConfirmationAsync(Guid orderId)
        {
            // 模拟发送邮件
            Console.WriteLine($"已发送订单确认邮件，订单ID: {orderId}");
            await Task.CompletedTask;
        }
    }
}