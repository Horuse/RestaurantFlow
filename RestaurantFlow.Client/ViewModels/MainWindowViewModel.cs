using System;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using RestaurantFlow.Client.Models;
using RestaurantFlow.Client.Services;
using RestaurantFlow.Client.Views;

namespace RestaurantFlow.Client.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly RestaurantApiService _apiService;
    
    [Reactive]
    private ObservableCollection<MenuItemModel> _menuItems = new();
    
    [Reactive]
    private ObservableCollection<CategoryModel> _categories = new();
    
    [Reactive]
    private CategoryModel? _selectedCategory;
    
    [Reactive]
    private int _tableNumber = 1;
    
    [Reactive]
    private string _tableNumberText = "1";
    
    [Reactive]
    private bool _isLoading;
    
    [Reactive]
    private ViewModelBase? _currentView;
    
    [Reactive]
    private bool _isMenuView = true;
    
    [Reactive] 
    private bool _isCartView = false;
    
    [Reactive]
    private bool _isOrderSuccessView = false;
    
    [Reactive]
    private bool _isTableSetupView = true;
    
    [Reactive]
    private bool _hasTableSetup = false;
    
    [Reactive]
    private string _searchText = "";

    public CartService CartService { get; }
    
    public ObservableCollection<MenuItemModel> FilteredMenuItems { get; } = new();
    
    public CartViewModel CartViewModel { get; }
    
    public OrderSuccessViewModel OrderSuccessViewModel { get; }
    
    public TableSetupViewModel TableSetupViewModel { get; }

    public MainWindowViewModel() : this(null!, null!)
    {
    }

    public MainWindowViewModel(RestaurantApiService apiService, CartService cartService)
    {
        _apiService = apiService;
        CartService = cartService;
        
        CartViewModel = new CartViewModel(apiService, cartService);
        OrderSuccessViewModel = new OrderSuccessViewModel();
        TableSetupViewModel = new TableSetupViewModel();
        
        TableSetupViewModel.TableConfirmed += OnTableConfirmed;
        
        CartViewModel.BackToMenuRequested += () => ShowMenuView();
        CartViewModel.OrderCompleted += orderNumber =>
        {
            ShowOrderSuccessView(orderNumber);
        };
        
        OrderSuccessViewModel.BackToMenuRequested += () => ShowMenuView();
        OrderSuccessViewModel.TimerExpired += () => ShowMenuView();
        
        this.WhenAnyValue(x => x.SelectedCategory)
            .Subscribe(_ => FilterMenuItems());
            
        this.WhenAnyValue(x => x.SearchText)
            .Subscribe(_ => FilterMenuItems());

        this.WhenAnyValue(x => x.TableNumberText)
            .Subscribe(text =>
            {
                if (int.TryParse(text, out var number) && number >= 1 && number <= 99)
                {
                    TableNumber = number;
                }
            });

        this.WhenAnyValue(x => x.TableNumber)
            .Subscribe(number =>
            {
                if (TableNumberText != number.ToString())
                {
                    TableNumberText = number.ToString();
                }
            });
            
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            var menuItemsTask = _apiService.GetMenuItemsAsync();
            var categoriesTask = _apiService.GetCategoriesAsync();
            
            await Task.WhenAll(menuItemsTask, categoriesTask);
            
            var menuItems = await menuItemsTask;
            var categories = await categoriesTask;
            
            MenuItems.Clear();
            Categories.Clear();
            
            Categories.Add(new CategoryModel { Id = 0, Name = "Усе", DisplayOrder = 0 });
            
            foreach (var category in categories.OrderBy(c => c.DisplayOrder))
            {
                Categories.Add(category);
            }
            
            foreach (var item in menuItems)
            {
                MenuItems.Add(item);
            }
            
            FilterMenuItems();
            SelectedCategory = Categories.FirstOrDefault();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading data: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void FilterMenuItems()
    {
        FilteredMenuItems.Clear();
        
        var itemsToShow = SelectedCategory?.Id == 0 || SelectedCategory == null
            ? MenuItems.Where(item => item.IsAvailable)
            : MenuItems.Where(item => item.IsAvailable && item.CategoryId == SelectedCategory.Id);
            
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            itemsToShow = itemsToShow.Where(item => 
                item.Name.ToLowerInvariant().Contains(SearchText.ToLowerInvariant()) ||
                item.Description.ToLowerInvariant().Contains(SearchText.ToLowerInvariant()));
        }
            
        foreach (var item in itemsToShow)
        {
            FilteredMenuItems.Add(item);
        }
    }

    [ReactiveCommand]
    private void SelectCategory(CategoryModel category)
    {
        SelectedCategory = category;
    }

    [ReactiveCommand]
    private void AddToCart(MenuItemModel menuItem)
    {
        CartService.AddItem(menuItem);
    }

    [ReactiveCommand]
    private void OpenCart()
    {
        ShowCartView();
        
        CartViewModel.TableNumber = TableNumber;
        CartViewModel.TableNumberText = TableNumberText;
    }

    private void ShowMenuView()
    {
        IsMenuView = true;
        IsCartView = false;
        IsOrderSuccessView = false;
        IsTableSetupView = false;
    }

    private void ShowCartView()
    {
        IsMenuView = false;
        IsCartView = true;
        IsOrderSuccessView = false;
        IsTableSetupView = false;
    }

    private void ShowOrderSuccessView(string orderNumber)
    {
        IsMenuView = false;
        IsCartView = false;
        IsOrderSuccessView = true;
        IsTableSetupView = false;
        
        OrderSuccessViewModel.StartTimer(orderNumber);
    }
    
    private void OnTableConfirmed(object? sender, string tableNumber)
    {
        TableNumber = int.Parse(tableNumber);
        HasTableSetup = true;
        ShowMenuView();
    }
    
    private void ShowTableSetupView()
    {
        IsMenuView = false;
        IsCartView = false;
        IsOrderSuccessView = false;
        IsTableSetupView = true;
    }
}