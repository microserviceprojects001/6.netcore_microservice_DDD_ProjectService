using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Domain/Orders/Events/ShippingAddressChangedEvent.cs
namespace Domain.Orders.Events
{
    public record ShippingAddressChangedEvent(
        OrderId OrderId,
        Address OldAddress,
        Address NewAddress,
        DateTime ChangedAt) : IDomainEvent;
}