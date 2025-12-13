using RestaurantFlow.Shared.Enums;

namespace RestaurantFlow.Data.Entities;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int MenuItemId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }  // Ціна на момент замовлення
    public string SpecialRequests { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public int? CookId { get; set; }  // Хто готує
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedCookingAt { get; set; }
    public DateTime? ReadyAt { get; set; }
    public string Notes { get; set; } = string.Empty;  // Нотатки кухаря
    
    // Navigation properties
    public Order Order { get; set; } = null!;
    public MenuItem MenuItem { get; set; } = null!;
    public Staff? Cook { get; set; }
}