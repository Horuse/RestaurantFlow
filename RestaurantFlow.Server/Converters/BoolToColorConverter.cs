using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace RestaurantFlow.Server.Converters;

public class BoolToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isAvailable)
        {
            return isAvailable ? "#28a745" : "#dc3545"; // Зелений для доступного, червоний для недоступного
        }
        
        return "#6c757d"; // Сірий за замовчуванням
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}