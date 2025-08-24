using BLL.Services;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
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

namespace ShoppingOnline
{
    /// <summary>
    /// Interaction logic for HomeWindow.xaml
    /// </summary>
    /// 
    //.
    public partial class HomeWindow : Window, INotifyPropertyChanged
    {
        private readonly IProductService _productService;
        private ObservableCollection<Product> _products = new();
        private Product? _mainBannerProduct;
        private ObservableCollection<Product> _smallBannerProducts = new();

        public HomeWindow()
        {
            InitializeComponent();
            _productService = new ProductService();
            // _products already initialized above
            this.DataContext = this;
            LoadProducts();
            UpdateLoginButton();
        }

        public ObservableCollection<Product> Products
        {
            get => _products;
            set
            {
                _products = value;
                OnPropertyChanged(nameof(Products));
            }
        }

        public Product? MainBannerProduct
        {
            get => _mainBannerProduct;
            set
            {
                _mainBannerProduct = value;
                OnPropertyChanged(nameof(MainBannerProduct));
            }
        }

        public ObservableCollection<Product> SmallBannerProducts
        {
            get => _smallBannerProducts;
            set
            {
                _smallBannerProducts = value;
                OnPropertyChanged(nameof(SmallBannerProducts));
            }
        }

        public void LoadProducts()
        {
            try
            {
                // Load products for main grid
                var products = _productService.GetFeaturedProducts(8);
                Products.Clear();
                foreach (var product in products)
                {
                    Products.Add(product);
                }

                // Load random products for banners
                var allProducts = _productService.GetActiveProducts();
                if (allProducts.Count > 0)
                {
                    // Random for main banner
                    var random = new Random();
                    MainBannerProduct = allProducts[random.Next(allProducts.Count)];

                    // Random for small banners (3 different products)
                    SmallBannerProducts.Clear();
                    var availableProducts = allProducts.Where(p => p.ProductId != MainBannerProduct?.ProductId).ToList();
                    for (int i = 0; i < 3 && i < availableProducts.Count; i++)
                    {
                        var randomIndex = random.Next(availableProducts.Count);
                        var selectedProduct = availableProducts[randomIndex];
                        SmallBannerProducts.Add(selectedProduct);
                        availableProducts.RemoveAt(randomIndex);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ProductCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is Product product && !string.IsNullOrEmpty(product.ProductId))
            {
                var detailWindow = new DetailWindow(product.ProductId);
                detailWindow.Show();
            }
        }

        private void TestDetailWindow_Click(object sender, RoutedEventArgs e)
        {
            var detailWindow = new DetailWindow();
            detailWindow.Show();
        }

        private void OpenCart_Click(object sender, RoutedEventArgs e)
        {
            // Check if user is logged in
            if (!UserSession.IsLoggedIn)
            {
                var result = MessageBox.Show("Vui lòng đăng nhập để xem giỏ hàng!\nBạn có muốn đăng nhập ngay không?", 
                    "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    if (UserSession.ShowLoginDialog())
                    {
                        UpdateLoginButton(); // Update login button after successful login
                        var cartWindow = new CartWindow();
                        cartWindow.Show();
                    }
                }
                return;
            }
            
            // User is logged in, open cart
            var cart = new CartWindow();
            cart.Show();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            if (UserSession.IsLoggedIn)
            {
                // Logout
                var result = MessageBox.Show($"Bạn có muốn đăng xuất khỏi tài khoản {UserSession.CustomerName}?", 
                    "Xác nhận đăng xuất", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    UserSession.Logout();
                    UpdateLoginButton();
                    MessageBox.Show("Đã đăng xuất thành công!", "Thông báo", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                // Login
                if (UserSession.ShowLoginDialog())
                {
                    UpdateLoginButton();
                }
            }
        }

        private void UpdateLoginButton()
        {
            if (UserSession.IsLoggedIn)
            {
                LoginButton.Content = $"Xin chào, {UserSession.CustomerName}";
            }
            else
            {
                LoginButton.Content = "Đăng nhập";
            }
        }

        private void MainBanner_Click(object sender, RoutedEventArgs e)
        {
            if (MainBannerProduct != null && !string.IsNullOrEmpty(MainBannerProduct.ProductId))
            {
                var detailWindow = new DetailWindow(MainBannerProduct.ProductId);
                detailWindow.Show();
            }
        }

        private void SmallBanner_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is Product product && !string.IsNullOrEmpty(product.ProductId))
            {
                var detailWindow = new DetailWindow(product.ProductId);
                detailWindow.Show();
            }
        }

        private void SmallBannerButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Product product && !string.IsNullOrEmpty(product.ProductId))
            {
                var detailWindow = new DetailWindow(product.ProductId);
                detailWindow.Show();
            }
        }
    }

    public class PriceToMillionConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is decimal price)
            {
                if (price >= 1000000)
                {
                    return $"Giá từ {price / 1000000:F1} Triệu";
                }
                else
                {
                    return $"Giá từ {price:N0}₫";
                }
            }
            return "Giá liên hệ";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
