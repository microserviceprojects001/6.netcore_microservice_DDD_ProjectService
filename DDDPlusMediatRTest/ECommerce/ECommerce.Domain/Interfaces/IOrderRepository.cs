// Domain/Interfaces/IOrderRepository.cs
using ECommerce.Domain.Entities;
namespace ECommerce.Domain.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(Guid userId);
    }
}
