using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace MapEditor.WpfShell.Converters
{
    internal class Array2StringConverter : IValueConverter 
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) 
        {
            if (targetType != typeof(string))
            {
                return value;
            }
            return string.Join(Environment.NewLine, (IEnumerable<string>)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) 
        {
            throw new NotImplementedException();
        }
    }
}
