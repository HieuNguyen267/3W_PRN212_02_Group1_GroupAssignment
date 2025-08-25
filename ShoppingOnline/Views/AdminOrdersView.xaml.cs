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
                _allOrders = _adminService.GetAllOrders();
                ApplyOrderFilters();
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi t?i d? li?u ??n h�ng: {ex.Message}", "L?i", 
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

                // Update count display
                if (OrderCountText != null)
                {
                    OrderCountText.Text = $"Hien thi: {Orders.Count} don hang";
                }

                // Update filtered revenue
                if (FilteredRevenueText != null)
                {
                    var totalRevenue = Orders.Sum(o => o.TotalAmount);
                    FilteredRevenueText.Text = $"Tong doanh thu: {totalRevenue:N0}?";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi l?c ??n h�ng: {ex.Message}", "L?i", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStatistics()
        {
            try
            {
                if (_allOrders == null) return;

                // Update statistics cards
                if (TotalOrdersText != null)
                    TotalOrdersText.Text = _allOrders.Count.ToString();

                if (PendingOrdersText != null)
                    PendingOrdersText.Text = _allOrders.Count(o => o.Status == "Pending").ToString();

                if (ConfirmedOrdersText != null)
                    ConfirmedOrdersText.Text = _allOrders.Count(o => o.Status == "Confirmed").ToString();

                if (ShippingOrdersText != null)
                    ShippingOrdersText.Text = _allOrders.Count(o => o.Status == "Shipping").ToString();

                if (TotalRevenueText != null)
                {
                    var totalRevenue = _allOrders.Sum(o => o.TotalAmount);
                    TotalRevenueText.Text = $"{totalRevenue:N0}?";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi c?p nh?t th?ng k�: {ex.Message}", "L?i", 
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
            MessageBox.Show("?� l�m m?i danh s�ch ??n h�ng!", "Th�ng b�o", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show("Ch?c n?ng Export Excel ?ang ???c ph�t tri?n!", "Th�ng b�o", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi export Excel: {ex.Message}", "L?i", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PrintReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show("Ch?c n?ng in b�o c�o ?ang ???c ph�t tri?n!", "Th�ng b�o", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi in b�o c�o: {ex.Message}", "L?i", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DetailedStats_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show("Ch?c n?ng th?ng k� chi ti?t ?ang ???c ph�t tri?n!", "Th�ng b�o", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi xem th?ng k� chi ti?t: {ex.Message}", "L?i", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportPDF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show("Ch?c n?ng Export PDF ?ang ???c ph�t tri?n!", "Th�ng b�o", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi export PDF: {ex.Message}", "L?i", 
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
                    MessageBox.Show($"L?i khi m? chi ti?t ??n h�ng: {ex.Message}", "L?i", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ConfirmOrder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int orderId)
            {
                var result = MessageBox.Show("B?n c� ch?c mu?n x�c nh?n ??n h�ng n�y?", 
                    "X�c nh?n ??n h�ng", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (_adminService.UpdateOrderStatus(orderId, "Confirmed"))
                        {
                            MessageBox.Show("?� x�c nh?n ??n h�ng th�nh c�ng!", "Th�nh c�ng", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadOrders(); // Refresh
                        }
                        else
                        {
                            MessageBox.Show("L?i khi x�c nh?n ??n h�ng!", "L?i", 
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

        private void ShipOrder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int orderId)
            {
                var result = MessageBox.Show("B?n c� ch?c mu?n chuy?n ??n h�ng n�y sang tr?ng th�i ?ang giao?", 
                    "Giao h�ng", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (_adminService.UpdateOrderStatus(orderId, "Shipping"))
                        {
                            MessageBox.Show("?� chuy?n ??n h�ng sang tr?ng th�i ?ang giao!", "Th�nh c�ng", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadOrders(); // Refresh
                        }
                        else
                        {
                            MessageBox.Show("L?i khi c?p nh?t tr?ng th�i ??n h�ng!", "L?i", 
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
                var result = MessageBox.Show("B?n c� ch?c mu?n h?y ??n h�ng n�y?", 
                    "H?y ??n h�ng", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (_adminService.UpdateOrderStatus(orderId, "Cancelled"))
                        {
                            MessageBox.Show("?� h?y ??n h�ng!", "Th�nh c�ng", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadOrders(); // Refresh
                        }
                        else
                        {
                            MessageBox.Show("L?i khi h?y ??n h�ng!", "L?i", 
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