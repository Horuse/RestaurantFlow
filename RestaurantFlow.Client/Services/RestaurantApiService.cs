using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using RestaurantFlow.Client.Models;
using RestaurantFlow.Shared.Enums;

namespace RestaurantFlow.Client.Services;

public class RestaurantApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public RestaurantApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<List<MenuItemModel>> GetMenuItemsAsync()
    {
        var response = await _httpClient.GetAsync("api/menu/items");
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var items = JsonSerializer.Deserialize<List<MenuItemModel>>(content, _jsonOptions);
        return items ?? new List<MenuItemModel>();
    }

    public async Task<List<CategoryModel>> GetCategoriesAsync()
    {
        var response = await _httpClient.GetAsync("api/menu/categories");
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var categories = JsonSerializer.Deserialize<List<CategoryModel>>(content, _jsonOptions);
        return categories ?? new List<CategoryModel>();
    }

    public async Task<OrderResponse?> CreateOrderAsync(CreateOrderRequest request)
    {
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync("api/orders", content);
        response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<OrderResponse>(responseContent, _jsonOptions);
    }
}

public record CreateOrderRequest(
    [Range(1, 999)] int TableNumber,
    [Required, MinLength(1)] List<CreateOrderItemRequest> Items
);

public record CreateOrderItemRequest(
    int MenuItemId,
    [Range(1, 20)] int Quantity,
    string? SpecialInstructions = null
);

public record OrderResponse(
    int Id,
    string OrderNumber,
    OrderStatus Status,
    int TableNumber,
    decimal TotalAmount,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    List<OrderItemResponse> Items
);

public record OrderItemResponse(
    int Id,
    int MenuItemId,
    string MenuItemName,
    int Quantity,
    decimal Price,
    OrderStatus Status,
    string? SpecialInstructions,
    DateTime? StartedCookingAt,
    DateTime? ReadyAt
);