// Handlers/Queries/GetOrderByIdHandler.cs
using ECommerce.API.DTOs;
using AutoMapper;
using MediatR;
using ECommerce.Domain.Interfaces;
using ECommerce.Domain.Exceptions;
public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetOrderByIdHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId)
            ?? throw new NotFoundException("订单不存在");

        return _mapper.Map<OrderDto>(order);
    }
}