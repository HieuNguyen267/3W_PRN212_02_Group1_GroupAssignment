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
using ShoppingOnline.Converters;


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

        private string _stockStatus = "";
        public string StockStatus
        {
            get => _stockStatus;
            set
            {
                _stockStatus = value;
                OnPropertyChanged(nameof(StockStatus));
            }
        }

        private string _stockMessage = "";
        public string StockMessage
        {
            get => _stockMessage;
            set
            {
                _stockMessage = value;
                OnPropertyChanged(nameof(StockMessage));
            }
        }

        private bool _canAddToCart = true;
        public bool CanAddToCart
        {
            get => _canAddToCart;
            set
            {
                _canAddToCart = value;
                OnPropertyChanged(nameof(CanAddToCart));
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
                    UpdateStockInformation(product);
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

        private void UpdateStockInformation(Product product)
        {
            if (product.StockQuantity <= 0)
            {
                StockStatus = "HẾT HÀNG";
                StockMessage = "Sản phẩm hiện tại đã hết hàng, vui lòng chọn sản phẩm khác.";
                CanAddToCart = false;
            }
            else if (product.StockQuantity <= 5)
            {
                StockStatus = $"CÒN {product.StockQuantity} SẢN PHẨM";
                StockMessage = "Sản phẩm sắp hết hàng, hãy đặt hàng sớm!";
                CanAddToCart = true;
            }
            else
            {
                StockStatus = "CÒN HÀNG";
                StockMessage = $"Còn {product.StockQuantity} sản phẩm trong kho.";
                CanAddToCart = true;
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
                    // Check stock before adding to cart
                    if (CurrentProduct.StockQuantity <= 0)
                    {
                        MessageBox.Show("Sản phẩm đã hết hàng, vui lòng chọn sản phẩm khác!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

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
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
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


}
