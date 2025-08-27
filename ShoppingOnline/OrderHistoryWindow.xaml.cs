using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DAL.Entities;
using BLL.Services;

namespace ShoppingOnline
{
    public partial class OrderHistoryWindow : Window, INotifyPropertyChanged
    {
        private readonly OrderService _orderService;
        private readonly int _customerId;

        public ObservableCollection<OrderViewModel> Orders { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public OrderHistoryWindow(int customerId)
        {
            InitializeComponent();
            _orderService = new OrderService();
            _customerId = customerId;
            Orders = new ObservableCollection<OrderViewModel>();
            this.DataContext = this;
            
            LoadOrderHistory();
        }

        private void LoadOrderHistory()
        {
            try
            {
                var orders = _orderService.GetPaidOrdersByCustomer(_customerId);
                
                Orders.Clear();
                foreach (var order in orders)
                {
                    Orders.Add(new OrderViewModel(order));
                }

                // Show/hide empty state
                if (Orders.Count == 0)
                {
                    OrdersItemsControl.Visibility = Visibility.Collapsed;
                    EmptyStateText.Visibility = Visibility.Visible;
                }
                else
                {
                    OrdersItemsControl.Visibility = Visibility.Visible;
                    EmptyStateText.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải lịch sử đơn hàng: {ex.Message}", 
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
        private void CancelOrder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int orderId)
            {
                var result = MessageBox.Show(
                    "Bạn có chắc chắn muốn hủy đơn hàng này không?",
                    "Xác nhận hủy đơn hàng",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"Attempting to cancel order ID: {orderId}");
                        
                        // Update order status to "Cancelled"
                        bool success = _orderService.UpdateOrderStatus(orderId, "Cancelled");
                        
                        System.Diagnostics.Debug.WriteLine($"UpdateOrderStatus result: {success}");
                        
                        if (success)
                        {
                            MessageBox.Show(
                                "Đã hủy đơn hàng thành công!",
                                "Thành công",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                            
                            // Reload order history
                            LoadOrderHistory();
                        }
                        else
                        {
                            MessageBox.Show(
                                "Không thể hủy đơn hàng. Vui lòng thử lại!",
                                "Lỗi",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in CancelOrder_Click: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                        
                        MessageBox.Show(
                            $"Lỗi khi hủy đơn hàng: {ex.Message}",
                            "Lỗi",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ReceiveOrder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int orderId)
            {
                var result = MessageBox.Show(
                    "Bạn có chắc chắn đã nhận được hàng?",
                    "Xác nhận đã nhận hàng",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Set status to Completed
                        bool success = _orderService.UpdateOrderStatus(orderId, "Completed");
                        if (success)
                        {
                            MessageBox.Show(
                                "Cảm ơn bạn! Đơn hàng đã được đánh dấu hoàn thành.",
                                "Thành công",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                            LoadOrderHistory();
                        }
                        else
                        {
                            MessageBox.Show(
                                "Không thể cập nhật trạng thái. Vui lòng thử lại!",
                                "Lỗi",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Lỗi khi cập nhật trạng thái: {ex.Message}",
                            "Lỗi",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // ViewModel for Order display
    public class OrderViewModel
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public ObservableCollection<OrderDetailViewModel> OrderDetails { get; set; }
        public int ProductCount { get; set; }
        public string ProductSummary { get; set; }
        public string StatusDisplay { get; set; }
        public System.Windows.Media.Brush StatusColor { get; set; }
        public bool CanCancel { get; set; }
        public bool CanMarkReceived { get; set; }

        public OrderViewModel(Order order)
        {
            OrderId = order.OrderId;
            OrderDate = order.OrderDate ?? DateTime.Now;
            TotalAmount = order.TotalAmount;
            Status = order.Status ?? "Unknown";
            
            // Create status display
            StatusDisplay = GetStatusDisplay(order.Status);
            StatusColor = GetStatusColor(order.Status);
            
            // Check if order can be cancelled
            CanCancel = CanCancelOrder(order.Status);
            // Check if order can be marked as received
            CanMarkReceived = CanMarkReceivedOrder(order.Status);
            
            OrderDetails = new ObservableCollection<OrderDetailViewModel>();
            if (order.OrderDetails != null)
            {
                foreach (var detail in order.OrderDetails)
                {
                    OrderDetails.Add(new OrderDetailViewModel(detail));
                }
                ProductCount = order.OrderDetails.Count;
                
                // Create product summary
                var productSummaries = new List<string>();
                foreach (var detail in order.OrderDetails)
                {
                    if (detail.Product != null)
                    {
                        productSummaries.Add($"{detail.Product.ProductName} (x{detail.Quantity})");
                    }
                }
                ProductSummary = string.Join(", ", productSummaries);
            }
            else
            {
                ProductCount = 0;
                ProductSummary = "Không có sản phẩm";
            }
        }
        
        private string GetStatusDisplay(string? status)
        {
            return status?.ToLower() switch
            {
                "pending" => "⏳ Chờ xác nhận",
                "confirmed" => "✅ Đã xác nhận",
                "preparing" => "📦 Đang chuẩn bị",
                "shipping" => "🚚 Đang giao hàng",
                "delivered" => "📬 Đã giao hàng",
                "completed" => "✅ Hoàn thành",
                "cancelled" => "❌ Đã hủy",
                _ => "❓ Không xác định"
            };
        }
        
        private System.Windows.Media.Brush GetStatusColor(string? status)
        {
            return status?.ToLower() switch
            {
                "pending" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Orange),
                "confirmed" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Blue),
                "preparing" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Purple),
                "shipping" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.DarkBlue),
                "delivered" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green),
                "completed" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.DarkGreen),
                "cancelled" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red),
                _ => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray)
            };
        }
        
        private bool CanCancelOrder(string? status)
        {
            return status?.ToLower() switch
            {
                "pending" => true,
                "confirmed" => true,
                _ => false
            };
        }

        private bool CanMarkReceivedOrder(string? status)
        {
            return status?.ToLower() switch
            {
                "shipping" => true,
                "delivered" => true,
                _ => false
            };
        }
    }
    
    // ViewModel for OrderDetail display
    public class OrderDetailViewModel
    {
        public int OrderDetailId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public Product Product { get; set; }

        public OrderDetailViewModel(OrderDetail orderDetail)
        {
            OrderDetailId = orderDetail.OrderDetailId;
            Quantity = orderDetail.Quantity;
            UnitPrice = orderDetail.UnitPrice;
            Product = orderDetail.Product;
        }
    }
}
