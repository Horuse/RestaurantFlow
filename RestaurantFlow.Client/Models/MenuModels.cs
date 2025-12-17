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
    public int Calories { get; set; }
    public string Allergens { get; set; } = "";
    public bool IsPopular { get; set; }
    public bool IsRecommended { get; set; }
    public string Ingredients { get; set; } = "";
    
    public string ImageUrl => HasImage ? $"http://localhost:5000/api/menu/items/{Id}/image" : "";
    
    public string FormattedCookingTime => EstimatedCookingTimeMinutes > 0 ? 
        $"~{EstimatedCookingTimeMinutes} хв" : "";
    
    public string FormattedCalories => Calories > 0 ? 
        $"{Calories} ккал" : "";
    
    public bool HasIngredients => !string.IsNullOrWhiteSpace(Ingredients);
    public bool HasAllergens => !string.IsNullOrWhiteSpace(Allergens);
    public bool HasCalories => Calories > 0;
    public bool HasCookingTime => EstimatedCookingTimeMinutes > 0;
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