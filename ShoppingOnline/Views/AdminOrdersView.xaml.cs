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
                    filteredOrders = filteredOrders.Where(o => o.Status == selectedStatus);
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
                        _ => filteredOrders
                    };
                }

                // Update orders collection
                Orders.Clear();
                foreach (var order in filteredOrders.OrderByDescending(o => o.OrderDate))
                {
                    Orders.Add(order);
                }

                // Update DataGrid
                if (OrdersDataGrid != null)
                {
                    OrdersDataGrid.ItemsSource = Orders;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi l?c ??n hàng: {ex.Message}", "L?i", 
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
            LoadOrders();
            MessageBox.Show("?ã làm m?i danh sách ??n hàng!", "Thông báo", 
                MessageBoxButton.OK, MessageBoxImage.Information);
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