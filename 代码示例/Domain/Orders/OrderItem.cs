
// Domain/Orders/OrderItem.cs
// Domain/Orders/OrderItem.cs
namespace Domain.Orders
{
    public class OrderItem : Entity
    {
        public OrderItemId Id { get; private set; }
        public ProductId ProductId { get; private set; }
        public string ProductName { get; private set; }
        public int Quantity { get; private set; }
        public Money UnitPrice { get; private set; }
        public Money SubTotal => UnitPrice * Quantity;

        // EF Core需要的私有构造函数
        private OrderItem() { }

        // 工厂方法
        public static OrderItem Create(ProductId productId, string productName, int quantity, Money unitPrice)
        {
            if (quantity <= 0)
                throw new ArgumentException("数量必须大于0", nameof(quantity));

            return new OrderItem
            {
                Id = OrderItemId.New(),
                ProductId = productId,
                ProductName = productName ?? throw new ArgumentNullException(nameof(productName)),
                Quantity = quantity,
                UnitPrice = unitPrice
            };
        }

        // 业务方法
        public void IncreaseQuantity(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("增加数量必须大于0", nameof(quantity));

            Quantity += quantity;
        }

        public void UpdateQuantity(int newQuantity)
        {
            if (newQuantity <= 0)
                throw new ArgumentException("数量必须大于0", nameof(newQuantity));

            Quantity = newQuantity;
        }
    }

    public record OrderItemId(Guid Value)
    {
        public static OrderItemId New() => new OrderItemId(Guid.NewGuid());
    }
}