using System;
using System.ComponentModel.DataAnnotations;

namespace RestaurantFlow.Server.DTOs;

public record IngredientResponse(
    int Id,
    string Name,
    string Unit,
    decimal CurrentStock,
    decimal MinimumStock,
    bool IsActive,
    bool IsLowStock
);

public record UpdateStockRequest(
    [Range(-9999, 9999)] decimal Quantity,
    [Required] string Reason
);

public record CreateIngredientRequest(
    [Required] string Name,
    [Required] string Unit,
    [Range(0, 9999)] decimal CurrentStock,
    [Range(0, 9999)] decimal MinimumStock
);

public record UpdateIngredientRequest(
    [Required] string Name,
    [Required] string Unit,
    [Range(0, 9999)] decimal CurrentStock,
    [Range(0, 9999)] decimal MinimumStock
);

public record InventoryLogResponse(
    int Id,
    int IngredientId,
    string IngredientName,
    decimal QuantityChanged,
    decimal StockAfter,
    string Reason,
    DateTime CreatedAt
);