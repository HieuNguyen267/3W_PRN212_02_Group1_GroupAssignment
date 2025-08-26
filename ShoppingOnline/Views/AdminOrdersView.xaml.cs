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
        private ObservableCollection<Order> _orders = new();
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

        public ObservableCollection<Order> Orders
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
                    var now = DateTime.Now;
                    filteredOrders = selectedDateFilter switch
                    {
                        "Hom nay" => filteredOrders.Where(o => o.OrderDate?.Date == now.Date),
                        "7 ngay qua" => filteredOrders.Where(o => o.OrderDate >= now.AddDays(-7)),
                        "30 ngay qua" => filteredOrders.Where(o => o.OrderDate >= now.AddDays(-30)),
                        "Thang nay" => filteredOrders.Where(o => o.OrderDate?.Month == now.Month && o.OrderDate?.Year == now.Year),
                        _ => filteredOrders
                    };
                }

                // Convert to display models with product information and carrier information, then sort
                var displayOrders = filteredOrders.Select(o => 
                {
                    // Get order details to show product information
                    var orderDetails = _adminService.GetOrderDetailsByOrderId(o.OrderId);
                    var productNames = orderDetails.Any() 
                        ? string.Join(", ", orderDetails.Take(2).Select(od => od.Product?.ProductName).Where(name => !string.IsNullOrEmpty(name)))
                        : "Khong co san pham";
                    
                    // Add "..." if there are more than 2 products
                    if (orderDetails.Count > 2)
                    {
                        productNames += "...";
                    }
                    
                    var totalQuantity = orderDetails.Sum(od => od.Quantity);
                    
                    // Get carrier information
                    string carrierName = "Chua chon";
                    string carrierInfo = "Chua gan nguoi giao hang";
                    
                    if (o.CarrierId.HasValue && o.Carrier != null)
                    {
                        carrierName = o.Carrier.FullName ?? "Khong ro ten";
                        carrierInfo = $"Ten: {o.Carrier.FullName ?? "Khong ro"}\nSo dien thoai: {o.Carrier.Phone ?? "Khong co"}\nXe: {o.Carrier.VehicleNumber ?? "Khong co"}";
                    }
                    
                    return new OrderDisplayModel
                    {
                        OrderId = o.OrderId,
                        Customer = o.Customer,
                        Phone = o.Phone,
                        OrderDate = o.OrderDate,
                        TotalAmount = o.TotalAmount,
                        Status = o.Notes?.Contains("[CANCELLED]") == true ? "Cancelled" : o.Status, // Show as Cancelled if cancelled via Notes
                        ShippingAddress = o.ShippingAddress,
                        ActualStatus = o.Status, // Keep actual status for internal use
                        Notes = o.Notes,
                        ProductNames = productNames,
                        TotalQuantity = totalQuantity,
                        CarrierName = carrierName,
                        CarrierInfo = carrierInfo,
                        CarrierId = o.CarrierId
                    };
                }).OrderByDescending(o => o.OrderDate).ToList();

                // Update DataGrid
                if (OrdersDataGrid != null)
                {
                    OrdersDataGrid.ItemsSource = displayOrders;
                }

                // Update count display
                if (OrderCountText != null)
                {
                    OrderCountText.Text = $"Hien thi: {displayOrders.Count} don hang";
                }

                // Update filtered revenue - only count non-cancelled orders
                if (FilteredRevenueText != null)
                {
                    var totalRevenue = displayOrders
                        .Where(o => o.Status != "Cancelled") // Exclude cancelled orders from revenue calculation
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
                // Use _allOrders (all loaded orders) for statistics
                var allOrders = _allOrders ?? new List<Order>();

                // Update statistics cards - exclude cancelled orders from counts
                if (TotalOrdersText != null)
                    TotalOrdersText.Text = allOrders.Count(o => o.Notes?.Contains("[CANCELLED]") != true).ToString();

                if (PendingOrdersText != null)
                    PendingOrdersText.Text = allOrders.Count(o => o.Status == "Pending" && (o.Notes?.Contains("[CANCELLED]") != true)).ToString();

                if (ConfirmedOrdersText != null)
                    ConfirmedOrdersText.Text = allOrders.Count(o => o.Status == "Confirmed" && (o.Notes?.Contains("[CANCELLED]") != true)).ToString();

                if (ShippingOrdersText != null)
                    ShippingOrdersText.Text = allOrders.Count(o => o.Status == "Shipping" && (o.Notes?.Contains("[CANCELLED]") != true)).ToString();

                if (TotalRevenueText != null)
                {
                    // Calculate total order value (all non-cancelled orders) instead of just completed orders
                    var totalOrderValue = allOrders
                        .Where(o => (o.Notes?.Contains("[CANCELLED]") != true))
                        .Sum(o => o.TotalAmount);
                    TotalRevenueText.Text = $"{totalOrderValue:N0} VND";
                }
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
            }
        }

        private void StatusFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyOrderFilters();
        }

        private void DateFilter_Changed(object sender, SelectionChangedEventArgs e)
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
                var result = MessageBox.Show("Ban co chac muon xac nhan don hang nay?", 
                    "Xac nhan don hang", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
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
                            MessageBox.Show("Da xac nhan don hang thanh cong!", "Thanh cong", 
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
                        MessageBox.Show("Da gan nguoi giao hang thanh cong!", "Thanh cong", 
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
                var result = MessageBox.Show("Ban co chac muon huy don hang nay?\n\n" +
                                           "LUU Y: Don hang bi huy se KHONG duoc tinh vao tong gia tri don hang va thong ke.", 
                    "Huy don hang", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                
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
                                MessageBox.Show($"Da huy don hang thanh cong!\n\n" +
                                              $"Don hang #{orderId} gia tri {order.TotalAmount:N0} VND da bi loai khoi tong gia tri don hang.", 
                                              "Thanh cong", MessageBoxButton.OK, MessageBoxImage.Information);
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

        // Add display model to handle cancelled orders properly and include product information
        public class OrderDisplayModel
        {
            public int OrderId { get; set; }
            public Customer? Customer { get; set; }
            public string? Phone { get; set; }
            public DateTime? OrderDate { get; set; }
            public decimal TotalAmount { get; set; }
            public string? Status { get; set; } // This will show "Cancelled" for cancelled orders
            public string? ShippingAddress { get; set; }
            public string? ActualStatus { get; set; } // Original database status
            public string? Notes { get; set; }
            public string ProductNames { get; set; } = ""; // Product names in order
            public int TotalQuantity { get; set; } // Total quantity of all products
            public string CarrierName { get; set; } = "Chua chon"; // Carrier name
            public string CarrierInfo { get; set; } = "Chua gan nguoi giao hang"; // Carrier details for tooltip
            public int? CarrierId { get; set; } // Carrier ID
            
            // Property to check if carrier is assigned
            public bool HasCarrierAssigned => !string.IsNullOrEmpty(CarrierName) && CarrierName != "Chua chon";
        }
    }
}