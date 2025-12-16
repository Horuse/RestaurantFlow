using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RestaurantFlow.Server.Services;
using RestaurantFlow.Server.DTOs;
using RestaurantFlow.Server.Hubs;
using RestaurantFlow.Data.Entities;
using RestaurantFlow.Shared.Enums;

namespace RestaurantFlow.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IMenuService _menuService;
    private readonly IRestaurantNotificationService _notificationService;

    public OrdersController(
        IOrderService orderService, 
        IMenuService menuService,
        IRestaurantNotificationService notificationService)
    {
        _orderService = orderService;
        _menuService = menuService;
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<List<OrderResponse>>> GetOrders([FromQuery] OrderStatus? status = null)
    {
        var orders = status.HasValue 
            ? await _orderService.GetOrdersByStatusAsync(status.Value)
            : await _orderService.GetAllOrdersAsync();

        var response = orders.Select(MapToOrderResponse).ToList();
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderResponse>> GetOrder(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
            return NotFound();

        return Ok(MapToOrderResponse(order));
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            System.Console.WriteLine($"Received order request for table {request.TableNumber} with {request.Items.Count} items");
            
            if (!ModelState.IsValid)
            {
                System.Console.WriteLine("Model state is invalid");
                return BadRequest(ModelState);
            }

            foreach (var item in request.Items)
            {
                System.Console.WriteLine($"Validating menu item {item.MenuItemId}");
                var menuItem = await _menuService.GetMenuItemByIdAsync(item.MenuItemId);
                if (menuItem == null)
                {
                    System.Console.WriteLine($"Menu item {item.MenuItemId} not found");
                    return BadRequest($"Menu item {item.MenuItemId} not found");
                }
                if (!menuItem.IsAvailable)
                {
                    System.Console.WriteLine($"Menu item {menuItem.Name} is not available");
                    return BadRequest($"Menu item {menuItem.Name} is not available");
                }
                System.Console.WriteLine($"Menu item {menuItem.Name} validated successfully");
            }

            System.Console.WriteLine("Creating order entity");
            var order = new Order
            {
                TableNumber = request.TableNumber,
                OrderType = OrderType.DineIn,
                Status = OrderStatus.Pending,
                PaymentMethod = PaymentMethod.Cash,
                CreatedAt = DateTime.UtcNow,
                PaidAt = DateTime.UtcNow,
                OrderItems = new List<OrderItem>()
            };

            decimal totalAmount = 0;
            foreach (var item in request.Items)
            {
                var menuItem = await _menuService.GetMenuItemByIdAsync(item.MenuItemId);
                var orderItem = new OrderItem
                {
                    MenuItemId = item.MenuItemId,
                    Quantity = item.Quantity,
                    Price = menuItem!.Price,
                    Status = OrderStatus.Pending,
                    SpecialRequests = item.SpecialInstructions ?? string.Empty,
                    CreatedAt = DateTime.UtcNow
                };
                
                order.OrderItems.Add(orderItem);
                totalAmount += orderItem.Price * orderItem.Quantity;
                System.Console.WriteLine($"Added order item: {menuItem.Name} x{item.Quantity}");
            }

            order.TotalAmount = totalAmount;
            order.OrderNumber = $"ORD-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
            System.Console.WriteLine($"Order created with number {order.OrderNumber}, total: {totalAmount}");

            System.Console.WriteLine("Saving order to database");
            var created = await _orderService.CreateOrderAsync(order);
            System.Console.WriteLine($"Order saved with ID {created.Id}");
            
            var response = MapToOrderResponse(created);
            
            await _notificationService.NotifyNewOrder(response);

            return CreatedAtAction(nameof(GetOrder), new { id = created.Id }, response);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error creating order: {ex.Message}");
            System.Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
            return NotFound();

        await _orderService.UpdateOrderStatusAsync(id, request.Status);

        var statusUpdate = new OrderStatusUpdate(id, request.Status, DateTime.UtcNow);
        await _notificationService.NotifyOrderStatusChanged(statusUpdate);

        return NoContent();
    }

    [HttpPut("items/{orderItemId}/status")]
    public async Task<IActionResult> UpdateOrderItemStatus(int orderItemId, [FromBody] UpdateOrderStatusRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _orderService.UpdateOrderItemStatusAsync(orderItemId, request.Status);

        return NoContent();
    }

    private static OrderResponse MapToOrderResponse(Order order)
    {
        return new OrderResponse(
            order.Id,
            order.OrderNumber,
            order.Status,
            order.TableNumber ?? 0,
            order.TotalAmount,
            order.CreatedAt,
            order.CompletedAt,
            order.OrderItems?.Select(item => new OrderItemResponse(
                item.Id,
                item.MenuItemId,
                item.MenuItem?.Name ?? "",
                item.Quantity,
                item.Price,
                item.Status,
                item.SpecialRequests,
                item.StartedCookingAt,
                item.ReadyAt
            )).ToList() ?? new List<OrderItemResponse>()
        );
    }
}