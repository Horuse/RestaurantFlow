using System;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Timers;

namespace RestaurantFlow.Client.ViewModels;

public partial class OrderSuccessViewModel : ViewModelBase, IDisposable
{
    private readonly Timer _timer;
    
    [Reactive]
    private string _orderNumber = "";
    
    [Reactive]
    private int _secondsRemaining = 10;

    public ReactiveCommand<Unit, Unit> BackToMenuCommand { get; }

    public event Action? TimerExpired;
    public event Action? BackToMenuRequested;

    public OrderSuccessViewModel()
    {
        BackToMenuCommand = ReactiveCommand.Create(BackToMenu);

        _timer = new Timer(1000);
        _timer.Elapsed += OnTimerElapsed;
    }

    public void StartTimer(string orderNumber)
    {
        OrderNumber = orderNumber;
        SecondsRemaining = 10;
        _timer.Start();
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        SecondsRemaining--;
        
        if (SecondsRemaining <= 0)
        {
            _timer.Stop();
            TimerExpired?.Invoke();
        }
    }

    private void BackToMenu()
    {
        _timer.Stop();
        BackToMenuRequested?.Invoke();
    }

    public void Dispose()
    {
        _timer?.Stop();
        _timer?.Dispose();
    }
}