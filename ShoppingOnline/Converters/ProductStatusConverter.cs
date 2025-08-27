using System;
using System.Globalization;
using System.Windows.Data;

namespace ShoppingOnline.Converters
{
    public class ProductStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "N/A";
                
            if (value is bool isActive)
            {
                return isActive ? "Hoạt động" : "Vô hiệu hóa";
            }
            
            // Handle nullable bool by casting
            try
            {
                var nullableBool = (bool?)value;
                return nullableBool.HasValue && nullableBool.Value ? "Hoạt động" : "Vô hiệu hóa";
            }
            catch
            {
                return "N/A";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
