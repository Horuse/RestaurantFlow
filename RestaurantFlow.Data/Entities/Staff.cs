using RestaurantFlow.Shared.Enums;

namespace RestaurantFlow.Data.Entities;

public class Staff
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public StaffRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public ICollection<OrderItem> PreparedItems { get; set; } = new List<OrderItem>();  // Для кухарів
    public ICollection<Order> HandedOrders { get; set; } = new List<Order>();  // Для працівників видачі
}