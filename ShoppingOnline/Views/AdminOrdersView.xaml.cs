using BLL.Services;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ShoppingOnline.Views
{
    public partial class AdminOrdersView : UserControl, INotifyPropertyChanged
    {
        private readonly IAdminService _adminService;
        private ObservableCollection<OrderViewModel> _orders = new();
        private List<Order> _allOrders = new();

        public AdminOrdersView()
        {
            InitializeComponent();
            _adminService = new AdminService();
            DataContext = this;
            
            Loaded += AdminOrdersView_Loaded;
        }

        private void AdminOrdersView_Loaded(object sender, RoutedEventArgs e)
        {
            LoadOrders();
            UpdateStatistics();
        }

        public ObservableCollection<OrderViewModel> Orders
        {
            get => _orders;
            set
            {
                _orders = value;
                OnPropertyChanged(nameof(Orders));
            }
        }

        private void LoadOrders()
        {
            try
            {
                // Load all orders from AdminService and store for filtering
                _allOrders = _adminService.GetAllOrders();
                
                // Apply current filters
                ApplyOrderFilters();
                
                // Update statistics
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi khi tai danh sach don hang: {ex.Message}", "Loi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyOrderFilters()
        {
            try
            {
                // Start with all orders
                var filteredOrders = _allOrders.AsEnumerable();

                // Apply search filter
                var searchText = OrderSearchBox?.Text?.Trim();
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    var searchLower = searchText.ToLower();
                    filteredOrders = filteredOrders.Where(o => 
                        o.OrderId.ToString().Contains(searchLower) ||
                        (o.Customer?.FullName?.ToLower().Contains(searchLower) == true) ||
                        (o.Phone?.ToLower().Contains(searchLower) == true) ||
                        (o.ShippingAddress?.ToLower().Contains(searchLower) == true));
                }

                // Apply status filter
                var selectedStatus = (StatusFilterComboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString();
                if (!string.IsNullOrEmpty(selectedStatus) && selectedStatus != "Tat ca trang thai")
                {
                    if (selectedStatus == "Cancelled")
                    {
                        // Filter for cancelled orders
                        filteredOrders = filteredOrders.Where(o => IsOrderCancelled(o));
                    }
                    else
                    {
                        // Filter for normal status and exclude cancelled orders
                        filteredOrders = filteredOrders.Where(o => 
                            o.Status == selectedStatus && 
                            !IsOrderCancelled(o));
                    }
                }

                // Apply date filter
                var selectedDateFilter = (DateFilterComboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString();
                if (!string.IsNullOrEmpty(selectedDateFilter) && selectedDateFilter != "Tat ca")
                {
                    var today = DateTime.Today;
                    switch (selectedDateFilter)
                    {
                        case "Hom nay":
                            filteredOrders = filteredOrders.Where(o => (o.OrderDate ?? DateTime.Now).Date == today);
                            break;
                        case "7 ngay qua":
                            filteredOrders = filteredOrders.Where(o => (o.OrderDate ?? DateTime.Now).Date >= today.AddDays(-7));
                            break;
                        case "30 ngay qua":
                            filteredOrders = filteredOrders.Where(o => (o.OrderDate ?? DateTime.Now).Date >= today.AddDays(-30));
                            break;
                        case "Thang nay":
                            filteredOrders = filteredOrders.Where(o => (o.OrderDate ?? DateTime.Now).Year == today.Year && (o.OrderDate ?? DateTime.Now).Month == today.Month);
                            break;
                    }
                }

                // Convert to ViewModels and update ObservableCollection
                var orderViewModels = filteredOrders
                    .Select(o => new OrderViewModel(o))
                    .OrderByDescending(ovm => ovm.OrderDate)
                    .ToList();

                Orders.Clear();
                foreach (var orderVM in orderViewModels)
                {
                    Orders.Add(orderVM);
                }

                // Update footer statistics after filtering
                UpdateFooterStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi khi loc don hang: {ex.Message}", "Loi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStatistics()
        {
            try
            {
                // Calculate total order value excluding cancelled orders
                var totalValue = _allOrders
                    .Where(o => !IsOrderCancelled(o))
                    .Sum(o => o.TotalAmount);

                // Update UI elements if they exist
                if (TotalRevenueText != null)
                {
                    TotalRevenueText.Text = $"{totalValue:N0} VND";
                }

                if (TotalOrdersText != null)
                {
                    var totalOrders = _allOrders.Count(o => !IsOrderCancelled(o));
                    TotalOrdersText.Text = totalOrders.ToString();
                }

                // Update footer statistics
                UpdateFooterStatistics();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating statistics: {ex.Message}");
            }
        }

        private bool IsOrderCancelled(Order order)
        {
            // Check if order is cancelled by either Status or Notes
            return order.Status == "Cancelled" || 
                   order.Notes?.Contains("[CANCELLED]") == true;
        }

        private void UpdateFooterStatistics()
        {
            try
            {
                // Update order count in footer (showing filtered orders)
                if (OrderCountText != null)
                {
                    OrderCountText.Text = $"Hien thi: {Orders.Count} don hang";
                }

                // Update filtered revenue in footer (only non-cancelled orders from filtered list)
                if (FilteredRevenueText != null)
                {
                    var filteredRevenue = Orders
                        .Where(o => !(o.Status == "Cancelled" || o.Notes?.Contains("[CANCELLED]") == true))
                        .Sum(o => o.TotalAmount ?? 0);
                    
                    FilteredRevenueText.Text = $"Tong gia tri don hang: {filteredRevenue:N0} VND";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating footer statistics: {ex.Message}");
            }
        }

        #region Event Handlers

        private void OrderSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyOrderFilters();
        }

        private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyOrderFilters();
        }

        private void DateFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyOrderFilters();
        }

        private void RefreshOrders_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Reset search box
                if (OrderSearchBox != null)
                {
                    OrderSearchBox.Text = "";
                }
                
                // Reset status filter
                if (StatusFilterComboBox != null)
                {
                    StatusFilterComboBox.SelectedIndex = 0; // "Tat ca trang thai"
                }
                
                // Reset date filter
                if (DateFilterComboBox != null)
                {
                    DateFilterComboBox.SelectedIndex = 0; // "Tat ca"
                }
                
                // Reload all orders
                LoadOrders();
                
                MessageBox.Show("Da lam moi danh sach don hang!", "Thong bao", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi khi lam moi du lieu: {ex.Message}", "Loi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConfirmOrder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int orderId)
            {
                try
                {
                    var success = _adminService.UpdateOrderStatus(orderId, "Confirmed");
                    
                    if (success)
                    {
                        MessageBox.Show("Đã xác nhận đơn hàng thành công!", "Thành công", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadOrders(); // Refresh orders and statistics
                    }
                    else
                    {
                        MessageBox.Show("Loi khi xac nhan don hang!", "Loi", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Loi: {ex.Message}", "Loi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CancelOrder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int orderId)
            {
                try
                {
                    // Find the order to get its details
                    var order = _allOrders.FirstOrDefault(o => o.OrderId == orderId);
                    if (order == null)
                    {
                        MessageBox.Show("Khong tim thay don hang!", "Loi", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Create cancellation note
                    var cancellationNote = $"[CANCELLED] - Huy bo luc {DateTime.Now:dd/MM/yyyy HH:mm:ss}";

                    // Update notes using AdminService
                    var updateSuccess = _adminService.UpdateOrderNotes(orderId, cancellationNote);
                    
                    if (updateSuccess)
                    {
                        MessageBox.Show($"Đã hủy đơn hàng thành công!\n\n" +
                                      $"Đơn hàng #{orderId} giá trị {order.TotalAmount:N0} VND đã bị loại khỏi tổng giá trị đơn hàng.", 
                                      "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadOrders(); // Refresh orders and statistics
                    }
                    else
                    {
                        MessageBox.Show("Loi khi huy don hang!", "Loi", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Loi: {ex.Message}\n\nChi tiet: {ex.InnerException?.Message}", "Loi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AssignCarrierToOrder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int orderId)
            {
                try
                {
                    // Find the order
                    var order = _allOrders.FirstOrDefault(o => o.OrderId == orderId);
                    if (order == null)
                    {
                        MessageBox.Show("Khong tim thay don hang!", "Loi", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Check if order is cancelled
                    if (order.Notes?.Contains("[CANCELLED]") == true)
                    {
                        MessageBox.Show("Don hang da bi huy - khong the gan nguoi giao hang!", "Loi", 
                                      MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Create and show carrier selection dialog
                    var carrierSelectionDialog = new CarrierSelectionDialog(orderId);
                    var result = carrierSelectionDialog.ShowDialog();
                    
                    if (result == true)
                    {
                        LoadOrders(); // Refresh orders
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Loi: {ex.Message}", "Loi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ViewOrderDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int orderId)
            {
                try
                {
                    var orderDetailWindow = new OrderDetailWindow(orderId);
                    orderDetailWindow.ShowDialog();
                    
                    // Refresh orders after viewing details
                    LoadOrders();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Loi: {ex.Message}", "Loi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        // ViewModel to handle order display and action button visibility
        public class OrderViewModel : INotifyPropertyChanged
        {
            private readonly Order _order;

            public OrderViewModel(Order order)
            {
                _order = order;
            }

            public int OrderId => _order.OrderId;
            public DateTime OrderDate => _order.OrderDate ?? DateTime.Now;
            public string CustomerName => _order.Customer?.FullName ?? "N/A";
            public string Phone => _order.Phone ?? "N/A";
            public string ShippingAddress => _order.ShippingAddress ?? "N/A";
            public decimal? TotalAmount => _order.TotalAmount;
            public string Status => _order.Status ?? "Pending";
            public string Notes => _order.Notes ?? "";
            
            // Additional properties for XAML binding
            public Customer? Customer => _order.Customer;
            public string ProductNames
            {
                get
                {
                    if (_order.OrderDetails != null && _order.OrderDetails.Any())
                    {
                        var productNames = _order.OrderDetails
                            .Where(od => od.Product != null)
                            .Select(od => od.Product!.ProductName)
                            .Distinct()
                            .ToList();
                        
                        return productNames.Any() ? string.Join(", ", productNames) : "Khong co san pham";
                    }
                    return "Khong co san pham";
                }
            }

            public int TotalQuantity
            {
                get
                {
                    return _order.OrderDetails?.Sum(od => od.Quantity) ?? 0;
                }
            }

            public string CarrierName
            {
                get
                {
                    return _order.Carrier?.FullName ?? "Chua chon";
                }
            }

            public string CarrierInfo
            {
                get
                {
                    if (_order.Carrier != null)
                    {
                        return $"Tên: {_order.Carrier.FullName}\nSĐT: {_order.Carrier.Phone}";
                    }
                    return "Chưa chọn người vận chuyển";
                }
            }

            // Computed properties for UI
            public string StatusDisplay
            {
                get
                {
                    if (Status == "Cancelled" || Notes?.Contains("[CANCELLED]") == true)
                        return "Đã hủy";
                    return Status switch
                    {
                        "Pending" => "Chờ xác nhận",
                        "Confirmed" => "Đã xác nhận",
                        "Preparing" => "Đang chuẩn bị",
                        "Shipping" => "Đang giao",
                        "Delivered" => "Đã giao",
                        "Completed" => "Hoàn thành",
                        _ => Status
                    };
                }
            }

            public string StatusColor
            {
                get
                {
                    if (Status == "Cancelled" || Notes?.Contains("[CANCELLED]") == true)
                        return "#FF6B6B"; // Red for cancelled
                    return Status switch
                    {
                        "Pending" => "#FFA500", // Orange
                        "Confirmed" => "#4169E1", // Blue
                        "Preparing" => "#9370DB", // Purple
                        "Shipping" => "#32CD32", // Green
                        "Delivered" => "#20B2AA", // Light Sea Green
                        "Completed" => "#228B22", // Forest Green
                        _ => "#808080" // Gray
                    };
                }
            }

            public bool CanConfirm => Status == "Pending" && !IsOrderCancelled();
            public bool CanCancel => (Status == "Pending" || Status == "Confirmed") && !IsOrderCancelled();
            public bool CanAssignCarrier => (Status == "Confirmed" || Status == "Preparing") && !IsOrderCancelled();

            private bool IsOrderCancelled()
            {
                return Status == "Cancelled" || Notes?.Contains("[CANCELLED]") == true;
            }

            public event PropertyChangedEventHandler? PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
