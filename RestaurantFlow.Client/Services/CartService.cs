using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;
using RestaurantFlow.Client.Models;

namespace RestaurantFlow.Client.Services;

public class CartService : ReactiveObject
{
    private readonly ObservableCollection<CartItemModel> _items = new();
    
    public ReadOnlyObservableCollection<CartItemModel> Items { get; }
    
    public int TotalItems => _items.Sum(item => item.Quantity);
    public decimal TotalPrice => _items.Sum(item => item.TotalPrice);

    public CartService()
    {
        Items = new ReadOnlyObservableCollection<CartItemModel>(_items);
        
        _items.CollectionChanged += (_, _) =>
        {
            this.RaisePropertyChanged(nameof(TotalItems));
            this.RaisePropertyChanged(nameof(TotalPrice));
        };
    }

    public void AddItem(MenuItemModel menuItem, int quantity = 1, string? specialInstructions = null)
    {
        var existingItem = _items.FirstOrDefault(item => 
            item.MenuItem.Id == menuItem.Id && 
            item.SpecialInstructions == specialInstructions);
            
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
            this.RaisePropertyChanged(nameof(TotalItems));
            this.RaisePropertyChanged(nameof(TotalPrice));
        }
        else
        {
            _items.Add(new CartItemModel
            {
                MenuItem = menuItem,
                Quantity = quantity,
                SpecialInstructions = specialInstructions
            });
        }
    }

    public void RemoveItem(CartItemModel item)
    {
        _items.Remove(item);
    }

    public void UpdateQuantity(CartItemModel item, int quantity)
    {
        if (quantity <= 0)
        {
            RemoveItem(item);
        }
        else
        {
            item.Quantity = quantity;
            this.RaisePropertyChanged(nameof(TotalItems));
            this.RaisePropertyChanged(nameof(TotalPrice));
        }
    }

    public void ClearCart()
    {
        _items.Clear();
    }

    public CreateOrderRequest CreateOrderRequest(int tableNumber)
    {
        var orderItems = _items.Select(item => new CreateOrderItemRequest(
            item.MenuItem.Id,
            item.Quantity,
            item.SpecialInstructions
        )).ToList();

        return new CreateOrderRequest(tableNumber, orderItems);
    }
}