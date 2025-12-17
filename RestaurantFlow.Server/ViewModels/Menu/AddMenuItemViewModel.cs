using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using RestaurantFlow.Server.Services;
using RestaurantFlow.Data.Entities;
using ShadUI;

namespace RestaurantFlow.Server.ViewModels.Menu;

public partial class IngredientSelectionItem : ReactiveObject
{
    [Reactive]
    private bool _isSelected;
    
    [Reactive]
    private decimal _quantity = 0;
    
    public Ingredient Ingredient { get; set; } = null!;
    public string Name => Ingredient?.Name ?? "";
    public string Unit => Ingredient?.Unit ?? "";
}

public partial class AddMenuItemViewModel : ReactiveObject
{
    private readonly IMenuService _menuService;
    private readonly IInventoryService _inventoryService;
    private readonly DialogManager _dialogManager;
    private readonly ToastManager _toastManager;
    private int? _menuItemId; // null для додавання, id для редагування
    
    [Required(ErrorMessage = "Назва страви є обов'язковою")]
    [Reactive]
    private string _name = "";

    [Required(ErrorMessage = "Опис страви є обов'язковий")]
    [Reactive]
    private string _description = "";

    [Required(ErrorMessage = "Ціна є обов'язковою")]
    [Range(0.01, 10000, ErrorMessage = "Ціна повинна бути від 0.01 до 10000 грн")]
    [Reactive]
    private decimal _price = 0;

    [Required(ErrorMessage = "Час приготування є обов'язковим")]
    [Range(1, 120, ErrorMessage = "Час приготування повинен бути від 1 до 120 хвилин")]
    [Reactive]
    private int _estimatedCookingTimeMinutes = 5;

    [Range(0, 5000, ErrorMessage = "Калорії повинні бути від 0 до 5000")]
    [Reactive]
    private int _calories = 0;

    [Reactive]
    private string _allergens = "";

    [Reactive]
    private bool _isPopular = false;

    [Reactive]
    private bool _isRecommended = false;

    [Reactive]
    private bool _isAvailable = true;

    [Required(ErrorMessage = "Категорія є обов'язковою")]
    [Reactive]
    private Category? _selectedCategory;

    [Reactive]
    private bool _isLoading = false;

    [Reactive]
    private bool _isBusy = false;

    [Reactive]
    private string _title = "Додати страву";

    [Reactive]
    private string _submitText = "Додати";

    public ObservableCollection<Category> Categories { get; } = new();
    public ObservableCollection<IngredientSelectionItem> AvailableIngredients { get; } = new();

    public AddMenuItemViewModel(IMenuService menuService, IInventoryService inventoryService, DialogManager dialogManager, ToastManager toastManager)
    {
        _menuService = menuService;
        _inventoryService = inventoryService;
        _dialogManager = dialogManager;
        _toastManager = toastManager;
    }

    public async Task InitializeAsync()
    {
        _menuItemId = null;
        Title = "Додати страву";
        SubmitText = "Додати";
        await LoadDataAsync();
    }

    public async Task InitializeForEditAsync(int menuItemId)
    {
        _menuItemId = menuItemId;
        Title = "Редагувати страву";
        SubmitText = "Зберегти";
        
        await LoadDataAsync();
        
        var menuItem = await _menuService.GetMenuItemByIdAsync(menuItemId);
        if (menuItem != null)
        {
            Name = menuItem.Name;
            Description = menuItem.Description;
            Price = menuItem.Price;
            EstimatedCookingTimeMinutes = menuItem.EstimatedCookingTimeMinutes;
            Calories = menuItem.Calories;
            Allergens = menuItem.Allergens;
            IsPopular = menuItem.IsPopular;
            IsRecommended = menuItem.IsRecommended;
            IsAvailable = menuItem.IsAvailable;
            SelectedCategory = Categories.FirstOrDefault(c => c.Id == menuItem.CategoryId);

            // Завантажуємо інгредієнти страви
            var menuItemIngredients = await _menuService.GetMenuItemIngredientsAsync(menuItemId);
            foreach (var mii in menuItemIngredients)
            {
                var ingredientItem = AvailableIngredients.FirstOrDefault(ai => ai.Ingredient.Id == mii.IngredientId);
                if (ingredientItem != null)
                {
                    ingredientItem.IsSelected = true;
                    ingredientItem.Quantity = mii.Quantity;
                }
            }
        }
    }

    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            var categories = await _menuService.GetCategoriesAsync();
            var ingredients = await _inventoryService.GetIngredientsAsync();

