using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Threading.Tasks;
using RestaurantFlow.Server.Services;
using RestaurantFlow.Data.Entities;
using RestaurantFlow.Server.ViewModels.Inventory;
using RestaurantFlow.Server.Models;
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
    private ObservableCollection<IngredientItem> _ingredients = new();
    
    [Reactive]
    private ObservableCollection<IngredientItem> _lowStockIngredients = new();
    
    [Reactive]
    private ObservableCollection<InventoryLog> _inventoryLogs = new();
    
    [Reactive]
    private IngredientItem? _selectedIngredient;
    
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
        _ = LoadDataCommand.Execute();
    }

    [ReactiveCommand]
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
                Ingredients.Add(IngredientItem.FromIngredient(ingredient));
            }
            
            foreach (var ingredient in lowStock)
            {
                LowStockIngredients.Add(IngredientItem.FromIngredient(ingredient));
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
    private async Task AddNewIngredient()
    {
        try
        {
            var addIngredientVm = _serviceProvider.GetRequiredService<AddIngredientViewModel>();
            addIngredientVm.Initialize();
            
            _dialogManager.CreateDialog(addIngredientVm)
                .Dismissible()
                .WithSuccessCallback(async _ =>
                {
                    await LoadDataCommand.Execute();
                })
                .Show();
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error in AddNewIngredient: {ex.Message}");
        }
    }
    
    [ReactiveCommand]
    private async Task EditIngredient(IngredientItem ingredient)
    {
        try
        {
            var addIngredientVm = _serviceProvider.GetRequiredService<AddIngredientViewModel>();
            await addIngredientVm.InitializeForEdit(ingredient.Id);
            
            _dialogManager.CreateDialog(addIngredientVm)
                .Dismissible()
                .WithSuccessCallback(async _ =>
                {
                    await LoadDataCommand.Execute();
                })
                .Show();
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error in EditIngredient: {ex.Message}");
        }
    }
    
    
    [ReactiveCommand]
    private async Task DeleteIngredient(IngredientItem ingredient)
    {
        try
        {
            await _inventoryService.DeleteIngredientAsync(ingredient.Id);
            await LoadDataCommand.Execute();
        }
        catch (Exception ex)
        {
        }
    }
    
    [ReactiveCommand]
    private async Task AdjustStock(IngredientItem ingredient)
    {
        if (StockAdjustment == 0 || string.IsNullOrWhiteSpace(AdjustmentReason))
            return;
            
        try
        {
            await _inventoryService.UpdateIngredientStockAsync(ingredient.Id, StockAdjustment, AdjustmentReason);
            await LoadDataCommand.Execute();
            
            StockAdjustment = 0;
            AdjustmentReason = "";
        }
        catch (Exception ex)
        {
        }
    }
    
    public string GetStockStatusColor(IngredientItem ingredient)
    {
        if (ingredient.CurrentStock <= ingredient.MinimumStock)
            return "#dc3545"; // Red for low stock
        else if (ingredient.CurrentStock <= ingredient.MinimumStock * 1.5m)
            return "#ffc107"; // Yellow for medium stock
        else
            return "#28a745"; // Green for good stock
    }
    
}