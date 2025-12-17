using System;
using System.ComponentModel;
using System.Windows.Input;
using ReactiveUI;

namespace RestaurantFlow.Client.ViewModels;

public class TableSetupViewModel : ReactiveObject
{
    private string _tableNumber = "1";
    
    public string TableNumber
    {
        get => _tableNumber;
        set => this.RaiseAndSetIfChanged(ref _tableNumber, value);
    }
    
    public ICommand ConfirmCommand { get; }
    
    public event EventHandler<string>? TableConfirmed;
    
    public TableSetupViewModel()
    {
        ConfirmCommand = ReactiveCommand.Create(ConfirmTable);
    }
    
    private void ConfirmTable()
    {
        if (!string.IsNullOrWhiteSpace(TableNumber))
        {
            TableConfirmed?.Invoke(this, TableNumber);
        }
    }
}