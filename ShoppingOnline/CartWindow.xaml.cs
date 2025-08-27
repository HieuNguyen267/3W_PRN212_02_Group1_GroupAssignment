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
using ShoppingOnline.Converters;
using ShoppingOnline.Constants;


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
                    int newQuantity = item.Quantity + 1;
                    
                    // Validate quantity using CartItemViewModel method
                    if (!item.ValidateQuantity(newQuantity, out string errorMessage))
                    {
                        MessageBox.Show(errorMessage, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    item.Quantity = newQuantity;
                    if (item.CartId > 0)
                    {
                        _cartService.UpdateQuantity(item.CartId, item.Quantity);
                    }
                    CalculateTotals();
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.Message, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        private void QuantityTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is CartItemViewModel item)
            {
                try
                {
                    // Parse the entered quantity
                    if (int.TryParse(textBox.Text, out int newQuantity))
                    {
                        // Validate quantity using CartItemViewModel method
                        if (!item.ValidateQuantity(newQuantity, out string errorMessage))
                        {
                            MessageBox.Show(errorMessage, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                            
                            // Reset to max available stock if exceeding stock
                            if (newQuantity > item.Product.StockQuantity && item.Product.StockQuantity > 0)
                            {
                                item.Quantity = item.Product.StockQuantity;
                            }
                            else
                            {
                                item.Quantity = 1; // Reset to 1 for other invalid cases
                            }
                            return;
                        }

                        // Update quantity if valid
                        if (item.CartId > 0)
                        {
                            _cartService.UpdateQuantity(item.CartId, newQuantity);
                        }
                        CalculateTotals();
                    }
                    else
                    {
                        MessageBox.Show("Vui lòng nhập số lượng hợp lệ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        item.Quantity = 1; // Reset to 1
                    }
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.Message, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    item.Quantity = 1; // Reset to 1
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi cập nhật số lượng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    item.Quantity = 1; // Reset to 1
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

        private void CheckoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CartItemViewModel item)
            {
                var result = MessageBox.Show($"Bạn có muốn thanh toán {item.Product.ProductName} với số lượng {item.Quantity}?", 
                    "Xác nhận thanh toán", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Debug logging
                        System.Diagnostics.Debug.WriteLine($"Creating order for customer: {_customerId}");
                        System.Diagnostics.Debug.WriteLine($"Product: {item.Product.ProductId}, Price: {item.Product.Price}, Quantity: {item.Quantity}");
                        
                        // Create order for this item
                        var order = new Order
                        {
                            CustomerId = _customerId,
                            OrderDate = DateTime.Now,
                            TotalAmount = item.Product.Price * item.Quantity,
                            Status = AppConstants.OrderStatus.Pending,
                            ShippingAddress = "",
                            Phone = ""
                        };

                        var orderService = new OrderService();
                        System.Diagnostics.Debug.WriteLine($"Order created, attempting to save...");
                        if (orderService.CreateOrder(order))
                        {
                            // Create order detail
                            var orderDetail = new OrderDetail
                            {
                                OrderId = order.OrderId,
                                ProductId = item.Product.ProductId,
                                Quantity = item.Quantity,
                                UnitPrice = item.Product.Price,
                                SubTotal = item.Product.Price * item.Quantity
                            };

                            // Add order detail to database
                            orderService.AddOrderDetail(orderDetail);

                            // Remove item from cart
                            if (item.CartId > 0)
                            {
                                _cartService.RemoveFromCart(item.CartId);
                            }
                            CartItems.Remove(item);
                            CalculateTotals();

                            MessageBox.Show($"Đã thanh toán thành công {item.Product.ProductName}!", 
                                "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Failed to create order");
                            MessageBox.Show("Lỗi khi tạo đơn hàng! Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi thanh toán: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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

            var result = MessageBox.Show($"Bạn có muốn thanh toán toàn bộ giỏ hàng ({CartItems.Count} sản phẩm)?", 
                "Xác nhận thanh toán", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var orderService = new OrderService();
                    var totalAmount = CartItems.Sum(item => item.Product.Price * item.Quantity);

                    // Create order for all items
                    var order = new Order
                    {
                        CustomerId = _customerId,
                        OrderDate = DateTime.Now,
                        TotalAmount = totalAmount,
                        Status = AppConstants.OrderStatus.Pending,
                        ShippingAddress = "",
                        Phone = ""
                    };

                    if (orderService.CreateOrder(order))
                    {
                        // Create order details for all items
                        foreach (var item in CartItems.ToList())
                        {
                            var orderDetail = new OrderDetail
                            {
                                OrderId = order.OrderId,
                                ProductId = item.Product.ProductId,
                                Quantity = item.Quantity,
                                UnitPrice = item.Product.Price,
                                SubTotal = item.Product.Price * item.Quantity
                            };

                            orderService.AddOrderDetail(orderDetail);

                            // Remove item from cart
                            if (item.CartId > 0)
                            {
                                _cartService.RemoveFromCart(item.CartId);
                            }
                        }

                        // Clear cart items from UI
                        CartItems.Clear();
                        CalculateTotals();

                        MessageBox.Show($"Đã thanh toán thành công toàn bộ giỏ hàng! Tổng cộng: {totalAmount:N0}₫", 
                            "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Lỗi khi tạo đơn hàng! Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi thanh toán: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    public class CartItemViewModel : INotifyPropertyChanged
    {
        private Product _product = null!;
        private int _quantity;
        private int _cartId;
        private string _stockInfo = "";

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
                UpdateStockInfo();
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                UpdateStockInfo();
            }
        }

        public string StockInfo
        {
            get => _stockInfo;
            set
            {
                _stockInfo = value;
                OnPropertyChanged(nameof(StockInfo));
            }
        }

        private void UpdateStockInfo()
        {
            if (_product != null)
            {
                if (_product.StockQuantity <= 0)
                {
                    StockInfo = "Hết hàng";
                }
                else if (_product.StockQuantity <= 5)
                {
                    StockInfo = $"Còn {_product.StockQuantity} sản phẩm (sắp hết)";
                }
                else
                {
                    StockInfo = $"Còn {_product.StockQuantity} sản phẩm";
                }
            }
        }

        public bool ValidateQuantity(int newQuantity, out string errorMessage)
        {
            errorMessage = "";
            
            if (_product == null)
            {
                errorMessage = "Sản phẩm không tồn tại!";
                return false;
            }

            if (newQuantity <= 0)
            {
                errorMessage = "Số lượng phải lớn hơn 0!";
                return false;
            }

            if (newQuantity > _product.StockQuantity)
            {
                errorMessage = $"Số lượng vượt quá tồn kho! Hiện tại chỉ còn {_product.StockQuantity} sản phẩm trong kho.";
                return false;
            }

            return true;
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
