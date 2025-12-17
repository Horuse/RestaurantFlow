using System;
using ReactiveUI;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace RestaurantFlow.Server.ViewModels;

public class ViewModelBase : ReactiveObject, INotifyDataErrorInfo
{
    private readonly Dictionary<string, List<string>> _errors = new();

    public bool HasErrors => _errors.Any();

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public IEnumerable GetErrors(string? propertyName)
    {
        if (propertyName != null && _errors.TryGetValue(propertyName, out var errors))
        {
            return errors;
        }
        return Enumerable.Empty<string>();
    }

    protected void ValidateAllProperties()
    {
        var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(prop => prop.CanRead && prop.GetCustomAttributes<ValidationAttribute>().Any());

        foreach (var property in properties)
        {
            ValidateProperty(property.Name, property.GetValue(this));
        }
    }

    protected void ValidateProperty(string propertyName, object? value)
    {
        var property = GetType().GetProperty(propertyName);
        if (property == null) return;

        var validationAttributes = property.GetCustomAttributes<ValidationAttribute>();
        var errors = validationAttributes
            .Where(attr => !attr.IsValid(value))
            .Select(attr => attr.ErrorMessage ?? $"{propertyName} is not valid")
            .ToList();

        if (errors.Any())
        {
            _errors[propertyName] = errors;
        }
        else
        {
            _errors.Remove(propertyName);
        }

        OnErrorsChanged(propertyName);
    }

    protected void ClearAllErrors()
    {
        var propertyNames = _errors.Keys.ToList();
        _errors.Clear();
        
        foreach (var propertyName in propertyNames)
        {
            OnErrorsChanged(propertyName);
        }
    }

    private void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }
}