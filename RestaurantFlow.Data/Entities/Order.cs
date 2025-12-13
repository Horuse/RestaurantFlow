using RestaurantFlow.Shared.Enums;

namespace RestaurantFlow.Data.Entities;

public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;  // Унікальний номер типу "#125"
    public OrderType OrderType { get; set; }  // На столик чи з собою
    public int? TableNumber { get; set; }  // 1-10, null якщо TakeAway
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string SpecialRequests { get; set; } = string.Empty;
    public int? HandedByStaffId { get; set; }  // Хто видав замовлення
    public DateTime CreatedAt { get; set; }
    public DateTime PaidAt { get; set; }  // Коли оплачено
    public DateTime? CompletedAt { get; set; }  // Коли видано
    
    // Navigation properties
    public Staff? HandedByStaff { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}