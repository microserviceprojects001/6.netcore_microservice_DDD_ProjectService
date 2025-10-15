using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// Application/Common/Interfaces/IDomainEventPublisher.cs
using Domain.Common;

namespace Application.Common.Interfaces
{
    public interface IDomainEventPublisher
    {
        Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
        Task PublishAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
    }
}