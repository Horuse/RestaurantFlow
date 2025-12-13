namespace RestaurantFlow.Data.Entities;

public class Ingredient
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;  // кг, л, шт
    public decimal CurrentStock { get; set; }
    public decimal MinimumStock { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation properties
    public ICollection<MenuItemIngredient> MenuItemIngredients { get; set; } = new List<MenuItemIngredient>();
    public ICollection<InventoryLog> InventoryLogs { get; set; } = new List<InventoryLog>();
}