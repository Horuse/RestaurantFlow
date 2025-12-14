using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using RestaurantFlow.Data;
using RestaurantFlow.Server.ViewModels;
using RestaurantFlow.Server.Views;
using RestaurantFlow.Server.Extensions;
using ShadUI;
using Splat;

namespace RestaurantFlow.Server;

public partial class App : Application
{
    public IServiceProvider? ServiceProvider { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        
        RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
        Locator.CurrentMutable.RegisterConstant(new AvaloniaActivationForViewFetcher(), typeof(IActivationForViewFetcher));
        Locator.CurrentMutable.RegisterConstant(new AutoDataTemplateBindingHook(), typeof(IPropertyBindingHook));
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();
        
        RegisterDialogs(ServiceProvider);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();
            
            var mainViewModel = ServiceProvider.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel,
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureServices(ServiceCollection services)
    {
        services.AddDbContext<RestaurantDbContext>(options =>
            options.UseSqlite("Data Source=restaurant.db"));
        
        services.AddScoped<RestaurantFlow.Server.Services.IOrderService, RestaurantFlow.Server.Services.OrderService>();
        services.AddScoped<RestaurantFlow.Server.Services.IMenuService, RestaurantFlow.Server.Services.MenuService>();
        services.AddScoped<RestaurantFlow.Server.Services.IInventoryService, RestaurantFlow.Server.Services.InventoryService>();
        services.AddScoped<RestaurantFlow.Server.Services.IStaffService, RestaurantFlow.Server.Services.StaffService>();
        
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<KitchenViewModel>();
        services.AddTransient<CounterViewModel>();
        services.AddTransient<MenuViewModel>();
        services.AddTransient<InventoryViewModel>();
        services.AddTransient<AnalyticsViewModel>();
        
        services.AddTransient<ViewModels.Menu.AddMenuItemViewModel>();
        
        services.AddSingleton<DialogManager>();
        services.AddSingleton<ToastManager>();
        
        services.AddTransient<IServiceProvider>(sp => sp);
        
        using var scope = services.BuildServiceProvider().CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestaurantDbContext>();
        context.Database.Migrate();
        context.SeedData();
    }

    private void RegisterDialogs(IServiceProvider serviceProvider)
    {
        var dialogManager = serviceProvider.GetRequiredService<DialogManager>();
        dialogManager.Register<Views.Menu.AddMenuItemDialog, ViewModels.Menu.AddMenuItemViewModel>();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}