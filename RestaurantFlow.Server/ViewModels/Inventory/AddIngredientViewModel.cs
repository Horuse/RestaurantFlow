using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using RestaurantFlow.Data.Entities;
using RestaurantFlow.Server.Services;
using ShadUI;

namespace RestaurantFlow.Server.ViewModels.Inventory
{
    public sealed partial class AddIngredientViewModel : ViewModelBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly DialogManager _dialogManager;
        private readonly ToastManager _toastManager;
        private int? _ingredientId;

        [Reactive]
        [Required(ErrorMessage = "Назва є обов'язковою")]
        private string _name = string.Empty;

        [Reactive]
        [Required(ErrorMessage = "Одиниця виміру є обов'язковою")]
        private string _unit = string.Empty;

        [Reactive]
        [Required(ErrorMessage = "Поточний запас є обов'язковим")]
        [Range(0, 10000, ErrorMessage = "Поточний запас повинен бути від 0 до 10000")]
        private decimal _currentStock = 0;

        [Reactive]
        [Required(ErrorMessage = "Мінімальний запас є обов'язковим")]
        [Range(0, 1000, ErrorMessage = "Мінімальний запас повинен бути від 0 до 1000")]
        private decimal _minimumStock = 0;

        [Reactive]
        private string _title = "Додати інгредієнт";

        [Reactive]
        private string _submitText = "Додати";

        [Reactive]
        private bool _isBusy = false;

        public AddIngredientViewModel(IInventoryService inventoryService, DialogManager dialogManager, ToastManager toastManager)
        {
            _inventoryService = inventoryService;
            _dialogManager = dialogManager;
            _toastManager = toastManager;
        }

        [ReactiveCommand]
        private async Task Submit()
        {
            ClearAllErrors();
            ValidateAllProperties();
                
            if (HasErrors) return;
            
            IsBusy = true;
            try
            {
                if (_ingredientId.HasValue)
                {
                    var existingIngredient = await _inventoryService.GetIngredientByIdAsync(_ingredientId.Value);
                    if (existingIngredient != null)
                    {
                        existingIngredient.Name = Name;
                        existingIngredient.Unit = Unit;
                        existingIngredient.CurrentStock = CurrentStock;
                        existingIngredient.MinimumStock = MinimumStock;

                        await _inventoryService.UpdateIngredientAsync(existingIngredient);
                        _dialogManager.Close(this, new CloseDialogOptions { Success = true });
                    }
                }
                else
                {
                    var ingredient = new Ingredient
                    {
                        Name = Name,
                        Unit = Unit,
                        CurrentStock = CurrentStock,
                        MinimumStock = MinimumStock,
                        IsActive = true
                    };

                    await _inventoryService.CreateIngredientAsync(ingredient);
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

        public void Initialize()
        {
            _ingredientId = null;
            Name = string.Empty;
            Unit = string.Empty;
            CurrentStock = 0;
            MinimumStock = 0;
            Title = "Додати інгредієнт";
            SubmitText = "Додати";
            IsBusy = false;
        }

        public async Task InitializeForEdit(int ingredientId)
        {
            _ingredientId = ingredientId;
            Title = "Редагувати інгредієнт";
            SubmitText = "Зберегти";
            
            var ingredient = await _inventoryService.GetIngredientByIdAsync(ingredientId);
            if (ingredient != null)
            {
                Name = ingredient.Name;
                Unit = ingredient.Unit;
                CurrentStock = ingredient.CurrentStock;
                MinimumStock = ingredient.MinimumStock;
            }
            
            IsBusy = false;
        }

        private void ShowErrorToast(string message)
        {
            _toastManager.CreateToast("Помилка")
                .WithContent(message)
                .Show();
        }
    }
}