// Handlers/Commands/UpdateOrderStatusHandler.cs

using MediatR;
using Microsoft.EntityFrameworkCore;
using ECommerce.Domain.Interfaces;

namespace ECommerce.Handlers.Commands
{
    public class UpdateOrderStatusHandler : IRequestHandler<UpdateOrderStatusCommand, Unit>
    {
        private readonly IOrderRepository _orderRepository;

        public UpdateOrderStatusHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<Unit> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId);
            if (order == null)
                throw new ArgumentException("订单未找到");

            order.UpdateStatus(request.Status);
            await _orderRepository.UpdateAsync(order);

            return Unit.Value;
        }
    }
}