using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// Infrastructure/Repositories/OrderRepository.cs
// Infrastructure/Repositories/OrderRepository.cs
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IDomainEventPublisher _domainEventPublisher;
        private readonly ILogger<OrderRepository> _logger;

        public OrderRepository(
            ApplicationDbContext context,
            IDomainEventPublisher domainEventPublisher,
            ILogger<OrderRepository> logger)
        {
            _context = context;
            _domainEventPublisher = domainEventPublisher;
            _logger = logger;
        }

        public async Task<Order?> GetByIdAsync(OrderId id, CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        }

        public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
        {
            await _context.Orders.AddAsync(order, cancellationToken);
        }

        public async Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
        {
            // EF Core会自动跟踪变更
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // 1. 收集领域事件
            var domainEvents = _context.ChangeTracker
                .Entries<Entity>()
                .SelectMany(x => x.Entity.DomainEvents)
                .ToList();

            _logger.LogDebug("收集到 {EventCount} 个领域事件", domainEvents.Count);

            // 2. 保存数据变更
            await _context.SaveChangesAsync(cancellationToken);

            // 3. 发布领域事件
            if (domainEvents.Any())
            {
                await _domainEventPublisher.PublishAsync(domainEvents, cancellationToken);
            }

            // 4. 清空领域事件
            ClearDomainEvents();
        }

        private void ClearDomainEvents()
        {
            foreach (var entry in _context.ChangeTracker.Entries<Entity>())
            {
                entry.Entity.ClearDomainEvents();
            }
        }
    }

    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(OrderId id, CancellationToken cancellationToken = default);
        Task AddAsync(Order order, CancellationToken cancellationToken = default);
        Task UpdateAsync(Order order, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
