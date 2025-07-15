using System.Globalization;

namespace JigaMultiplatform.Converters;

/// <summary>
/// Converter that converts boolean online status to status indicator colors
/// </summary>
public class StatusColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isOnline)
        {
            return isOnline ? Color.FromArgb("#00ff88") : Color.FromArgb("#ff4444");
        }
        
        return Color.FromArgb("#666666"); // Default gray for unknown status
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 