using System.ComponentModel.DataAnnotations;

namespace RestaurantFlow.Server.DTOs;

public record MenuItemResponse(
    int Id,
    string Name,
    string Description,
    decimal Price,
    bool IsAvailable,
    bool IsCurrentlyAvailable,
    int EstimatedCookingTimeMinutes,
    int CategoryId,
    string CategoryName,
    bool HasImage,
    int Calories,
    string Allergens,
    bool IsPopular,
    bool IsRecommended,
    string Ingredients
);

public record CategoryResponse(
    int Id,
    string Name,
    string Description,
    int DisplayOrder
);

public record CreateMenuItemRequest(
    [Required] string Name,
    string Description,
    [Range(0.01, double.MaxValue)] decimal Price,
    bool IsAvailable,
    [Range(1, 300)] int EstimatedCookingTimeMinutes,
    int CategoryId
);

public record UpdateMenuItemRequest(
    [Required] string Name,
    string Description,
    [Range(0.01, double.MaxValue)] decimal Price,
    bool IsAvailable,
    [Range(1, 300)] int EstimatedCookingTimeMinutes,
    int CategoryId
);