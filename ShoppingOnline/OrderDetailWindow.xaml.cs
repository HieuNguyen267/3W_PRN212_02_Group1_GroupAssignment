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
        private readonly int _orderId;
        private Order _currentOrder;
        private List<OrderDetail> _orderDetails;
        private List<Carrier> _carriers;

        public OrderDetailWindow(int orderId)
        {
            InitializeComponent();
            _adminService = new AdminService();
            _orderId = orderId;
            
            LoadOrderDetails();
            LoadCarriers();
        }

        private void LoadOrderDetails()
        {
            try
            {
                // Load order
                _currentOrder = _adminService.GetOrderById(_orderId);
                if (_currentOrder == null)
                {
                    MessageBox.Show("Kh�ng t�m th?y ??n h�ng!", "L?i", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                    return;
                }

                // Load order details
                _orderDetails = _adminService.GetOrderDetailsByOrderId(_orderId);

                // Update UI
                UpdateOrderInfo();
                UpdateOrderItems();
                UpdateStatusDisplay(_currentOrder.Status);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi t?i chi ti?t ??n h�ng: {ex.Message}", "L?i", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCarriers()
        {
            try
            {
                // Load available carriers from database
                _carriers = _adminService.GetAvailableCarriers();

                CarrierComboBox.ItemsSource = _carriers;
                CarrierComboBox.DisplayMemberPath = "FullName";
                CarrierComboBox.SelectedValuePath = "CarrierId";
                
                // Update current carrier display
                if (_currentOrder.CarrierId.HasValue)
                {
                    var currentCarrier = _carriers.FirstOrDefault(c => c.CarrierId == _currentOrder.CarrierId);
                    CurrentCarrierText.Text = currentCarrier?.FullName ?? "Kh�ng x�c ??nh";
                    CarrierComboBox.SelectedValue = _currentOrder.CarrierId;
                }
                else
                {
                    CurrentCarrierText.Text = "Ch?a ch?n";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi t?i danh s�ch ng??i v?n chuy?n: {ex.Message}", "L?i", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateOrderInfo()
        {
            // Order header
            OrderTitleText.Text = $"Chi ti?t ??n h�ng #{_currentOrder.OrderId}";
            OrderDateText.Text = $"Ng�y ??t: {_currentOrder.OrderDate:dd/MM/yyyy HH:mm}";

            // Customer info
            CustomerNameText.Text = _currentOrder.Customer?.FullName ?? "Kh�ng x�c ??nh";
            CustomerPhoneText.Text = _currentOrder.Phone ?? "Kh�ng c�";
            CustomerEmailText.Text = _currentOrder.Customer?.Account?.Email ?? "Kh�ng c�";
            ShippingAddressText.Text = _currentOrder.ShippingAddress ?? "Kh�ng c�";

            // Total amount
            TotalAmountText.Text = $"{_currentOrder.TotalAmount:N0}?";

            // Notes
            NotesTextBox.Text = _currentOrder.Notes ?? "";

            // Status combobox
            foreach (ComboBoxItem item in StatusComboBox.Items)
            {
                if (item.Content.ToString() == _currentOrder.Status)
                {
                    StatusComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void UpdateOrderItems()
        {
            OrderItemsGrid.ItemsSource = _orderDetails;
        }

        private void UpdateStatusDisplay(string status)
        {
            CurrentStatusText.Text = GetStatusDisplayText(status);
            
            // Update status border color
            StatusBorder.Background = status switch
            {
                "Pending" => new SolidColorBrush(Color.FromRgb(255, 152, 0)),    // Orange
                "Confirmed" => new SolidColorBrush(Color.FromRgb(76, 175, 80)),  // Green
                "Shipping" => new SolidColorBrush(Color.FromRgb(33, 150, 243)),  // Blue
                "Delivered" => new SolidColorBrush(Color.FromRgb(139, 195, 74)), // Light Green
                "Cancelled" => new SolidColorBrush(Color.FromRgb(244, 67, 54)),  // Red
                _ => new SolidColorBrush(Color.FromRgb(158, 158, 158))           // Gray
            };
        }

        private string GetStatusDisplayText(string status)
        {
            return status switch
            {
                "Pending" => "? Ch? x�c nh?n",
                "Confirmed" => "? ?� x�c nh?n",
                "Shipping" => "?? ?ang giao h�ng",
                "Delivered" => "? ?� giao th�nh c�ng",
                "Cancelled" => "? ?� h?y",
                _ => "? Kh�ng x�c ??nh"
            };
        }

        #region Event Handlers
        private void UpdateStatus_Click(object sender, RoutedEventArgs e)
        {
            if (StatusComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var newStatus = selectedItem.Content.ToString();
                if (newStatus != _currentOrder.Status)
                {
                    var result = MessageBox.Show($"B?n c� ch?c mu?n c?p nh?t tr?ng th�i th�nh '{GetStatusDisplayText(newStatus)}'?", 
                        "X�c nh?n c?p nh?t", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        UpdateOrderStatus(newStatus);
                    }
                }
                else
                {
                    MessageBox.Show("Tr?ng th�i hi?n t?i ?� l� tr?ng th�i ???c ch?n!", "Th�ng b�o", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void ConfirmOrder_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("B?n c� ch?c mu?n x�c nh?n ??n h�ng n�y?", 
                "X�c nh?n ??n h�ng", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                UpdateOrderStatus("Confirmed");
            }
        }

        private void CancelOrder_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("B?n c� ch?c mu?n h?y ??n h�ng n�y?\nH�nh ??ng n�y kh�ng th? ho�n t�c!", 
                "H?y ??n h�ng", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            
            if (result == MessageBoxResult.Yes)
            {
                UpdateOrderStatus("Cancelled");
            }
        }

        private void AssignCarrier_Click(object sender, RoutedEventArgs e)
        {
            if (CarrierComboBox.SelectedValue is int carrierId)
            {
                var carrier = _carriers.FirstOrDefault(c => c.CarrierId == carrierId);
                if (carrier != null)
                {
                    var result = MessageBox.Show($"B?n c� ch?c mu?n giao ??n h�ng cho '{carrier.FullName}'?\nS?T: {carrier.Phone}\nPh??ng ti?n: {carrier.VehicleNumber}", 
                        "X�c nh?n ch?n ng??i v?n chuy?n", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            if (_adminService.AssignCarrierToOrder(_orderId, carrierId))
                            {
                                _currentOrder.CarrierId = carrierId;
                                CurrentCarrierText.Text = carrier.FullName;
                                
                                MessageBox.Show($"?� giao ??n h�ng cho '{carrier.FullName}' th�nh c�ng!", "Th�nh c�ng", 
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show("L?i khi giao ??n h�ng cho ng??i v?n chuy?n!", "L?i", 
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
            else
            {
                MessageBox.Show("Vui l�ng ch?n ng??i v?n chuy?n!", "Th�ng b�o", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void StartShipping_Click(object sender, RoutedEventArgs e)
        {
            if (!_currentOrder.CarrierId.HasValue)
            {
                MessageBox.Show("Vui l�ng ch?n ng??i v?n chuy?n tr??c khi b?t ??u v?n chuy?n!", "Th�ng b�o", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var carrier = _carriers.FirstOrDefault(c => c.CarrierId == _currentOrder.CarrierId);
            var result = MessageBox.Show($"B?n c� ch?c mu?n b?t ??u v?n chuy?n ??n h�ng n�y?\nNg??i giao h�ng: {carrier?.FullName}", 
                "B?t ??u v?n chuy?n", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                UpdateOrderStatus("Shipping");
            }
        }

        private void MarkAsDelivered_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("B?n c� ch?c r?ng ??n h�ng n�y ?� ???c giao th�nh c�ng?", 
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
                    
                    MessageBox.Show($"?� c?p nh?t tr?ng th�i th�nh '{GetStatusDisplayText(newStatus)}'!", "Th�nh c�ng", 
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