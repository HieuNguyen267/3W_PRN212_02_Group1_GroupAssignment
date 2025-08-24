using BLL.Services;
using DAL.Entities;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace ShoppingOnline
{
    public partial class CartWindow : Window, INotifyPropertyChanged
    {
        private readonly CartService _cartService;
        private readonly IProductService _productService;
        private ObservableCollection<CartItemViewModel> _cartItems = new();
        private decimal _subTotal;
        private decimal _shippingFee;
        private decimal _discount;
        private decimal _total;
        private int _customerId;

        public CartWindow()
        {
            InitializeComponent();
            _cartService = new CartService();
            _productService = new ProductService();
            this.DataContext = this;
            
            // Check if user is logged in
            if (!UserSession.IsLoggedIn || UserSession.CustomerId == null)
            {
                MessageBox.Show("Vui lòng đăng nhập để xem giỏ hàng!", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                this.Close();
                return;
            }
            
            _customerId = UserSession.CustomerId.Value;
            
            // Debug: Show which customer ID is being used
            Console.WriteLine($"CartWindow: Loading cart for CustomerId = {_customerId}");
            Console.WriteLine($"UserSession.IsLoggedIn = {UserSession.IsLoggedIn}");
            Console.WriteLine($"UserSession.CustomerName = {UserSession.CustomerName}");
            
            LoadCartItems();
            CalculateTotals();
        }

        public ObservableCollection<CartItemViewModel> CartItems
        {
            get => _cartItems;
            set
            {
                _cartItems = value;
                OnPropertyChanged(nameof(CartItems));
                OnPropertyChanged(nameof(HasItems));
            }
        }

        public decimal SubTotal
        {
            get => _subTotal;
            set
            {
                _subTotal = value;
                OnPropertyChanged(nameof(SubTotal));
            }
        }

        public decimal ShippingFee
        {
            get => _shippingFee;
            set
            {
                _shippingFee = value;
                OnPropertyChanged(nameof(ShippingFee));
            }
        }

        public decimal Discount
        {
            get => _discount;
            set
            {
                _discount = value;
                OnPropertyChanged(nameof(Discount));
            }
        }

        public decimal Total
        {
            get => _total;
            set
            {
                _total = value;
                OnPropertyChanged(nameof(Total));
            }
        }

        public bool HasItems => CartItems.Count > 0;

        public void LoadCartItems()
        {
            try
            {
                CartItems.Clear();
                
                // Load from database
                var cartItems = _cartService.GetCartItems(_customerId);
                
                foreach (var item in cartItems)
                {
                    if (item.Product != null)
                    {
                        CartItems.Add(new CartItemViewModel
                        {
                            CartId = item.CartId,
                            Product = item.Product,
                            Quantity = item.Quantity
                        });
                    }
                }

                // No demo items - only show real cart items from database
                // If no items, cart will be empty
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải giỏ hàng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void CalculateTotals()
        {
            try
            {
                // Always use CartService for real data from database
                SubTotal = _cartService.CalculateSubTotal(_customerId);
                ShippingFee = _cartService.CalculateShippingFee(SubTotal);
                Discount = _cartService.CalculateDiscount(SubTotal);
                Total = _cartService.CalculateTotal(_customerId);
            }
            catch (Exception ex)
            {
                // If database fails, set all to 0
                SubTotal = 0;
                ShippingFee = 0;
                Discount = 0;
                Total = 0;
                
                MessageBox.Show($"Lỗi khi tính toán giỏ hàng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void BackToHome_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ContinueShopping_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CartItemViewModel item)
            {
                try
                {
                    item.Quantity++;
                    if (item.CartId > 0)
                    {
                        _cartService.UpdateQuantity(item.CartId, item.Quantity);
                    }
                    CalculateTotals();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi cập nhật số lượng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CartItemViewModel item)
            {
                if (item.Quantity > 1)
                {
                    try
                    {
                        item.Quantity--;
                        if (item.CartId > 0)
                        {
                            _cartService.UpdateQuantity(item.CartId, item.Quantity);
                        }
                        CalculateTotals();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi cập nhật số lượng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CartItemViewModel item)
            {
                var result = MessageBox.Show($"Bạn có chắc muốn xóa {item.Product.ProductName} khỏi giỏ hàng?", 
                    "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (item.CartId > 0)
                        {
                            _cartService.RemoveFromCart(item.CartId);
                        }
                        CartItems.Remove(item);
                        CalculateTotals();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi xóa sản phẩm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void Checkout_Click(object sender, RoutedEventArgs e)
        {
            if (CartItems.Count == 0)
            {
                MessageBox.Show("Giỏ hàng trống!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show("Chức năng thanh toán sẽ được phát triển trong phiên bản tiếp theo!", 
                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public class CartItemViewModel : INotifyPropertyChanged
    {
        private Product _product = null!;
        private int _quantity;
        private int _cartId;

        public int CartId
        {
            get => _cartId;
            set
            {
                _cartId = value;
                OnPropertyChanged(nameof(CartId));
            }
        }

        public Product Product
        {
            get => _product;
            set
            {
                _product = value;
                OnPropertyChanged(nameof(Product));
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class EmptyCartVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
