
using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Food_maui.Converters
{
    public class MultiplyConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && 
                double.TryParse(values[0]?.ToString(), out double originalPrice) && 
                double.TryParse(values[1]?.ToString(), out double itemQty))
            {
                return (originalPrice * itemQty).ToString("F3");
            }
            return "0.000";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}