// Application/Handlers/Queries/GetOrdersByUserHandler.cs
using ECommerce.Application.Queries;
using ECommerce.API.DTOs;
using ECommerce.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Handlers.Queries
{
    public class GetOrdersByUserHandler : IRequestHandler<GetOrdersByUserQuery, IEnumerable<OrderDto>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        public GetOrdersByUserHandler(IOrderRepository orderRepository, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<OrderDto>> Handle(GetOrdersByUserQuery request, CancellationToken ct)
        {
            var orders = await _orderRepository.GetByUserIdAsync(
                request.UserId,
                request.PageNumber,
                request.PageSize,
                ct);

            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }
    }
}