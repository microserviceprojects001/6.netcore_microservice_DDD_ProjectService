// Tests/UnitTests/OrderTests.cs
using Moq;
using ECommerce.Domain.Models;
using ECommerce.Domain.Interfaces;
using ECommerce.Application.Commands;
using ECommerce.Application.Handlers.Commands;

public class OrderTests
{
    [Fact]
    public async Task CreateOrderHandler_Should_Return_OrderId()
    {
        // 模拟仓储
        var mockRepo = new Mock<IOrderRepository>();
        mockRepo.Setup(repo => repo.AddAsync(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);

        var mediatorMock = new Mock<IMediator>();
        var handler = new CreateOrderHandler(mockRepo.Object, mediatorMock.Object);

        // 执行命令
        var result = await handler.Handle(
            new CreateOrderCommand { UserId = Guid.NewGuid(), Items = new List<OrderItemDto>() },
            CancellationToken.None);

        // 验证
        Assert.NotEqual(Guid.Empty, result);
        mockRepo.Verify(repo => repo.AddAsync(It.IsAny<Order>()), Times.Once);
    }
}