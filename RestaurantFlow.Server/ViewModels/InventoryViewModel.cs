using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using RestaurantFlow.Server.Services;
using RestaurantFlow.Data.Entities;

namespace RestaurantFlow.Server.ViewModels;

public partial class InventoryViewModel : ReactiveObject
{
    private readonly IInventoryService _inventoryService;
    
    [Reactive]
    private ObservableCollection<Ingredient> _ingredients = new();
    
    [Reactive]
    private ObservableCollection<Ingredient> _lowStockIngredients = new();
    
    [Reactive]
    private ObservableCollection<InventoryLog> _inventoryLogs = new();
    
    [Reactive]
    private Ingredient? _selectedIngredient;
    
    [Reactive]
    private bool _isLoading = false;
    
    [Reactive]
    private bool _isEditing = false;
    
    [Reactive]
    private decimal _stockAdjustment = 0;
    
    [Reactive]
    private string _adjustmentReason = "";

    public InventoryViewModel(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
        
        // Load data when ViewModel is created
        _ = LoadDataAsync();
    }

    public async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            var ingredients = await _inventoryService.GetIngredientsAsync();
            var lowStock = await _inventoryService.GetLowStockIngredientsAsync();
            var logs = await _inventoryService.GetInventoryLogsAsync();
            
            Ingredients.Clear();
            LowStockIngredients.Clear();
            InventoryLogs.Clear();
            
            foreach (var ingredient in ingredients)
            {
                Ingredients.Add(ingredient);
            }
            
            foreach (var ingredient in lowStock)
            {
                LowStockIngredients.Add(ingredient);
            }
            
            foreach (var log in logs)
            {
                InventoryLogs.Add(log);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [ReactiveCommand]
    private async Task RefreshDataAsync()
    {
        await LoadDataAsync();
    }
    
    [ReactiveCommand]
    private void AddNewIngredient()
    {
        SelectedIngredient = new Ingredient
        {
            Name = "",
            Unit = "",
            CurrentStock = 0,
            MinimumStock = 0,
            IsActive = true
        };
        IsEditing = true;
    }
    
    [ReactiveCommand]
    private void EditIngredient(Ingredient ingredient)
    {
        SelectedIngredient = ingredient;
        IsEditing = true;
    }
    
    [ReactiveCommand]
    private async Task SaveIngredient()
    {
        if (SelectedIngredient == null) return;
        
        try
        {
            if (SelectedIngredient.Id == 0)
            {
                await _inventoryService.CreateIngredientAsync(SelectedIngredient);
            }
            else
            {
                await _inventoryService.UpdateIngredientAsync(SelectedIngredient);
            }
            
            await LoadDataAsync();
            IsEditing = false;
            SelectedIngredient = null;
        }
        catch (Exception ex)
        {
        }
    }
    
    [ReactiveCommand]
    private void CancelEdit()
    {
        IsEditing = false;
        SelectedIngredient = null;
    }
    
    [ReactiveCommand]
    private async Task DeleteIngredient(Ingredient ingredient)
    {
        try
        {
            await _inventoryService.DeleteIngredientAsync(ingredient.Id);
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
        }
    }
    
    [ReactiveCommand]
    private async Task AdjustStock(Ingredient ingredient)
    {
        if (StockAdjustment == 0 || string.IsNullOrWhiteSpace(AdjustmentReason))
            return;
            
        try
        {
            await _inventoryService.UpdateIngredientStockAsync(ingredient.Id, StockAdjustment, AdjustmentReason);
            await LoadDataAsync();
            
            StockAdjustment = 0;
            AdjustmentReason = "";
        }
        catch (Exception ex)
        {
        }
    }
    
    public string GetStockStatusColor(Ingredient ingredient)
    {
        if (ingredient.CurrentStock <= ingredient.MinimumStock)
            return "#dc3545"; // Red for low stock
        else if (ingredient.CurrentStock <= ingredient.MinimumStock * 1.5m)
            return "#ffc107"; // Yellow for medium stock
        else
            return "#28a745"; // Green for good stock
    }
}