            Categories.Clear();
            foreach (var category in categories)
            {
                Categories.Add(category);
            }

            AvailableIngredients.Clear();
            foreach (var ingredient in ingredients)
            {
                AvailableIngredients.Add(new IngredientSelectionItem
                {
                    Ingredient = ingredient,
                    IsSelected = false,
                    Quantity = 0
                });
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [ReactiveCommand]
    private async Task SubmitAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ShowErrorToast("Назва страви є обов'язковою");
            return;
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            ShowErrorToast("Опис страви є обов'язковий");
            return;
        }

        if (Price <= 0)
        {
            ShowErrorToast("Ціна повинна бути більше 0");
            return;
        }

        if (SelectedCategory == null)
        {
            ShowErrorToast("Оберіть категорію");
            return;
        }

        IsBusy = true;
        try
        {
            if (_menuItemId.HasValue)
            {
                // Редагування існуючої страви
                var menuItem = await _menuService.GetMenuItemByIdAsync(_menuItemId.Value);
                if (menuItem != null)
                {
                    menuItem.Name = Name;
                    menuItem.Description = Description;
                    menuItem.Price = Price;
                    menuItem.EstimatedCookingTimeMinutes = EstimatedCookingTimeMinutes;
                    menuItem.Calories = Calories;
                    menuItem.Allergens = Allergens;
                    menuItem.IsPopular = IsPopular;
                    menuItem.IsRecommended = IsRecommended;
                    menuItem.IsAvailable = IsAvailable;
                    menuItem.CategoryId = SelectedCategory.Id;
                    menuItem.UpdatedAt = DateTime.UtcNow;

                    await _menuService.UpdateMenuItemAsync(menuItem);
                    
                    // Оновлюємо інгредієнти
                    await SaveIngredientsAsync(menuItem.Id);
                    
                    _dialogManager.Close(this, new CloseDialogOptions { Success = true });
                }
            }
            else
            {
                // Додавання нової страви
                var newMenuItem = new MenuItem
                {
                    Name = Name,
                    Description = Description,
                    Price = Price,
                    EstimatedCookingTimeMinutes = EstimatedCookingTimeMinutes,
                    Calories = Calories,
                    Allergens = Allergens,
                    IsPopular = IsPopular,
                    IsRecommended = IsRecommended,
                    IsAvailable = IsAvailable,
                    CategoryId = SelectedCategory.Id,
                    CreatedAt = DateTime.UtcNow
                };

                var createdMenuItem = await _menuService.CreateMenuItemAsync(newMenuItem);
                
                // Зберігаємо інгредієнти
                await SaveIngredientsAsync(createdMenuItem.Id);
                
                _dialogManager.Close(this, new CloseDialogOptions { Success = true });
            }
        }
        catch (Exception ex)
        {
            ShowErrorToast(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [ReactiveCommand]
    private void Cancel()
    {
        _dialogManager.Close(this, new CloseDialogOptions { Success = false });
    }

    private async Task SaveIngredientsAsync(int menuItemId)
    {
        var selectedIngredients = AvailableIngredients
            .Where(ai => ai.IsSelected && ai.Quantity > 0)
            .Select(ai => new MenuItemIngredient
            {
                MenuItemId = menuItemId,
                IngredientId = ai.Ingredient.Id,
                Quantity = ai.Quantity
            })
            .ToList();

        await _menuService.UpdateMenuItemIngredientsAsync(menuItemId, selectedIngredients);
    }

    private void ShowErrorToast(string message)
    {
        _toastManager.CreateToast("Помилка")
            .WithContent(message)
            .Show();
    }
}