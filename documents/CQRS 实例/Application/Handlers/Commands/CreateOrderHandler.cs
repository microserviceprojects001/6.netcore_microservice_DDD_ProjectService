// Handlers/Commands/CreateOrderHandler.cs
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public CreateOrderHandler(IOrderRepository orderRepository, IMediator mediator, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mediator = mediator;
        _mapper = mapper;
    }


    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        // var items = request.Items
        //                 .Select(dto => new OrderItem(dto.ProductId, dto.Quantity, dto.Price)) // 手动映射
        //                 .ToList();


        var items = _mapper.Map<List<OrderItem>>(request.Items);
        var order = Order.Create(request.UserId, items);

        await _orderRepository.AddAsync(order); // 用接口方式操作数据

        foreach (var domainEvent in order.DomainEvents)
        {
            await _mediator.Publish(domainEvent, ct);
        }
        order.ClearDomainEvents();

        return order.Id;
    }
}