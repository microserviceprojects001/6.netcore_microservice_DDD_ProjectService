// Commands/UpdateOrderStatusCommand.cs
using MediatR;
public record UpdateOrderStatusCommand : IRequest<Unit>
{
    public required Guid OrderId { get; init; }
    public required OrderStatus Status { get; init; }  // 修改为枚举类型
}