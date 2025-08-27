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
                        // Filter for cancelled orders (check Notes field)
                        filteredOrders = filteredOrders.Where(o => o.Notes?.Contains("[CANCELLED]") == true);
                    }
                    else
                    {
                        // Filter for normal status and exclude cancelled orders
                        filteredOrders = filteredOrders.Where(o => 
                            o.Status == selectedStatus && 
                            (o.Notes?.Contains("[CANCELLED]") != true));
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

                // Convert to OrderViewModel and update UI
                Orders.Clear();
                foreach (var order in filteredOrders)
                {
                    Orders.Add(new OrderViewModel(order));
                }

                // Update count display
                if (OrderCountText != null)
                {
                    OrderCountText.Text = $"Hien thi: {Orders.Count} don hang";
                }

                // Update filtered revenue - only count non-cancelled orders (by Status and Notes)
                if (FilteredRevenueText != null)
                {
                    var totalRevenue = Orders
                        .Where(o => o.Status != "Cancelled" && (o.Notes == null || !o.Notes.Contains("[CANCELLED]")))
                        .Sum(o => o.TotalAmount);
                    FilteredRevenueText.Text = $"Tong gia tri don hang: {totalRevenue:N0} VND";
                }
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
                // Use filtered Orders for statistics (instead of all loaded orders)
                var statsOrders = Orders.ToList();

                // Update statistics cards based on filtered Orders
                TotalOrdersText.Text = statsOrders.Count(o => o.Status != "Cancelled").ToString();
                PendingOrdersText.Text = statsOrders.Count(o => o.Status == "Pending").ToString();
                ConfirmedOrdersText.Text = statsOrders.Count(o => o.Status == "Confirmed").ToString();
                ShippingOrdersText.Text = statsOrders.Count(o => o.Status == "Shipping").ToString();

                // Calculate and display total revenue for filtered orders
                var totalValue = statsOrders.Where(o => o.Status != "Cancelled").Sum(o => o.TotalAmount);
                TotalRevenueText.Text = $"{totalValue:N0} VND";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi khi cap nhat thong ke: {ex.Message}", "Loi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Event Handlers
        private void OrderSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (OrderSearchBox != null)
            {
                SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(OrderSearchBox.Text) 
                    ? Visibility.Visible : Visibility.Hidden;
                
                ApplyOrderFilters();
                // Refresh top statistics after search filter changes
                UpdateStatistics();
            }
        }

        private void StatusFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyOrderFilters();
            // Refresh top statistics after status filter changes
            UpdateStatistics();
        }

        private void DateFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyOrderFilters();
            // Refresh top statistics after date filter changes
            UpdateStatistics();
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
                
                // Update search placeholder visibility
                if (SearchPlaceholder != null)
                {
                    SearchPlaceholder.Visibility = Visibility.Visible;
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

        private void ViewOrderDetail_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int orderId)
            {
                try
                {
                    var orderDetailWindow = new OrderDetailWindow(orderId);
                    var result = orderDetailWindow.ShowDialog();
                    
                    if (result == true)
                    {
                        // Refresh orders if changes were made
                        LoadOrders();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Loi khi mo chi tiet don hang: {ex.Message}", "Loi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ConfirmOrder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int orderId)
            {
                var result = MessageBox.Show("Bạn có chắc muốn xác nhận đơn hàng này?", 
                    "Xác nhận đơn hàng", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Validate order status before confirming
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
                            MessageBox.Show("DON HANG DA BI HUY!\n\n" +
                                          "Khong the xac nhan don hang da bi huy.\n" +
                                          "Vui long tao don hang moi neu can thiet.", 
                                          "Don hang da bi huy", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        if (order.Status != "Pending")
                        {
                            MessageBox.Show($"Chi co the xac nhan don hang co trang thai 'Pending'. Trang thai hien tai: {order.Status}", 
                                "Thong bao", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        if (_adminService.UpdateOrderStatus(orderId, "Confirmed"))
                        {
                            MessageBox.Show("Đã xác nhận đơn hàng thành công!", "Thành công", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadOrders(); // Refresh
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
                        // Refresh the orders list to show updated carrier information
                        LoadOrders();
                        MessageBox.Show("Đã gán người giao hàng thành công!", "Thành công", 
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Loi khi chon nguoi giao hang: {ex.Message}", "Loi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CancelOrder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int orderId)
            {
                var result = MessageBox.Show("Bạn có chắc muốn hủy đơn hàng này?\n\n" +
                                           "LƯU Ý: Đơn hàng bị hủy sẽ KHÔNG được tính vào tổng giá trị đơn hàng và thống kê.", 
                    "Hủy đơn hàng", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var order = _adminService.GetOrderById(orderId);
                        if (order == null)
                        {
                            MessageBox.Show("Khong tim thay don hang!", "Loi", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (order.Status == "Delivered")
                        {
                            MessageBox.Show("Khong the huy don hang da duoc giao!", "Thong bao", 
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        if (order.Notes?.Contains("[CANCELLED]") == true)
                        {
                            MessageBox.Show("Don hang da duoc huy truoc do!", "Thong bao", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }

                        // Use Notes-based cancellation to avoid database constraint issues
                        var cancellationNote = $"[CANCELLED] Don hang da bi huy luc {DateTime.Now:dd/MM/yyyy HH:mm}. " +
                                             $"Ly do: Yeu cau huy tu admin. Gia tri don hang: {order.TotalAmount:N0} VND. ";
                        
                        if (!string.IsNullOrEmpty(order.Notes))
                        {
                            cancellationNote += $"Ghi chu cu: {order.Notes}";
                        }

                        // Update notes in database using direct context to avoid constraint issues
                        using var context = new DAL.Entities.ShoppingOnlineContext();
                        var dbOrder = context.Orders.Find(orderId);
                        if (dbOrder != null)
                        {
                            dbOrder.Notes = cancellationNote;
                            var changes = context.SaveChanges();
                            
                            if (changes > 0)
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
                        else
                        {
                            MessageBox.Show("Khong tim thay don hang trong database!", "Loi", 
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

            // Delegate all properties to the underlying Order
            public int OrderId => _order.OrderId;
            public Customer? Customer => _order.Customer;
            public string? Phone => _order.Phone;
            public DateTime OrderDate => _order.OrderDate ?? DateTime.Now;
            public decimal TotalAmount => _order.TotalAmount;
            public string? Status => _order.Status;
            public string? ShippingAddress => _order.ShippingAddress;
            public string? Notes => _order.Notes;
            public int? CarrierId => _order.CarrierId;
            public Carrier? Carrier => _order.Carrier;

            // Computed properties for display
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

            // Action button visibility properties
            public bool CanConfirm
            {
                get
                {
                    // Only show confirm button for Pending orders
                    return _order.Status == "Pending";
                }
            }

            public bool CanCancel
            {
                get
                {
                    // Only show cancel button for Pending and Confirmed orders
                    return _order.Status == "Pending" || _order.Status == "Confirmed";
                }
            }

            public bool CanAssignCarrier
            {
                get
                {
                    // Show assign carrier button for Confirmed and Preparing orders
                    return _order.Status == "Confirmed" || _order.Status == "Preparing";
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}