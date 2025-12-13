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

        // TODO: Add repositories and services here
        
        services.AddTransient<MainWindowViewModel>();
        
        services.AddSingleton<DialogManager>();
        services.AddSingleton<ToastManager>();
        
        services.AddTransient<IServiceProvider>(sp => sp);
        
        // Initialize database with migrations
        using var scope = services.BuildServiceProvider().CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestaurantDbContext>();
        context.Database.Migrate();
    }

    private void RegisterDialogs(IServiceProvider serviceProvider)
    {
        var dialogManager = serviceProvider.GetRequiredService<DialogManager>();
        // TODO: Register dialogs here when they are created
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