namespace RestaurantFlow.Data.Entities;

public class MenuItemIngredient
{
    public int MenuItemId { get; set; }
    public int IngredientId { get; set; }
    public decimal Quantity { get; set; }  // Скільки потрібно інгредієнта на одну порцію
    
    // Navigation properties
    public MenuItem MenuItem { get; set; } = null!;
    public Ingredient Ingredient { get; set; } = null!;
}