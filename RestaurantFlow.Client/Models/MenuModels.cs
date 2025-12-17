using ReactiveUI;

namespace RestaurantFlow.Client.Models;

public class MenuItemModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsCurrentlyAvailable { get; set; } = true;
    public int EstimatedCookingTimeMinutes { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public bool HasImage { get; set; }
    public string ImageUrl => HasImage ? $"http://localhost:5000/api/menu/items/{Id}/image" : "";
}

public class CategoryModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int DisplayOrder { get; set; }
}

public class CartItemModel : ReactiveObject
{
    public MenuItemModel MenuItem { get; set; } = new();
    
    private int _quantity;
    public int Quantity 
    { 
        get => _quantity; 
        set 
        { 
            this.RaiseAndSetIfChanged(ref _quantity, value);
            this.RaisePropertyChanged(nameof(TotalPrice));
        }
    }
    
    public string? SpecialInstructions { get; set; }
    public decimal TotalPrice => MenuItem.Price * Quantity;
}