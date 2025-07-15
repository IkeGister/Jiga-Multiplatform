using System.Globalization;

namespace JigaMultiplatform.Converters;

/// <summary>
/// Converter that inverts boolean values for visibility binding
/// </summary>
public class InverseBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        
        return true; // Default to true (visible) if not a boolean
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        
        return false;
    }
} 