
using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Food_maui.Converters
{
    public class MultiValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Example: Combine two values into a single string
            if (values.Length == 2 && values[0] is string str1 && values[1] is string str2)
            {
                return $"{str1} {str2}";
            }
            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}