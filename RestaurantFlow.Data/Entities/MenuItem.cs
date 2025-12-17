namespace RestaurantFlow.Data.Entities;

public class MenuItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public byte[]? Image { get; set; }
    public int EstimatedCookingTimeMinutes { get; set; }
    public int Calories { get; set; }
    public string Allergens { get; set; } = string.Empty;
    public bool IsPopular { get; set; }
    public bool IsRecommended { get; set; }
    public bool IsAvailable { get; set; }
    public int CategoryId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public Category Category { get; set; } = null!;
    public ICollection<MenuItemIngredient> MenuItemIngredients { get; set; } = new List<MenuItemIngredient>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    
    // Computed property - не зберігається в базі
    public bool IsCurrentlyAvailable { get; set; } = true;
}