// API/DTOs/OrderDto.cs
namespace ECommerce.API.DTOs
{
    public record OrderDto(Guid Id, Guid UserId, string Status, List<OrderItemDto> Items);

    // public class OrderDto
    // {
    //     public Guid Id { get; set; }
    //     public Guid UserId { get; set; }
    //     public string? Status { get; set; }
    //     public List<OrderItemDto> Items { get; set; } = new();
    // }

    public class OrderItemDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}