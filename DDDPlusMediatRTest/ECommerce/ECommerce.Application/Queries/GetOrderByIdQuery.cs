// Application/Queries/GetOrdersByUserQuery.cs
using MediatR;
using ECommerce.API.DTOs;

public record GetOrderByIdQuery : IRequest<OrderDto>
{
    public required Guid OrderId { get; init; }
    public int PageSize { get; init; } = 10;
    public int PageNumber { get; init; } = 1;
}

