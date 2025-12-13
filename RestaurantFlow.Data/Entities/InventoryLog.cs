namespace RestaurantFlow.Data.Entities;

public class InventoryLog
{
    public int Id { get; set; }
    public int IngredientId { get; set; }
    public decimal QuantityChanged { get; set; }  // + додали, - використали
    public decimal StockAfter { get; set; }
    public string Reason { get; set; } = string.Empty;  // "Order #123", "Restock", "Waste"
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public Ingredient Ingredient { get; set; } = null!;
}