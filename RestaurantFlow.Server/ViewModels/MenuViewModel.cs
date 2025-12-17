using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using RestaurantFlow.Server.Services;
using RestaurantFlow.Data.Entities;
using Microsoft.Extensions.DependencyInjection;
using ShadUI;

namespace RestaurantFlow.Server.ViewModels;

public partial class MenuViewModel : ReactiveObject
{
    private readonly IMenuService _menuService;
    private readonly DialogManager _dialogManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISignalRConnectionService _signalRService;
    
    [Reactive]
    private ObservableCollection<MenuItem> _menuItems = new();
    
    [Reactive]
    private ObservableCollection<Category> _categories = new();
    
    [Reactive]
    private bool _isLoading = false;
    
    [Reactive]
    private string _searchText = "";

    public MenuViewModel(IMenuService menuService, DialogManager dialogManager, IServiceProvider serviceProvider, ISignalRConnectionService signalRService)
    {
        _menuService = menuService;
        _dialogManager = dialogManager;
        _serviceProvider = serviceProvider;
        _signalRService = signalRService;
        
        // Підписуємося на оновлення меню
        _signalRService.MenuUpdated += OnMenuUpdated;
        
        // Load data when ViewModel is created
        _ = LoadDataAsync();
    }
    
    private async void OnMenuUpdated()
    {
        // Оновлюємо дані при отриманні SignalR повідомлення в UI потоці
        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await LoadDataAsync();
        });
    }
    
    private void FilterMenuItems()
    {
        // For now just trigger property changed, later we can add real filtering
        this.RaisePropertyChanged(nameof(MenuItems));
    }

    public async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            var categories = await _menuService.GetCategoriesAsync();
            var menuItems = await _menuService.GetMenuItemsAsync();
            
            Categories.Clear();
            MenuItems.Clear();
            
            foreach (var category in categories)
            {
                Categories.Add(category);
            }
            
            foreach (var item in menuItems)
            {
                MenuItems.Add(item);
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
    private async Task AddNewMenuItemAsync()
    {
        var addMenuItemViewModel = _serviceProvider.GetRequiredService<ViewModels.Menu.AddMenuItemViewModel>();
        await addMenuItemViewModel.InitializeAsync();
        
        _dialogManager.CreateDialog(addMenuItemViewModel)
            .Dismissible()
            .WithSuccessCallback(async vm =>
            {
                await LoadDataAsync();
            })
            .Show();
    }
    
    [ReactiveCommand]
    private async Task EditMenuItemAsync(MenuItem menuItem)
    {
        var editMenuItemViewModel = _serviceProvider.GetRequiredService<ViewModels.Menu.AddMenuItemViewModel>();
        await editMenuItemViewModel.InitializeForEditAsync(menuItem.Id);
        
        _dialogManager.CreateDialog(editMenuItemViewModel)
            .Dismissible()
            .WithSuccessCallback(async vm =>
            {
                await LoadDataAsync();
            })
            .Show();
    }
    
    [ReactiveCommand]
    private async Task DeleteMenuItem(MenuItem menuItem)
    {
        try
        {
            await _menuService.DeleteMenuItemAsync(menuItem.Id);
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
        }
    }
    
    [ReactiveCommand]
    private async Task ToggleAvailability(MenuItem menuItem)
    {
        try
        {
            menuItem.IsAvailable = !menuItem.IsAvailable;
            await _menuService.UpdateMenuItemAsync(menuItem);
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            // TODO: Show error message to user
            menuItem.IsAvailable = !menuItem.IsAvailable; // Revert on error
        }
    }
}