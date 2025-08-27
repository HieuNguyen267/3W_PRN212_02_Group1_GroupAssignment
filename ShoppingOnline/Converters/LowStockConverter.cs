using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ShoppingOnline.Converters
{
    public class LowStockConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int stockQuantity)
            {
                if (stockQuantity <= 0)
                    return new SolidColorBrush(Colors.Red);
                else if (stockQuantity <= 5)
                    return new SolidColorBrush(Colors.Orange);
                else
                    return new SolidColorBrush(Colors.Green);
            }
            
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
