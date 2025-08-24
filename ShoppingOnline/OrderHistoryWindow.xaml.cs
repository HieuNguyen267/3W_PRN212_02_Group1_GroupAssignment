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

        public OrderViewModel(Order order)
        {
            OrderId = order.OrderId;
            OrderDate = order.OrderDate ?? DateTime.Now;
            TotalAmount = order.TotalAmount;
            Status = order.Status ?? "Unknown";
            
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
