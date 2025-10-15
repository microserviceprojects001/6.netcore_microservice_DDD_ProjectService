using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Domain/Orders/Events/OrderCreatedEvent.cs
// Domain/Orders/Events/OrderCreatedEvent.cs
using Domain.Common;

namespace Domain.Orders.Events
{
    public record OrderCreatedEvent(OrderId OrderId, CustomerId CustomerId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}