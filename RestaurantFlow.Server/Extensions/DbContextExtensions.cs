using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using RestaurantFlow.Data;
using RestaurantFlow.Data.Entities;
using RestaurantFlow.Shared.Enums;

namespace RestaurantFlow.Server.Extensions;

public static class DbContextExtensions
{
    public static void SeedData(this RestaurantDbContext context)
    {
        // Only seed if database is empty
        if (context.Categories.Any()) return;

        // Seed Categories
        var categories = new[]
        {
            new Category { Name = "Бургери", DisplayOrder = 1 },
            new Category { Name = "Картопля фрі", DisplayOrder = 2 },
            new Category { Name = "Напої", DisplayOrder = 3 },
            new Category { Name = "Десерти", DisplayOrder = 4 }
        };
        
        context.Categories.AddRange(categories);
        context.SaveChanges();

        // Seed Menu Items
        var menuItems = new[]
        {
            // Burgers
            new MenuItem 
            { 
                Name = "Біг Мак", 
                Description = "Два яловичі котлети, спеціальний соус, салат, сир, соління, цибуля на булочці з кунжутом",
                Price = 150.00m, 
                CategoryId = categories[0].Id,
                IsAvailable = true,
                EstimatedCookingTimeMinutes = 8
            },
            new MenuItem 
            { 
                Name = "Чізбургер", 
                Description = "Яловичий котлет, сир, соління, цибуля, кетчуп, гірчиця",
                Price = 80.00m, 
                CategoryId = categories[0].Id,
                IsAvailable = true,
                EstimatedCookingTimeMinutes = 5
            },
            new MenuItem 
            { 
                Name = "Чікен Бургер", 
                Description = "Куряча котлета, майонез, салат, помідор",
                Price = 120.00m, 
                CategoryId = categories[0].Id,
                IsAvailable = true,
                EstimatedCookingTimeMinutes = 6
            },
            
            // Fries
            new MenuItem 
            { 
                Name = "Картопля фрі мала", 
                Description = "Золотиста картопля фрі",
                Price = 35.00m, 
                CategoryId = categories[1].Id,
                IsAvailable = true,
                EstimatedCookingTimeMinutes = 3
            },
            new MenuItem 
            { 
                Name = "Картопля фрі велика", 
                Description = "Золотиста картопля фрі, велика порція",
                Price = 55.00m, 
                CategoryId = categories[1].Id,
                IsAvailable = true,
                EstimatedCookingTimeMinutes = 4
            },
            
            // Drinks
            new MenuItem 
            { 
                Name = "Кока-кола 0,5л", 
                Description = "Освіжаюча кока-кола",
                Price = 30.00m, 
                CategoryId = categories[2].Id,
                IsAvailable = true,
                EstimatedCookingTimeMinutes = 1
            },
            new MenuItem 
            { 
                Name = "Кава", 
                Description = "Ароматна гаряча кава",
                Price = 25.00m, 
                CategoryId = categories[2].Id,
                IsAvailable = true,
                EstimatedCookingTimeMinutes = 2
            },
            
            // Desserts
            new MenuItem 
            { 
                Name = "Морозиво Макфлурі", 
                Description = "Ванільне морозиво з шоколадними крихтами",
                Price = 45.00m, 
                CategoryId = categories[3].Id,
                IsAvailable = true,
                EstimatedCookingTimeMinutes = 3
            }
        };
        
        context.MenuItems.AddRange(menuItems);
        context.SaveChanges();

        // Seed Ingredients
        var ingredients = new[]
        {
            new Ingredient { Name = "Яловичий котлет", Unit = "шт", CurrentStock = 50, MinimumStock = 10, IsActive = true },
            new Ingredient { Name = "Куряча котлета", Unit = "шт", CurrentStock = 30, MinimumStock = 8, IsActive = true },
            new Ingredient { Name = "Сир чедер", Unit = "шт", CurrentStock = 100, MinimumStock = 20, IsActive = true },
            new Ingredient { Name = "Салат", Unit = "г", CurrentStock = 500, MinimumStock = 100, IsActive = true },
            new Ingredient { Name = "Картопля", Unit = "кг", CurrentStock = 25, MinimumStock = 5, IsActive = true },
            new Ingredient { Name = "Булочка з кунжутом", Unit = "шт", CurrentStock = 80, MinimumStock = 15, IsActive = true }
        };
        
        context.Ingredients.AddRange(ingredients);
        context.SaveChanges();

        // Seed Staff
        var staff = new[]
        {
            new Staff 
            { 
                Name = "Олексій Петренко", 
                Role = StaffRole.Cook, 
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Staff 
            { 
                Name = "Марія Іванова", 
                Role = StaffRole.Counter, 
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Staff 
            { 
                Name = "Дмитро Коваленко", 
                Role = StaffRole.Cook, 
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Staff 
            { 
                Name = "Анна Сидорова", 
                Role = StaffRole.Cook, 
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };
        
        context.Staff.AddRange(staff);
        context.SaveChanges();

        // Seed Sample Orders
        var bigMac = menuItems[0];
        var fries = menuItems[3];
        var cola = menuItems[5];

        var orders = new[]
        {
            new Order
            {
                OrderNumber = "A001",
                OrderType = OrderType.DineIn,
                TableNumber = 5,
                Status = OrderStatus.Pending,
                TotalAmount = 215.00m,
                CreatedAt = DateTime.UtcNow.AddMinutes(-5),
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { MenuItemId = bigMac.Id, Quantity = 1, Price = bigMac.Price, Status = OrderStatus.Pending },
                    new OrderItem { MenuItemId = fries.Id, Quantity = 1, Price = fries.Price, Status = OrderStatus.Pending },
                    new OrderItem { MenuItemId = cola.Id, Quantity = 1, Price = cola.Price, Status = OrderStatus.Pending }
                }
            },
            new Order
            {
                OrderNumber = "A002",
                OrderType = OrderType.TakeAway,
                Status = OrderStatus.InProgress,
                TotalAmount = 200.00m,
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { MenuItemId = menuItems[2].Id, Quantity = 1, Price = menuItems[2].Price, Status = OrderStatus.InProgress, StartedCookingAt = DateTime.UtcNow.AddMinutes(-5) },
                    new OrderItem { MenuItemId = menuItems[4].Id, Quantity = 1, Price = menuItems[4].Price, Status = OrderStatus.InProgress, StartedCookingAt = DateTime.UtcNow.AddMinutes(-5) },
                    new OrderItem { MenuItemId = cola.Id, Quantity = 1, Price = cola.Price, Status = OrderStatus.Ready, ReadyAt = DateTime.UtcNow.AddMinutes(-2) }
                }
            },
            new Order
            {
                OrderNumber = "A003",
                OrderType = OrderType.DineIn,
                TableNumber = 2,
                Status = OrderStatus.Ready,
                TotalAmount = 175.00m,
                CreatedAt = DateTime.UtcNow.AddMinutes(-15),
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { MenuItemId = menuItems[1].Id, Quantity = 2, Price = menuItems[1].Price, Status = OrderStatus.Ready, ReadyAt = DateTime.UtcNow.AddMinutes(-3) },
                    new OrderItem { MenuItemId = fries.Id, Quantity = 1, Price = fries.Price, Status = OrderStatus.Ready, ReadyAt = DateTime.UtcNow.AddMinutes(-3) }
                }
            }
        };

        context.Orders.AddRange(orders);
        context.SaveChanges();
    }
}