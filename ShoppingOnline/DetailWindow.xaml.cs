using BLL.Services;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Globalization;

namespace ShoppingOnline
{
    /// <summary>
    /// Interaction logic for DetailWindow.xaml
    /// </summary>
    public partial class DetailWindow : Window, INotifyPropertyChanged
    {
        private readonly IProductService _productService;
        private Product? _currentProduct;
        private string _selectedVersion = string.Empty;
        private string _selectedColor = string.Empty;

        public DetailWindow()
        {
            InitializeComponent();
            _productService = new ProductService();
            this.DataContext = this;
            LoadSampleProduct();
        }

        public DetailWindow(string productId)
        {
            InitializeComponent();
            _productService = new ProductService();
            this.DataContext = this;
            LoadProduct(productId);
        }

        public Product? CurrentProduct
        {
            get => _currentProduct;
            set
            {
                _currentProduct = value;
                OnPropertyChanged(nameof(CurrentProduct));
            }
        }

        public string SelectedVersion
        {
            get => _selectedVersion;
            set
            {
                _selectedVersion = value;
                OnPropertyChanged(nameof(SelectedVersion));
            }
        }

        public string SelectedColor
        {
            get => _selectedColor;
            set
            {
                _selectedColor = value;
                OnPropertyChanged(nameof(SelectedColor));
            }
        }

        private void LoadProduct(string productId)
        {
            try
            {
                var product = _productService.GetProductById(productId);
                if (product != null)
                {
                    CurrentProduct = product;
                }
                else
                {
                    MessageBox.Show("Không tìm thấy sản phẩm!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải thông tin sản phẩm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private void LoadSampleProduct()
        {
            // Create a sample product for demo purposes
            CurrentProduct = new Product
            {
                ProductId = "P001",
                ProductName = "Samsung Galaxy Tab A9+ WIFI 4GB 64GB",
                Description = "Máy tính bảng Samsung Galaxy Tab A9+ với màn hình 11 inch, chip Snapdragon 695",
                Price = 4290000,
                Category = new Category { CategoryName = "Tablet" }
            };
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check if user is logged in
                if (!UserSession.IsLoggedIn)
                {
                    var result = MessageBox.Show("Bạn cần đăng nhập để thêm sản phẩm vào giỏ hàng!\nBạn có muốn đăng nhập ngay bây giờ?", 
                        "Yêu cầu đăng nhập", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        if (!UserSession.ShowLoginDialog())
                        {
                            return; // User cancelled login
                        }
                    }
                    else
                    {
                        return; // User doesn't want to login
                    }
                }

                if (CurrentProduct != null && UserSession.IsLoggedIn)
                {
                    // Add to cart using CartService
                    var cartService = new CartService();
                    cartService.AddToCart(UserSession.CustomerId!.Value, CurrentProduct.ProductId, 1);
                    
                    var productName = CurrentProduct.ProductName ?? "Sản phẩm";
                    MessageBox.Show($"Đã thêm {productName} vào giỏ hàng!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Open cart window to show the added item
                    var cartWindow = new CartWindow();
                    cartWindow.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm vào giỏ hàng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Favorite_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Đã thêm vào danh sách yêu thích!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void QandA_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tính năng Hỏi đáp sẽ được phát triển sau!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Specifications_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tính năng Thông số kỹ thuật sẽ được phát triển sau!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Compare_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tính năng So sánh sẽ được phát triển sau!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Converters
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
                        return new BitmapImage(new Uri(firstImage.ImageUrl));
                    }
                    catch
                    {
                        // Return null if image loading fails
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
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
