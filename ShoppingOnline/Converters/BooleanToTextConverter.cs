using System;
using System.Globalization;
using System.Windows.Data;

namespace ShoppingOnline.Converters
{
    public class BooleanToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "Hoạt động" : "Vô hiệu";
            }
            return "Không xác định";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
