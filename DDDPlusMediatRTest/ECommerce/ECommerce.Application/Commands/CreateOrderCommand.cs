// Commands/CreateOrderCommand.cs
using MediatR;
using System;
using ECommerce.API.DTOs;
public record CreateOrderCommand : IRequest<Guid>
{
    public required Guid UserId { get; init; }
    public required List<OrderItemDto> Items { get; init; }
}

