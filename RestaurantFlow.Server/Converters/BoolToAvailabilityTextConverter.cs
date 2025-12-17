using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace RestaurantFlow.Server.Converters;

public class BoolToAvailabilityTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isAvailable)
        {
            return isAvailable ? "Доступно" : "Недоступно";
        }
        
        return "Невідомо";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}