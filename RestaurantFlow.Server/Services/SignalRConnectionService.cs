using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using RestaurantFlow.Server.DTOs;
using RestaurantFlow.Shared.Enums;

namespace RestaurantFlow.Server.Services;

public interface ISignalRConnectionService
{
    event Action<OrderResponse>? NewOrderReceived;
    event Action<OrderStatusUpdate>? OrderStatusChanged;
    Task StartConnectionAsync();
    Task StopConnectionAsync();
    bool IsConnected { get; }
}

public class SignalRConnectionService : ISignalRConnectionService, IDisposable
{
    private HubConnection? _connection;

    public event Action<OrderResponse>? NewOrderReceived;
    public event Action<OrderStatusUpdate>? OrderStatusChanged;

    public bool IsConnected => _connection?.State == HubConnectionState.Connected;

    public async Task StartConnectionAsync()
    {
        if (_connection != null)
            return;

        _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5000/restaurantHub")
            .WithAutomaticReconnect()
            .Build();

        _connection.On<OrderResponse>("NewOrder", (order) =>
        {
            System.Console.WriteLine($"Received new order via SignalR: {order.OrderNumber}");
            NewOrderReceived?.Invoke(order);
        });

        _connection.On<OrderStatusUpdate>("OrderStatusChanged", (update) =>
        {
            System.Console.WriteLine($"Order status changed via SignalR: Order {update.OrderId} -> {update.Status}");
            OrderStatusChanged?.Invoke(update);
        });

        _connection.Reconnecting += (error) =>
        {
            System.Console.WriteLine($"SignalR reconnecting: {error?.Message}");
            return Task.CompletedTask;
        };

        _connection.Reconnected += (connectionId) =>
        {
            System.Console.WriteLine($"SignalR reconnected: {connectionId}");
            return Task.CompletedTask;
        };

        _connection.Closed += (error) =>
        {
            System.Console.WriteLine($"SignalR connection closed: {error?.Message}");
            return Task.CompletedTask;
        };

        try
        {
            await _connection.StartAsync();
            System.Console.WriteLine("SignalR connection started successfully");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error starting SignalR connection: {ex.Message}");
            throw;
        }
    }

    public async Task StopConnectionAsync()
    {
        if (_connection != null)
        {
            await _connection.StopAsync();
            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    public void Dispose()
    {
        _ = StopConnectionAsync();
    }
}