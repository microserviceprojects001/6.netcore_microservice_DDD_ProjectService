// API/Controllers/OrdersController.cs
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ECommerce.API.DTOs;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrdersController(IMediator mediator)
            => _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orderId = await _mediator.Send(new CreateOrderCommand()
            {
                UserId = Guid.NewGuid(),
                Items = new List<OrderItemDto>
                 {
                     new OrderItemDto { ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 100.00m }
                 }
            });  // <-- 这里调用了 Handler
            //return CreatedAtAction(nameof(GetOrder), new { id = orderId }, null);

            //var orders = await _mediator.Send(new GetOrdersQuery());
            return Ok(orderId);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
        {
            var orderId = await _mediator.Send(command);  // <-- 这里调用了 Handler
            return CreatedAtAction(nameof(GetOrder), new { id = orderId }, null);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(Guid id)
            => Ok(await _mediator.Send(new GetOrderByIdQuery { OrderId = id }));

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] UpdateOrderStatusCommand command)
        {
            command.OrderId = id;
            await _mediator.Send(command);
            return NoContent();
        }
    }
}