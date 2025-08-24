using BLL.Services;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ShoppingOnline
{
    public partial class OrderDetailWindow : Window
    {
        private readonly IAdminService _adminService;
        private readonly OrderService _orderService;
        private Order _currentOrder;
        private int _orderId;
        private List<Carrier> _availableCarriers;

        public OrderDetailWindow(int orderId)
        {
            InitializeComponent();
            _adminService = new AdminService();
            _orderService = new OrderService();
            _orderId = orderId;
            _availableCarriers = new List<Carrier>();
            
            LoadOrderData();
            LoadCarriers();
        }

        private void LoadOrderData()
        {
            try
            {
                _currentOrder = _adminService.GetOrderById(_orderId);
                if (_currentOrder == null)
                {
                    MessageBox.Show("Kh�ng t�m th?y ??n h�ng!", "L?i", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                    return;
                }

                // Update header
                OrderTitleText.Text = $"Chi ti?t ??n h�ng #{_currentOrder.OrderId}";
                OrderDateText.Text = $"Ng�y ??t: {_currentOrder.OrderDate:dd/MM/yyyy HH:mm}";

                // Update customer info
                CustomerNameText.Text = _currentOrder.Customer?.FullName ?? "N/A";
                CustomerPhoneText.Text = _currentOrder.Phone ?? "N/A";
                CustomerEmailText.Text = _currentOrder.Customer?.Account?.Email ?? "N/A";
                ShippingAddressText.Text = _currentOrder.ShippingAddress ?? "N/A";

                // Update status
                UpdateStatusDisplay(_currentOrder.Status ?? "Pending");
                
                // Set status combobox
                foreach (ComboBoxItem item in StatusComboBox.Items)
                {
                    if (item.Content.ToString() == _currentOrder.Status)
                    {
                        StatusComboBox.SelectedItem = item;
                        break;
                    }
                }

                // Update carrier info
                if (_currentOrder.Carrier != null)
                {
                    CurrentCarrierText.Text = $"{_currentOrder.Carrier.FullName} - {_currentOrder.Carrier.Phone} ({_currentOrder.Carrier.VehicleNumber})";
                }
                else
                {
                    CurrentCarrierText.Text = "Ch?a ch?n ng??i v?n chuy?n";
                }

                // Load order items
                var orderDetails = _adminService.GetOrderDetails(_orderId);
                OrderItemsGrid.ItemsSource = orderDetails;

                // Update total
                TotalAmountText.Text = $"{_currentOrder.TotalAmount:N0}?";

                // Load notes
                NotesTextBox.Text = _currentOrder.Notes ?? "";

                // Update button states based on status
                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi t?i d? li?u ??n h�ng: {ex.Message}", "L?i", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCarriers()
        {
            try
            {
                _availableCarriers = _adminService.GetAvailableCarriers();
                CarrierComboBox.ItemsSource = _availableCarriers;
                
                // Select current carrier if exists
                if (_currentOrder?.CarrierId != null)
                {
                    CarrierComboBox.SelectedValue = _currentOrder.CarrierId;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi t?i danh s�ch ng??i v?n chuy?n: {ex.Message}", "L?i", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStatusDisplay(string status)
        {
            CurrentStatusText.Text = status;
            
            // Update status color
            StatusBorder.Background = status switch
            {
                "Pending" => new SolidColorBrush(Color.FromRgb(255, 193, 7)),      // Yellow
                "Confirmed" => new SolidColorBrush(Color.FromRgb(33, 150, 243)),   // Blue
                "Shipping" => new SolidColorBrush(Color.FromRgb(255, 152, 0)),     // Orange
                "Delivered" => new SolidColorBrush(Color.FromRgb(76, 175, 80)),    // Green
                "Cancelled" => new SolidColorBrush(Color.FromRgb(244, 67, 54)),    // Red
                _ => new SolidColorBrush(Color.FromRgb(158, 158, 158))             // Gray
            };
        }

        private void UpdateButtonStates()
        {
            var status = _currentOrder?.Status ?? "Pending";
            
            // Enable/disable buttons based on status
            var confirmButton = FindName("ConfirmOrder_Click") as Button;
            var cancelButton = FindName("CancelOrder_Click") as Button;
            
            // Update button in grid would need different approach since it's in XAML
            // For now, we'll handle this through the button click events
        }

        #region Event Handlers

        private void UpdateStatus_Click(object sender, RoutedEventArgs e)
        {
            if (StatusComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var newStatus = selectedItem.Content.ToString();
                
                var result = MessageBox.Show($"B?n c� ch?c mu?n c?p nh?t tr?ng th�i th�nh '{newStatus}'?", 
                    "X�c nh?n c?p nh?t", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (_adminService.UpdateOrderStatus(_orderId, newStatus))
                        {
                            _currentOrder.Status = newStatus;
                            UpdateStatusDisplay(newStatus);
                            MessageBox.Show("C?p nh?t tr?ng th�i th�nh c�ng!", "Th�nh c�ng", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("L?i khi c?p nh?t tr?ng th�i!", "L?i", 
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

        private void ConfirmOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_currentOrder?.Status == "Pending")
            {
                UpdateOrderStatus("Confirmed");
            }
            else
            {
                MessageBox.Show("Ch? c� th? x�c nh?n ??n h�ng ? tr?ng th�i 'Pending'!", "Th�ng b�o", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CancelOrder_Click(object sender, RoutedEventArgs e)
        {
            var currentStatus = _currentOrder?.Status ?? "";
            
            if (currentStatus == "Shipping" || currentStatus == "Delivered")
            {
                MessageBox.Show("Kh�ng th? h?y ??n h�ng ?ang v?n chuy?n ho?c ?� giao!", "Th�ng b�o", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            var result = MessageBox.Show("B?n c� ch?c mu?n h?y ??n h�ng n�y?", 
                "X�c nh?n h?y", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            
            if (result == MessageBoxResult.Yes)
            {
                UpdateOrderStatus("Cancelled");
            }
        }

        private void AssignCarrier_Click(object sender, RoutedEventArgs e)
        {
            if (CarrierComboBox.SelectedItem is Carrier selectedCarrier)
            {
                var result = MessageBox.Show($"B?n c� ch?c mu?n ch?n '{selectedCarrier.FullName}' l�m ng??i v?n chuy?n?", 
                    "X�c nh?n ch?n ng??i v?n chuy?n", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (_adminService.AssignCarrierToOrder(_orderId, selectedCarrier.CarrierId))
                        {
                            _currentOrder.CarrierId = selectedCarrier.CarrierId;
                            _currentOrder.Carrier = selectedCarrier;
                            
                            CurrentCarrierText.Text = $"{selectedCarrier.FullName} - {selectedCarrier.Phone} ({selectedCarrier.VehicleNumber})";
                            
                            MessageBox.Show("?� ch?n ng??i v?n chuy?n th�nh c�ng!", "Th�nh c�ng", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("L?i khi ch?n ng??i v?n chuy?n!", "L?i", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"L?i khi ch?n ng??i v?n chuy?n: {ex.Message}", "L?i", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui l�ng ch?n ng??i v?n chuy?n!", "Th�ng b�o", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void StartShipping_Click(object sender, RoutedEventArgs e)
        {
            if (_currentOrder?.CarrierId == null)
            {
                MessageBox.Show("Vui l�ng ch?n ng??i v?n chuy?n tr??c khi b?t ??u v?n chuy?n!", "Th�ng b�o", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (_currentOrder?.Status != "Confirmed")
            {
                MessageBox.Show("Ch? c� th? b?t ??u v?n chuy?n ??n h�ng ?� x�c nh?n!", "Th�ng b�o", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            var result = MessageBox.Show("B?n c� ch?c mu?n b?t ??u v?n chuy?n ??n h�ng n�y?", 
                "X�c nh?n v?n chuy?n", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                UpdateOrderStatus("Shipping");
            }
        }

        private void MarkAsDelivered_Click(object sender, RoutedEventArgs e)
        {
            if (_currentOrder?.Status != "Shipping")
            {
                MessageBox.Show("Ch? c� th? x�c nh?n giao h�ng cho ??n h�ng ?ang v?n chuy?n!", "Th�ng b�o", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            var result = MessageBox.Show("B?n c� ch?c mu?n x�c nh?n ??n h�ng n�y ?� ???c giao th�nh c�ng?", 
                "X�c nh?n ?� giao", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                UpdateOrderStatus("Delivered");
            }
        }

        private void UpdateOrderStatus(string newStatus)
        {
            try
            {
                if (_adminService.UpdateOrderStatus(_orderId, newStatus))
                {
                    _currentOrder.Status = newStatus;
                    UpdateStatusDisplay(newStatus);
                    
                    // Update combobox selection
                    foreach (ComboBoxItem item in StatusComboBox.Items)
                    {
                        if (item.Content.ToString() == newStatus)
                        {
                            StatusComboBox.SelectedItem = item;
                            break;
                        }
                    }
                    
                    MessageBox.Show($"?� c?p nh?t tr?ng th�i th�nh '{newStatus}'!", "Th�nh c�ng", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("L?i khi c?p nh?t tr?ng th�i!", "L?i", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i: {ex.Message}", "L?i", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Save notes
                using var context = new DAL.Entities.ShoppingOnlineContext();
                var order = context.Orders.Find(_orderId);
                if (order != null)
                {
                    order.Notes = NotesTextBox.Text;
                    context.SaveChanges();
                    
                    MessageBox.Show("?� l?u thay ??i!", "Th�nh c�ng", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi l?u thay ??i: {ex.Message}", "L?i", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        #endregion
    }
}