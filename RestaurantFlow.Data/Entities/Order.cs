using RestaurantFlow.Shared.Enums;

namespace RestaurantFlow.Data.Entities;

public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OrderType OrderType { get; set; }
    public int? TableNumber { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string SpecialRequests { get; set; } = string.Empty;
    public int? HandedByStaffId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime PaidAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    public Staff? HandedByStaff { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}