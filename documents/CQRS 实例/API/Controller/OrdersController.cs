// API/Controllers/OrdersController.cs
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ECommerce.Application.Commands;
using ECommerce.Application.Queries;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrdersController(IMediator mediator)
            => _mediator = mediator;

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