using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using DAL.Entities;

namespace ShoppingOnline.Converters
{
    public class FirstImageConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ICollection<ProductImage> images && images.Count > 0)
            {
                var firstImage = images.First();
                if (!string.IsNullOrEmpty(firstImage.ImageUrl))
                {
                    try
                    {
                        string imagePath = firstImage.ImageUrl;
                        if (imagePath.StartsWith("img/"))
                        {
                            string currentDir = Directory.GetCurrentDirectory();
                            string absolutePath = Path.Combine(currentDir, imagePath);
                            
                            if (File.Exists(absolutePath))
                            {
                                return new BitmapImage(new Uri(absolutePath));
                            }
                        }
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
            return null;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class VisibilityConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
