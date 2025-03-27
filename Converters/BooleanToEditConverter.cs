
using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace Food_maui.Converters
{
    public class BooleanToEditConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isEditable)
            {
                return isEditable;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}