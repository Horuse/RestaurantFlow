using System;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using RestaurantFlow.Client.Models;
using RestaurantFlow.Client.Services;

namespace RestaurantFlow.Client.ViewModels;

public partial class CartViewModel : ViewModelBase
{
    private readonly RestaurantApiService _apiService;
    
    [Reactive]
    private int _tableNumber = 1;
    
    [Reactive]
    private string _tableNumberText = "1";
    
    [Reactive]
    private bool _isLoading;
    
    [Reactive]
    private bool _isOrderPlaced;

    public CartService CartService { get; }
    
    public bool IsCartEmpty => CartService.TotalItems == 0;
    
    public ReactiveCommand<Unit, Unit> BackToMenuCommand { get; }
    public ReactiveCommand<CartItemModel, Unit> IncreaseQuantityCommand { get; }
    public ReactiveCommand<CartItemModel, Unit> DecreaseQuantityCommand { get; }
    public ReactiveCommand<CartItemModel, Unit> RemoveItemCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearCartCommand { get; }
    public ReactiveCommand<Unit, Unit> PlaceOrderCommand { get; }

    public event Action? BackToMenuRequested;
    public event Action<string>? OrderCompleted;

    public CartViewModel() : this(null!, null!)
    {
    }

    public CartViewModel(RestaurantApiService apiService, CartService cartService)
    {
        _apiService = apiService;
        CartService = cartService;
        
        CartService.PropertyChanged += (_, _) =>
        {
            this.RaisePropertyChanged(nameof(IsCartEmpty));
        };

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

        BackToMenuCommand = ReactiveCommand.Create(BackToMenu);
        IncreaseQuantityCommand = ReactiveCommand.Create<CartItemModel>(IncreaseQuantity);
        DecreaseQuantityCommand = ReactiveCommand.Create<CartItemModel>(DecreaseQuantity);
        RemoveItemCommand = ReactiveCommand.Create<CartItemModel>(RemoveItem);
        ClearCartCommand = ReactiveCommand.Create(ClearCart);
        PlaceOrderCommand = ReactiveCommand.CreateFromTask(PlaceOrder, 
            this.WhenAnyValue(x => x.IsCartEmpty, isEmpty => !isEmpty && !IsLoading));
    }

    private void BackToMenu()
    {
        BackToMenuRequested?.Invoke();
    }

    private void IncreaseQuantity(CartItemModel item)
    {
        CartService.UpdateQuantity(item, item.Quantity + 1);
    }

    private void DecreaseQuantity(CartItemModel item)
    {
        CartService.UpdateQuantity(item, item.Quantity - 1);
    }

    private void RemoveItem(CartItemModel item)
    {
        CartService.RemoveItem(item);
    }

    private void ClearCart()
    {
        CartService.ClearCart();
    }

    private async Task PlaceOrder()
    {
        if (CartService.TotalItems == 0)
            return;

        IsLoading = true;
        try
        {
            System.Diagnostics.Debug.WriteLine($"Creating order for table {TableNumber} with {CartService.TotalItems} items");
            
            var orderRequest = CartService.CreateOrderRequest(TableNumber);
            System.Diagnostics.Debug.WriteLine($"Order request created: {System.Text.Json.JsonSerializer.Serialize(orderRequest)}");
            
            var order = await _apiService.CreateOrderAsync(orderRequest);
            
            if (order != null)
            {
                System.Diagnostics.Debug.WriteLine($"Order created successfully: {order.OrderNumber}");
                IsOrderPlaced = true;
                
                CartService.ClearCart();
                
                OrderCompleted?.Invoke(order.OrderNumber);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Order creation returned null");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating order: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}