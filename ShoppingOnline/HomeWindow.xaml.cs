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
using System.Windows.Controls.Primitives;

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
        private ObservableCollection<Product> _searchSuggestions = new();
        private List<Product> _allProducts = new();

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

        public ObservableCollection<Product> SearchSuggestions
        {
            get => _searchSuggestions;
            set
            {
                _searchSuggestions = value;
                OnPropertyChanged(nameof(SearchSuggestions));
            }
        }

        public void LoadProducts()
        {
            try
            {
                // Load ALL active products for main grid
                var products = _productService.GetActiveProducts();
                _allProducts = products.ToList(); // Store all products for search
                
                Products.Clear();
                foreach (var product in products)
                {
                    Products.Add(product);
                }

                // Load random products for banners
                if (products.Count > 0)
                {
                    // Random for main banner
                    var random = new Random();
                    MainBannerProduct = products[random.Next(products.Count)];

                    // Random for small banners (3 different products)
                    SmallBannerProducts.Clear();
                    var availableProducts = products.Where(p => p.ProductId != MainBannerProduct?.ProductId).ToList();
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

        private void AllProducts_Click(object sender, RoutedEventArgs e)
        {
            LoadProducts();
            SectionTitle.Text = "Tất cả sản phẩm";
        }

        private void Category_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string categoryId)
            {
                LoadProductsByCategory(categoryId);
                
                // Update section title based on category
                var categoryName = GetCategoryName(categoryId);
                SectionTitle.Text = categoryName;
            }
        }

        private string GetCategoryName(string categoryId)
        {
            return categoryId switch
            {
                "CAT001" => "Điện thoại, Tablet",
                "CAT002" => "Laptop",
                "CAT003" => "Âm thanh, Mic thu âm",
                "CAT004" => "Đồng hồ, Camera",
                "CAT005" => "Đồ gia dụng",
                "CAT006" => "Phụ kiện",
                "CAT007" => "PC, Màn hình, Máy in",
                "CAT008" => "Tivi",
                _ => "Sản phẩm"
            };
        }

        private void LoadProductsByCategory(string categoryId)
        {
            try
            {
                // Load products by category
                var products = _productService.GetProductsByCategory(categoryId);
                Products.Clear();
                foreach (var product in products)
                {
                    Products.Add(product);
                }

                // Update banners with random products from the same category
                if (products.Count > 0)
                {
                    var random = new Random();
                    MainBannerProduct = products[random.Next(products.Count)];

                    // Random for small banners (3 different products from same category)
                    SmallBannerProducts.Clear();
                    var availableProducts = products.Where(p => p.ProductId != MainBannerProduct?.ProductId).ToList();
                    for (int i = 0; i < 3 && i < availableProducts.Count; i++)
                    {
                        var randomIndex = random.Next(availableProducts.Count);
                        var selectedProduct = availableProducts[randomIndex];
                        SmallBannerProducts.Add(selectedProduct);
                        availableProducts.RemoveAt(randomIndex);
                    }
                }
                else
                {
                    // If no products in category, clear banners
                    MainBannerProduct = null;
                    SmallBannerProducts.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải sản phẩm theo danh mục: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchComboBox_DropDownOpened(object sender, EventArgs e)
        {
            var searchText = SearchComboBox.Text?.Trim();
            
            if (string.IsNullOrEmpty(searchText) || searchText == "Q Bạn muốn mua gì hôm nay?")
            {
                SearchSuggestions.Clear();
                return;
            }

            // Filter products based on search text (product name OR category name)
            var suggestions = _allProducts
                .Where(p => p.ProductName.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                           (p.Category?.CategoryName?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true) ||
                           // Smart search for common keywords
                           (searchText.ToLower() == "điện" && p.Category?.CategoryName?.Contains("Điện thoại", StringComparison.OrdinalIgnoreCase) == true) ||
                           (searchText.ToLower() == "laptop" && p.Category?.CategoryName?.Contains("Laptop", StringComparison.OrdinalIgnoreCase) == true) ||
                           (searchText.ToLower() == "âm thanh" && p.Category?.CategoryName?.Contains("Âm thanh", StringComparison.OrdinalIgnoreCase) == true) ||
                           (searchText.ToLower() == "đồng hồ" && p.Category?.CategoryName?.Contains("Đồng hồ", StringComparison.OrdinalIgnoreCase) == true) ||
                           (searchText.ToLower() == "gia dụng" && p.Category?.CategoryName?.Contains("Đồ gia dụng", StringComparison.OrdinalIgnoreCase) == true) ||
                           (searchText.ToLower() == "phụ kiện" && p.Category?.CategoryName?.Contains("Phụ kiện", StringComparison.OrdinalIgnoreCase) == true) ||
                           (searchText.ToLower() == "pc" && p.Category?.CategoryName?.Contains("PC", StringComparison.OrdinalIgnoreCase) == true) ||
                           (searchText.ToLower() == "tivi" && p.Category?.CategoryName?.Contains("Tivi", StringComparison.OrdinalIgnoreCase) == true))
                .Take(10) // Limit to 10 suggestions
                .ToList();

            SearchSuggestions.Clear();
            foreach (var product in suggestions)
            {
                SearchSuggestions.Add(product);
            }
        }

        private void SearchComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchComboBox.SelectedItem is Product selectedProduct)
            {
                // Open detail window for selected product
                var detailWindow = new DetailWindow(selectedProduct.ProductId);
                detailWindow.Show();
                
                // Reset search
                SearchComboBox.SelectedItem = null;
                SearchComboBox.Text = "Q Bạn muốn mua gì hôm nay?";
                SearchSuggestions.Clear();
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            var searchText = SearchComboBox.Text?.Trim();
            
            if (string.IsNullOrEmpty(searchText) || searchText == "Q Bạn muốn mua gì hôm nay?")
            {
                // If no search text, show all products
                LoadProducts();
                SectionTitle.Text = "Tất cả sản phẩm";
                return;
            }

            try
            {
                // Search products by name OR category name
                var searchResults = _allProducts
                    .Where(p => p.ProductName.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                               (p.Category?.CategoryName?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true) ||
                               // Smart search for common keywords
                               (searchText.ToLower() == "điện" && p.Category?.CategoryName?.Contains("Điện thoại", StringComparison.OrdinalIgnoreCase) == true) ||
                               (searchText.ToLower() == "laptop" && p.Category?.CategoryName?.Contains("Laptop", StringComparison.OrdinalIgnoreCase) == true) ||
                               (searchText.ToLower() == "âm thanh" && p.Category?.CategoryName?.Contains("Âm thanh", StringComparison.OrdinalIgnoreCase) == true) ||
                               (searchText.ToLower() == "đồng hồ" && p.Category?.CategoryName?.Contains("Đồng hồ", StringComparison.OrdinalIgnoreCase) == true) ||
                               (searchText.ToLower() == "gia dụng" && p.Category?.CategoryName?.Contains("Đồ gia dụng", StringComparison.OrdinalIgnoreCase) == true) ||
                               (searchText.ToLower() == "phụ kiện" && p.Category?.CategoryName?.Contains("Phụ kiện", StringComparison.OrdinalIgnoreCase) == true) ||
                               (searchText.ToLower() == "pc" && p.Category?.CategoryName?.Contains("PC", StringComparison.OrdinalIgnoreCase) == true) ||
                               (searchText.ToLower() == "tivi" && p.Category?.CategoryName?.Contains("Tivi", StringComparison.OrdinalIgnoreCase) == true))
                    .ToList();

                // Update products display
                Products.Clear();
                foreach (var product in searchResults)
                {
                    Products.Add(product);
                }

                // Update section title
                SectionTitle.Text = $"Kết quả tìm kiếm: '{searchText}' ({searchResults.Count} sản phẩm)";

                // Update banners with search results
                if (searchResults.Count > 0)
                {
                    var random = new Random();
                    MainBannerProduct = searchResults[random.Next(searchResults.Count)];

                    // Random for small banners (3 different products from search results)
                    SmallBannerProducts.Clear();
                    var availableProducts = searchResults.Where(p => p.ProductId != MainBannerProduct?.ProductId).ToList();
                    for (int i = 0; i < 3 && i < availableProducts.Count; i++)
                    {
                        var randomIndex = random.Next(availableProducts.Count);
                        var selectedProduct = availableProducts[randomIndex];
                        SmallBannerProducts.Add(selectedProduct);
                        availableProducts.RemoveAt(randomIndex);
                    }
                }
                else
                {
                    // If no search results, clear banners
                    MainBannerProduct = null;
                    SmallBannerProducts.Clear();
                }

                // Clear search suggestions
                SearchSuggestions.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm kiếm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
