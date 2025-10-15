using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Infrastructure/Services/DomainEventPublisher.cs
using Application.Common.Interfaces;
using Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class DomainEventPublisher : IDomainEventPublisher
    {
        private readonly IMediator _mediator;
        private readonly ILogger<DomainEventPublisher> _logger;

        public DomainEventPublisher(IMediator mediator, ILogger<DomainEventPublisher> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            if (domainEvent == null) return;

            try
            {
                _logger.LogDebug("发布领域事件: {EventType}", domainEvent.GetType().Name);

                await _mediator.Publish(domainEvent, cancellationToken);

                _logger.LogDebug("领域事件发布成功: {EventType}", domainEvent.GetType().Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发布领域事件失败: {EventType}", domainEvent.GetType().Name);
                throw;
            }
        }

        public async Task PublishAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
        {
            if (domainEvents == null || !domainEvents.Any()) return;

            foreach (var domainEvent in domainEvents)
            {
                await PublishAsync(domainEvent, cancellationToken);
            }
        }
    }
}