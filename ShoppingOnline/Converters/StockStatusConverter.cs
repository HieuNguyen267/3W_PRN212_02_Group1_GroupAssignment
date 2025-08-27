using System;
using System.Globalization;
using System.Windows.Data;

namespace ShoppingOnline.Converters
{
    public class StockStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int stockQuantity)
            {
                if (stockQuantity <= 0)
                    return "Hết hàng";
                else if (stockQuantity <= 5)
                    return $"Còn {stockQuantity} (sắp hết)";
                else
                    return $"Còn {stockQuantity}";
            }
            
            return "N/A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
