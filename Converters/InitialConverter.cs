using System.Globalization;

namespace JigaMultiplatform.Converters;

/// <summary>
/// Converter that extracts the first letter of a string for avatar initials
/// </summary>
public class InitialConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string text && !string.IsNullOrEmpty(text))
        {
            return text[0].ToString().ToUpper();
        }
        
        return "?";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 