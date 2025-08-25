using BLL.Services;
using DAL.Entities;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ShoppingOnline
{
    public partial class AdminOrdersWindow : Window, INotifyPropertyChanged
    {
        private readonly IAdminService _adminService;
        private ObservableCollection<Order> _orders = new();
        private List<Order> _allOrders = new();

        public AdminOrdersWindow()
        {
            InitializeComponent();
            _adminService = new AdminService();
            DataContext = this;
            
            LoadOrders();
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
                _allOrders = _adminService.GetAllOrders();
                
                // Initialize search placeholder visibility
                if (SearchPlaceholder != null && OrderSearchBox != null)
                {
                    SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(OrderSearchBox.Text) 
                        ? Visibility.Visible : Visibility.Hidden;
                }
                
                // Reset filters to default
                if (StatusFilterComboBox != null && StatusFilterComboBox.Items.Count > 0)
                {
                    StatusFilterComboBox.SelectedIndex = 0; // "Tat ca trang thai"
                }
                
                if (DateFilterComboBox != null && DateFilterComboBox.Items.Count > 0)
                {
                    DateFilterComboBox.SelectedIndex = 0; // "Tat ca"
                }
                
                // Clear search box
                if (OrderSearchBox != null)
                {
                    OrderSearchBox.Text = "";
                }
                
                ApplyOrderFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi t?i d? li?u ??n hàng: {ex.Message}", "L?i", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyOrderFilters()
        {
            try
            {
                // Check if controls are available
                if (OrderSearchBox == null || StatusFilterComboBox == null || DateFilterComboBox == null || OrdersDataGrid == null)
                {
                    return; // Exit if controls are not loaded yet
                }

                var filteredOrders = _allOrders.AsEnumerable();
                
                // Debug logging
                System.Diagnostics.Debug.WriteLine($"Total orders before filtering: {_allOrders.Count}");

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
                    
                    System.Diagnostics.Debug.WriteLine($"After search filter '{searchText}': {filteredOrders.Count()}");
                }

                // Apply status filter
                var selectedStatus = (StatusFilterComboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString();
                System.Diagnostics.Debug.WriteLine($"Selected status: '{selectedStatus}'");
                
                if (!string.IsNullOrEmpty(selectedStatus) && selectedStatus != "Tat ca trang thai")
                {
                    filteredOrders = filteredOrders.Where(o => o.Status == selectedStatus);
                    System.Diagnostics.Debug.WriteLine($"After status filter '{selectedStatus}': {filteredOrders.Count()}");
                }

                // Apply date filter
                var selectedDateFilter = (DateFilterComboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString();
                System.Diagnostics.Debug.WriteLine($"Selected date filter: '{selectedDateFilter}'");
                
                if (!string.IsNullOrEmpty(selectedDateFilter) && selectedDateFilter != "Tat ca")
                {
                    var now = DateTime.Now;
                    filteredOrders = selectedDateFilter switch
                    {
                        "Hom nay" => filteredOrders.Where(o => o.OrderDate?.Date == now.Date),
                        "7 ngay qua" => filteredOrders.Where(o => o.OrderDate >= now.AddDays(-7)),
                        "30 ngay qua" => filteredOrders.Where(o => o.OrderDate >= now.AddDays(-30)),
                        _ => filteredOrders
                    };
                    System.Diagnostics.Debug.WriteLine($"After date filter '{selectedDateFilter}': {filteredOrders.Count()}");
                }

                // Update orders collection
                Orders.Clear();
                foreach (var order in filteredOrders.OrderByDescending(o => o.OrderDate))
                {
                    Orders.Add(order);
                }

                // Update DataGrid
                OrdersDataGrid.ItemsSource = Orders;
                
                // Show count in page subtitle
                var totalCount = filteredOrders.Count();
                PageSubtitle.Text = $"Danh sách và tr?ng thái ??n hàng ({totalCount} ??n hàng)";
                
                System.Diagnostics.Debug.WriteLine($"Final filtered count: {totalCount}");
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi l?c ??n hàng: {ex.Message}", "L?i", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine($"Error in ApplyOrderFilters: {ex.Message}");
            }
        }

        #region Event Handlers
        private void OrderSearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            SearchPlaceholder.Visibility = Visibility.Hidden;
        }

        private void OrderSearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(OrderSearchBox.Text))
            {
                SearchPlaceholder.Visibility = Visibility.Visible;
            }
        }

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
                // Reload all orders data
                _allOrders = _adminService.GetAllOrders();
                
                // Debug info
                MessageBox.Show($"?ã t?i {_allOrders.Count} ??n hàng t? database", "Debug Info", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Reset all filters
                if (StatusFilterComboBox != null)
                {
                    StatusFilterComboBox.SelectedIndex = 0; // "Tat ca trang thai"
                }
                
                if (DateFilterComboBox != null)
                {
                    DateFilterComboBox.SelectedIndex = 0; // "Tat ca"
                }
                
                if (OrderSearchBox != null)
                {
                    OrderSearchBox.Text = "";
                    SearchPlaceholder.Visibility = Visibility.Visible;
                }
                
                ApplyOrderFilters();
                
                MessageBox.Show("?ã làm m?i danh sách ??n hàng!", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi làm m?i d? li?u: {ex.Message}", "L?i", 
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
                    MessageBox.Show($"L?i khi m? chi ti?t ??n hàng: {ex.Message}", "L?i", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ConfirmOrder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int orderId)
            {
                var result = MessageBox.Show("B?n có ch?c mu?n xác nh?n ??n hàng này?", 
                    "Xác nh?n ??n hàng", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (_adminService.UpdateOrderStatus(orderId, "Confirmed"))
                        {
                            MessageBox.Show("?ã xác nh?n ??n hàng thành công!", "Thành công", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadOrders(); // Refresh
                        }
                        else
                        {
                            MessageBox.Show("L?i khi xác nh?n ??n hàng!", "L?i", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"L?i: {ex.Message}", "L?i", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void CancelOrder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int orderId)
            {
                var result = MessageBox.Show("B?n có ch?c mu?n h?y ??n hàng này?", 
                    "H?y ??n hàng", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (_adminService.UpdateOrderStatus(orderId, "Cancelled"))
                        {
                            MessageBox.Show("?ã h?y ??n hàng!", "Thành công", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadOrders(); // Refresh
                        }
                        else
                        {
                            MessageBox.Show("L?i khi h?y ??n hàng!", "L?i", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"L?i: {ex.Message}", "L?i", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BackToDashboard_Click(object sender, RoutedEventArgs e)
        {
            // Not needed anymore in single-window navigation
            MessageBox.Show("Navigation ?ã ???c c?p nh?t! Vui lòng s? d?ng menu bên trái ?? chuy?n trang.", "Thông báo", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // Not needed anymore in single-window navigation
            AdminSession.EndNavigation();
            base.OnClosing(e);
        }

        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}