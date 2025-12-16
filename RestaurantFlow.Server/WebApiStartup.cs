using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RestaurantFlow.Data;
using RestaurantFlow.Server.Services;
using RestaurantFlow.Server.Repositories;
using RestaurantFlow.Server.Hubs;

namespace RestaurantFlow.Server;

public class WebApiStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Add controllers and API services
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        
        // Add SignalR
        services.AddSignalR();
        
        // Add CORS
        services.AddCors(options =>
        {
            options.AddPolicy("RestaurantFlowClient", builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        // Database
        services.AddDbContext<RestaurantDbContext>(options =>
            options.UseSqlite("Data Source=restaurant.db"));

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IMenuRepository, MenuRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IStaffRepository, StaffRepository>();
        services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();

        // Services
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IStaffService, StaffService>();

        // Notification service
        services.AddScoped<IRestaurantNotificationService, RestaurantNotificationService>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseCors("RestaurantFlowClient");
        app.UseRouting();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHub<RestaurantApiHub>("/restaurantHub");
        });
    }
}