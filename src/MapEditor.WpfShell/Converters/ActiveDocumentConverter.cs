using MapEditor.WpfShell.ViewModels;
using System;
using System.Globalization;
using System.Windows.Data;

namespace MapEditor.WpfShell.Converters
{
    /// <summary>
    /// Source: https://github.com/Dirkster99/AvalonDock/blob/master/source/MVVMTestApp/Converters/ActiveDocumentConverter.cs
    /// </summary>
    internal class ActiveDocumentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MapEditorViewModel)
            {
                return value;
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MapEditorViewModel)
            {
                return value;
            }
            return Binding.DoNothing;
        }
    }
}
