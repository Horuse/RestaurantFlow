using ReactiveUI;
using ReactiveUI.SourceGenerators;
using RestaurantFlow.Data.Entities;

namespace RestaurantFlow.Server.Models;

public partial class IngredientItem : ReactiveObject
{
    public int Id { get; set; }
    
    [Reactive]
    private string _name = string.Empty;
    
    [Reactive]
    private string _unit = string.Empty;
    
    [Reactive]
    private decimal _currentStock;
    
    [Reactive]
    private decimal _minimumStock;
    
    [Reactive]
    private bool _isActive;

    public static IngredientItem FromIngredient(Ingredient ingredient)
    {
        return new IngredientItem
        {
            Id = ingredient.Id,
            Name = ingredient.Name,
            Unit = ingredient.Unit,
            CurrentStock = ingredient.CurrentStock,
            MinimumStock = ingredient.MinimumStock,
            IsActive = ingredient.IsActive
        };
    }

    public Ingredient ToIngredient()
    {
        return new Ingredient
        {
            Id = Id,
            Name = Name,
            Unit = Unit,
            CurrentStock = CurrentStock,
            MinimumStock = MinimumStock,
            IsActive = IsActive
        };
    }
}