using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RestaurantFlow.Shared.Enums;

namespace RestaurantFlow.Server.DTOs;

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

public record CreateOrderRequest(
    [Range(1, 999)] int TableNumber,
    [Required, MinLength(1)] List<CreateOrderItemRequest> Items
);

public record CreateOrderItemRequest(
    int MenuItemId,
    [Range(1, 20)] int Quantity,
    string? SpecialInstructions = null
);

public record UpdateOrderStatusRequest(
    OrderStatus Status
);

public record OrderStatusUpdate(
    int OrderId,
    OrderStatus Status,
    DateTime UpdatedAt
);