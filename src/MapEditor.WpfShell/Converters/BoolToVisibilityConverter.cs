using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MapEditor.WpfShell.Converters 
{
    /// <summary>
    /// Source: https://github.com/Dirkster99/AvalonDock/blob/master/source/MVVMTestApp/Converters/BoolToVisibilityConverter.cs
	/// Implements a Boolean to Visibility converter
	/// Use ConverterParameter=true to negate the visibility - boolean interpretation.
	/// </summary>
    [ValueConversion(typeof(Boolean), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool IsInverted;
            if (parameter == null)
            {
                IsInverted = false;
            }
            else 
            {
                bool.TryParse(parameter.ToString(), out IsInverted);
            }
            bool IsVisible = value == null ? false : (bool)value;
            if (IsVisible)
            {
                return IsInverted ? Visibility.Hidden : Visibility.Visible;
            }
            else
            {
                return IsInverted ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visiblility = value == null ? Visibility.Hidden : (Visibility)value;
            bool IsInverted;
            if (parameter == null)
            {
                IsInverted = false;
            }
            else
            {
                bool.TryParse(parameter.ToString(), out IsInverted);
            }
            return (visiblility == Visibility.Visible) != IsInverted;
        }
    }
}
