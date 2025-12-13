namespace RestaurantFlow.Data.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation properties
    public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
}