using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Domain/Orders/Events/ShippingAddressChangedEvent.cs
// Domain/Orders/Events/ShippingAddressChangedEvent.cs
using Domain.Common;

namespace Domain.Orders.Events
{
    public record ShippingAddressChangedEvent(
        OrderId OrderId,
        Address OldAddress,
        Address NewAddress) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}