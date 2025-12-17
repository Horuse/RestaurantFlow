using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using RestaurantFlow.Server.Services;
using RestaurantFlow.Data.Entities;
using RestaurantFlow.Server.ViewModels.Inventory;
using ShadUI;
using Microsoft.Extensions.DependencyInjection;

namespace RestaurantFlow.Server.ViewModels;

public partial class InventoryViewModel : ReactiveObject
{
    private readonly IInventoryService _inventoryService;
    private readonly DialogManager _dialogManager;
    private readonly ToastManager _toastManager;
    private readonly IServiceProvider _serviceProvider;
    
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
    private decimal _stockAdjustment = 0;
    
    [Reactive]
    private string _adjustmentReason = "";

    public InventoryViewModel(IInventoryService inventoryService, DialogManager dialogManager, ToastManager toastManager, IServiceProvider serviceProvider)
    {
        _inventoryService = inventoryService;
        _dialogManager = dialogManager;
        _toastManager = toastManager;
        _serviceProvider = serviceProvider;
        
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
    private async Task AddNewIngredient()
    {
        System.Console.WriteLine("AddNewIngredient method called!");
        
        try
        {
            System.Console.WriteLine("Getting AddIngredientViewModel from DI...");
            var addIngredientVm = _serviceProvider.GetRequiredService<AddIngredientViewModel>();
            System.Console.WriteLine("AddIngredientViewModel retrieved successfully");
            
            System.Console.WriteLine("Initializing AddIngredientViewModel...");
            addIngredientVm.Initialize();
            System.Console.WriteLine("AddIngredientViewModel initialized");
            
            System.Console.WriteLine("Creating dialog...");
            _dialogManager.CreateDialog(addIngredientVm)
                .Dismissible()
                .WithSuccessCallback(async _ =>
                {
                    System.Console.WriteLine("Dialog success callback triggered");
                    await LoadDataAsync();
                })
                .Show();
            System.Console.WriteLine("Dialog shown successfully");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error in AddNewIngredient: {ex.Message}");
            System.Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
    
    [ReactiveCommand]
    private async Task EditIngredient(Ingredient ingredient)
    {
        System.Console.WriteLine($"EditIngredient called for: {ingredient.Name}");
        
        try
        {
            var addIngredientVm = _serviceProvider.GetRequiredService<AddIngredientViewModel>();
            await addIngredientVm.InitializeForEdit(ingredient.Id);
            
            _dialogManager.CreateDialog(addIngredientVm)
                .Dismissible()
                .WithSuccessCallback(async _ =>
                {
                    System.Console.WriteLine("EditIngredient dialog success callback triggered");
                    await LoadDataAsync();
                })
                .Show();
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error in EditIngredient: {ex.Message}");
        }
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