// Domain/Events/OrderCreatedEvent.cs
using MediatR;

namespace ECommerce.Domain.Events
{
    public record OrderCreatedEvent(Guid OrderId) : INotification;
}