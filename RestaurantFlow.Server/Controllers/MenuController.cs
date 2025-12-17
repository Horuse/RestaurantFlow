using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RestaurantFlow.Server.Services;
using RestaurantFlow.Server.DTOs;
using RestaurantFlow.Server.Hubs;
using RestaurantFlow.Data.Entities;

namespace RestaurantFlow.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    private readonly IMenuService _menuService;
    private readonly IRestaurantNotificationService _notificationService;

    public MenuController(IMenuService menuService, IRestaurantNotificationService notificationService)
    {
        _menuService = menuService;
        _notificationService = notificationService;
    }

    [HttpGet("items")]
    public async Task<ActionResult<List<MenuItemResponse>>> GetMenuItems()
    {
        var items = await _menuService.GetMenuItemsAsync();
        var response = items.Select(item => new MenuItemResponse(
            item.Id,
            item.Name,
            item.Description,
            item.Price,
            item.IsAvailable,
            item.IsCurrentlyAvailable,
            item.EstimatedCookingTimeMinutes,
            item.CategoryId,
            item.Category?.Name ?? "",
            item.Image != null
        )).ToList();

        return Ok(response);
    }

    [HttpGet("categories")]
    public async Task<ActionResult<List<CategoryResponse>>> GetCategories()
    {
        var categories = await _menuService.GetCategoriesAsync();
        var response = categories.Select(cat => new CategoryResponse(
            cat.Id,
            cat.Name,
            cat.Description,
            cat.DisplayOrder
        )).ToList();

        return Ok(response);
    }

    [HttpGet("items/{id}")]
    public async Task<ActionResult<MenuItemResponse>> GetMenuItem(int id)
    {
        var item = await _menuService.GetMenuItemByIdAsync(id);
        if (item == null)
            return NotFound();

        // Перевіряємо доступність для окремого айтема
        item.IsCurrentlyAvailable = item.IsAvailable && await _menuService.IsMenuItemAvailableAsync(item.Id);
        
        var response = new MenuItemResponse(
            item.Id,
            item.Name,
            item.Description,
            item.Price,
            item.IsAvailable,
            item.IsCurrentlyAvailable,
            item.EstimatedCookingTimeMinutes,
            item.CategoryId,
            item.Category?.Name ?? "",
            item.Image != null
        );

        return Ok(response);
    }

    [HttpPost("items")]
    public async Task<ActionResult<MenuItemResponse>> CreateMenuItem([FromBody] CreateMenuItemRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var menuItem = new MenuItem
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            IsAvailable = request.IsAvailable,
            EstimatedCookingTimeMinutes = request.EstimatedCookingTimeMinutes,
            CategoryId = request.CategoryId
        };

        var created = await _menuService.CreateMenuItemAsync(menuItem);
        await _notificationService.NotifyMenuUpdated();

        created.IsCurrentlyAvailable = created.IsAvailable && await _menuService.IsMenuItemAvailableAsync(created.Id);
        
        var response = new MenuItemResponse(
            created.Id,
            created.Name,
            created.Description,
            created.Price,
            created.IsAvailable,
            created.IsCurrentlyAvailable,
            created.EstimatedCookingTimeMinutes,
            created.CategoryId,
            created.Category?.Name ?? "",
            created.Image != null
        );

        return CreatedAtAction(nameof(GetMenuItem), new { id = created.Id }, response);
    }

    [HttpPut("items/{id}")]
    public async Task<ActionResult<MenuItemResponse>> UpdateMenuItem(int id, [FromBody] UpdateMenuItemRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingItem = await _menuService.GetMenuItemByIdAsync(id);
        if (existingItem == null)
            return NotFound();

        existingItem.Name = request.Name;
        existingItem.Description = request.Description;
        existingItem.Price = request.Price;
        existingItem.IsAvailable = request.IsAvailable;
        existingItem.EstimatedCookingTimeMinutes = request.EstimatedCookingTimeMinutes;
        existingItem.CategoryId = request.CategoryId;

        var updated = await _menuService.UpdateMenuItemAsync(existingItem);
        await _notificationService.NotifyMenuUpdated();

        updated.IsCurrentlyAvailable = updated.IsAvailable && await _menuService.IsMenuItemAvailableAsync(updated.Id);

        var response = new MenuItemResponse(
            updated.Id,
            updated.Name,
            updated.Description,
            updated.Price,
            updated.IsAvailable,
            updated.IsCurrentlyAvailable,
            updated.EstimatedCookingTimeMinutes,
            updated.CategoryId,
            updated.Category?.Name ?? "",
            updated.Image != null
        );

        return Ok(response);
    }

    [HttpDelete("items/{id}")]
    public async Task<IActionResult> DeleteMenuItem(int id)
    {
        var existingItem = await _menuService.GetMenuItemByIdAsync(id);
        if (existingItem == null)
            return NotFound();

        await _menuService.DeleteMenuItemAsync(id);
        await _notificationService.NotifyMenuUpdated();

        return NoContent();
    }

    [HttpGet("items/{id}/image")]
    public async Task<IActionResult> GetMenuItemImage(int id)
    {
        var menuItem = await _menuService.GetMenuItemByIdAsync(id);
        if (menuItem?.Image == null)
        {
            return NotFound();
        }

        return File(menuItem.Image, "image/jpeg");
    }
}