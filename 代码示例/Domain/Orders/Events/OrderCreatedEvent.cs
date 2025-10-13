using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Domain/Orders/Events/OrderCreatedEvent.cs
namespace Domain.Orders.Events
{
    public record OrderCreatedEvent(OrderId OrderId, CustomerId CustomerId, Money TotalAmount) : IDomainEvent;
}