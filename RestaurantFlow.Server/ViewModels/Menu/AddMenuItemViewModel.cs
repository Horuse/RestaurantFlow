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

public partial class AddMenuItemViewModel : ReactiveObject
{
    private readonly IMenuService _menuService;
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

    public AddMenuItemViewModel(IMenuService menuService, DialogManager dialogManager, ToastManager toastManager)
    {
        _menuService = menuService;
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
        }
    }

    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            var categories = await _menuService.GetCategoriesAsync();

            Categories.Clear();
            foreach (var category in categories)
            {
                Categories.Add(category);
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

                await _menuService.CreateMenuItemAsync(newMenuItem);
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

    private void ShowErrorToast(string message)
    {
        _toastManager.CreateToast("Помилка")
            .WithContent(message)
            .Show();
    }
}