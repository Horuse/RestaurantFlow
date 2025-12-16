namespace RestaurantFlow.Client.Models;

public class MenuItemModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public int EstimatedCookingTimeMinutes { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public string ImageUrl => $"/api/menu/items/{Id}/image";
}

public class CategoryModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int DisplayOrder { get; set; }
}

public class CartItemModel
{
    public MenuItemModel MenuItem { get; set; } = new();
    public int Quantity { get; set; }
    public string? SpecialInstructions { get; set; }
    public decimal TotalPrice => MenuItem.Price * Quantity;
}