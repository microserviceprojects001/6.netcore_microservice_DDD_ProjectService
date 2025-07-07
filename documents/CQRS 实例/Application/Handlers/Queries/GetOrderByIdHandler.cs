// Handlers/Queries/GetOrderByIdHandler.cs
public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly OrderReadDbContext _readDb;
    private readonly IConfigurationProvider _mapperConfig;
    public GetOrderByIdHandler(OrderReadDbContext readDb, IConfigurationProvider mapperConfig)
    {
        _readDb = readDb;
        _mapperConfig = mapperConfig;
    }
    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken ct)
    {
        return await _readDb.Orders
                                .Where(o => o.Id == request.OrderId)
                                .ProjectTo<OrderDto>(_mapperConfig)
                                .FirstOrDefaultAsync(ct)
    ?? throw new NotFoundException("订单不存在");
    }
}