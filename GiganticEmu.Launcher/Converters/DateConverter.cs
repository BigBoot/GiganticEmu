using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace GiganticEmu.Launcher;

public class DateConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime date)
        {
            return date.ToShortDateString();
        }

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime date)
        {
            return date.ToShortDateString();
        }

        return value;
    }
}
