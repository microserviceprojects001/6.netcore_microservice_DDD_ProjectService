// Domain/Interfaces/IEmailService.cs
namespace ECommerce.Domain.Interfaces
{
    public interface IEmailService
    {
        Task SendOrderConfirmationAsync(Guid orderId);
    }
}
