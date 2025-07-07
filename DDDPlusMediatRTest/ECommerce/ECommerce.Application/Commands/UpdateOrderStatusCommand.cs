// Commands/UpdateOrderStatusCommand.cs
using MediatR;
using ECommerce.Domain.Enums;
public record UpdateOrderStatusCommand : IRequest<Unit>
{
    public required Guid OrderId { get; set; }
    public required OrderStatus Status { get; set; }  // 修改为枚举类型
}