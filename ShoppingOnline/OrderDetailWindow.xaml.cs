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
        private Order? _currentOrder;
        private List<OrderDetail>? _orderDetails;

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
                    MessageBox.Show("Khong tim thay don hang!", "Loi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                    return;
                }

                // Load order details
                _orderDetails = _adminService.GetOrderDetailsByOrderId(_orderId);

                // Update UI
                UpdateOrderInfo();
                UpdateOrderItems();
                UpdateStatusDisplay(_currentOrder.Status ?? "Pending");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi khi tai chi tiet don hang: {ex.Message}", "Loi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCarriers()
        {
            try
            {
                var carriers = _adminService.GetAvailableCarriers();
                CarrierComboBox.ItemsSource = carriers;
                
                if (_currentOrder?.CarrierId.HasValue == true)
                {
                    var currentCarrier = carriers.FirstOrDefault(c => c.CarrierId == _currentOrder.CarrierId);
                    CurrentCarrierText.Text = currentCarrier?.FullName ?? "Khong xac dinh";
                    CarrierComboBox.SelectedValue = _currentOrder.CarrierId;
                }
                else
                {
                    CurrentCarrierText.Text = "Chua chon nguoi giao hang";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi khi tai danh sach nguoi giao hang: {ex.Message}", "Loi", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateOrderInfo()
        {
            if (_currentOrder == null) return;

            // Check if order is cancelled
            bool isCancelled = _currentOrder.Notes?.Contains("[CANCELLED]") == true;

            // Order header
            OrderTitleText.Text = $"Chi tiet don hang #{_currentOrder.OrderId}";
            OrderDateText.Text = $"Ngay dat: {_currentOrder.OrderDate:dd/MM/yyyy HH:mm}";

            // Customer info
            CustomerNameText.Text = _currentOrder.Customer?.FullName ?? "Khong xac dinh";
            CustomerPhoneText.Text = _currentOrder.Phone ?? "Khong co";
            CustomerEmailText.Text = _currentOrder.Customer?.Account?.Email ?? "Khong co";
            ShippingAddressText.Text = _currentOrder.ShippingAddress ?? "Khong co";

            // Total amount
            TotalAmountText.Text = $"{_currentOrder.TotalAmount:N0}d";

            // Notes
            NotesTextBox.Text = _currentOrder.Notes ?? "";

            // Update UI based on cancellation status
            UpdateUIForCancelledOrder(isCancelled);

            // Status combobox
            if (isCancelled)
            {
                // Show cancelled status in combobox
                foreach (ComboBoxItem item in StatusComboBox.Items)
                {
                    if (item.Content?.ToString() == "Cancelled")
                    {
                        StatusComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
            else
            {
                // Status combobox normal behavior
                foreach (ComboBoxItem item in StatusComboBox.Items)
                {
                    if (item.Content?.ToString() == _currentOrder.Status)
                    {
                        StatusComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void UpdateUIForCancelledOrder(bool isCancelled)
        {
            // Disable/Enable UI elements based on cancellation status
            StatusComboBox.IsEnabled = !isCancelled;
            UpdateStatusButton.IsEnabled = !isCancelled;
            ConfirmOrderButton.IsEnabled = !isCancelled;
            CancelOrderButton.IsEnabled = !isCancelled;

            // Set tooltips
            if (isCancelled)
            {
                var tooltip = "Don hang da bi huy - khong the thuc hien thao tac nay";
                StatusComboBox.ToolTip = tooltip;
                UpdateStatusButton.ToolTip = tooltip;
                ConfirmOrderButton.ToolTip = tooltip;
                CancelOrderButton.ToolTip = "Don hang da bi huy";
            }
            else
            {
                StatusComboBox.ToolTip = null;
                UpdateStatusButton.ToolTip = null;
                ConfirmOrderButton.ToolTip = null;
                CancelOrderButton.ToolTip = null;
            }

            // Visual feedback for disabled state
            if (isCancelled)
            {
                StatusComboBox.Opacity = 0.5;
                UpdateStatusButton.Opacity = 0.5;
                ConfirmOrderButton.Opacity = 0.5;
                CancelOrderButton.Opacity = 0.5;
            }
            else
            {
                StatusComboBox.Opacity = 1.0;
                UpdateStatusButton.Opacity = 1.0;
                ConfirmOrderButton.Opacity = 1.0;
                CancelOrderButton.Opacity = 1.0;
            }
        }

        private void UpdateOrderItems()
        {
            if (_orderDetails != null)
            {
                OrderItemsGrid.ItemsSource = _orderDetails;
            }
        }

        private void UpdateStatusDisplay(string status)
        {
            // Check if order is cancelled based on notes
            bool isCancelled = _currentOrder?.Notes?.Contains("[CANCELLED]") == true;
            
            if (isCancelled)
            {
                CurrentStatusText.Text = "Da huy";
                StatusBorder.Background = new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Red
                return;
            }
            
            CurrentStatusText.Text = GetStatusDisplayText(status);
            
            // Update status border color
            StatusBorder.Background = status switch
            {
                "Pending" => new SolidColorBrush(Color.FromRgb(255, 152, 0)),    // Orange
                "Confirmed" => new SolidColorBrush(Color.FromRgb(76, 175, 80)),  // Green
                "Preparing" => new SolidColorBrush(Color.FromRgb(156, 39, 176)), // Purple
                "Shipping" => new SolidColorBrush(Color.FromRgb(33, 150, 243)),  // Blue
                "Delivered" => new SolidColorBrush(Color.FromRgb(139, 195, 74)), // Light Green
                "Completed" => new SolidColorBrush(Color.FromRgb(76, 175, 80)),  // Green
                _ => new SolidColorBrush(Color.FromRgb(158, 158, 158))           // Gray
            };
        }

        private string GetStatusDisplayText(string status)
        {
            // Check if order is cancelled based on notes
            if (_currentOrder?.Notes?.Contains("[CANCELLED]") == true)
            {
                return "Da huy";
            }
            
            return status switch
            {
                "Pending" => "Cho xac nhan",
                "Confirmed" => "Da xac nhan", 
                "Preparing" => "Dang chuan bi",
                "Shipping" => "Dang giao hang",
                "Delivered" => "Da giao thanh cong",
                "Completed" => "Da hoan thanh",
                _ => "Khong xac dinh"
            };
        }

        #region Event Handlers
        private void UpdateStatus_Click(object sender, RoutedEventArgs e)
        {
            if (StatusComboBox.SelectedItem is ComboBoxItem selectedItem && _currentOrder != null)
            {
                var newStatus = selectedItem.Content?.ToString();
                if (newStatus != null && newStatus != _currentOrder.Status)
                {
                    // Refresh order data to ensure we have latest info
                    RefreshOrderData();
                    
                    var result = MessageBox.Show($"CAP NHAT TRANG THAI DON HANG?\n\n" +
                                               $"Don hang: #{_orderId}\n" +
                                               $"Trang thai hien tai: {GetStatusDisplayText(_currentOrder.Status ?? "")}\n" +
                                               $"Trang thai moi: {GetStatusDisplayText(newStatus)}\n\n" +
                                               $"Ban co chac chan muon thay doi?", 
                        "Xac nhan cap nhat", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        UpdateOrderStatus(newStatus);
                    }
                    else
                    {
                        // Reset combobox to current status
                        foreach (ComboBoxItem item in StatusComboBox.Items)
                        {
                            if (item.Content?.ToString() == _currentOrder.Status)
                            {
                                StatusComboBox.SelectedItem = item;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("TRANG THAI KHONG THAY DOI!\n\n" +
                                  "Trang thai ban chon giong voi trang thai hien tai.\n" +
                                  "Khong can cap nhat.", 
                                  "Thong bao", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void RefreshOrderData()
        {
            try
            {
                var refreshedOrder = _adminService.GetOrderById(_orderId);
                if (refreshedOrder != null)
                {
                    var previousStatus = _currentOrder?.Status;
                    _currentOrder = refreshedOrder;
                    
                    // Update UI if status changed
                    if (previousStatus != _currentOrder.Status)
                    {
                        UpdateStatusDisplay(_currentOrder.Status ?? "Pending");
                        
                        // Update combobox
                        foreach (ComboBoxItem item in StatusComboBox.Items)
                        {
                            if (item.Content?.ToString() == _currentOrder.Status)
                            {
                                StatusComboBox.SelectedItem = item;
                                break;
                            }
                        }
                        
                        MessageBox.Show($"THONG TIN DON HANG DA DUOC CAP NHAT!\n\n" +
                                      $"Trang thai da thay doi tu '{GetStatusDisplayText(previousStatus ?? "")}' " +
                                      $"sang '{GetStatusDisplayText(_currentOrder.Status ?? "")}'\n\n" +
                                      $"Co the do nguoi khac da cap nhat don hang nay.", 
                                      "Thong bao cap nhat", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing order data: {ex.Message}");
            }
        }

        private void ConfirmOrder_Click(object sender, RoutedEventArgs e)
        {
            // Check if order is already cancelled
            if (_currentOrder?.Notes?.Contains("[CANCELLED]") == true)
            {
                MessageBox.Show("DON HANG DA BI HUY!\n\n" +
                              "Khong the xac nhan don hang da bi huy.\n" +
                              "Vui long tao don hang moi neu can thiet.", 
                              "Don hang da bi huy", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_currentOrder?.Status != "Pending")
            {
                MessageBox.Show("Chi co the xac nhan don hang o trang thai 'Pending'!", "Thong bao", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Ban co chac muon xac nhan don hang nay?", 
                "Xac nhan don hang", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                UpdateOrderStatus("Confirmed");
            }
        }

        private void CancelOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_currentOrder?.Status == "Delivered" || _currentOrder?.Status == "Completed")
            {
                MessageBox.Show("Khong the huy don hang da duoc giao hoac hoan thanh!", "Thong bao", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Ban co chac muon huy don hang nay?\nHanh dong nay khong the hoan tac!", 
                "Huy don hang", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // According to database constraint, valid statuses are:
                    // 'Pending', 'Confirmed', 'Preparing', 'Shipping', 'Delivered', 'Completed'
                    // Since 'Cancelled' is not allowed, we'll keep it as 'Pending' but add a note
                    
                    System.Diagnostics.Debug.WriteLine($"Attempting to cancel order {_orderId}");
                    
                    // Instead of changing status to 'Cancelled', we'll:
                    // 1. Keep status as current
                    // 2. Add cancellation note
                    // 3. Update UI to show as cancelled
                    
                    // Update notes to indicate cancellation
                    var cancellationNote = $"[CANCELLED] Don hang da bi huy luc {DateTime.Now:dd/MM/yyyy HH:mm}. " +
                                         $"Ly do: Yeu cau huy tu admin. ";
                    
                    if (!string.IsNullOrEmpty(_currentOrder.Notes))
                    {
                        cancellationNote += $"Ghi chu cu: {_currentOrder.Notes}";
                    }
                    
                    // Update notes in database
                    using var context = new DAL.Entities.ShoppingOnlineContext();
                    var order = context.Orders.Find(_orderId);
                    if (order != null)
                    {
                        order.Notes = cancellationNote;
                        var changes = context.SaveChanges();
                        
                        if (changes > 0)
                        {
                            // Update local order
                            _currentOrder.Notes = cancellationNote;
                            NotesTextBox.Text = cancellationNote;
                            
                            // Update status display to show as cancelled (visually)
                            CurrentStatusText.Text = "Da huy (ghi chu)";
                            StatusBorder.Background = new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Red
                            
                            MessageBox.Show($"THANH CONG!\n\nDa danh dau huy don hang #{_orderId}\n\n" +
                                          $"Luu y:\n" +
                                          $"- Don hang duoc danh dau da huy trong ghi chu\n" +
                                          $"- Trang thai database van giu nguyen do han che cua he thong\n" +
                                          $"- Thong tin huy duoc luu trong phan ghi chu\n\n" +
                                          $"Thoi gian huy: {DateTime.Now:dd/MM/yyyy HH:mm}", 
                                          "Huy don hang thanh cong", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Khong the cap nhat thong tin huy don hang!", "Loi", 
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
                    var errorDetails = ex.InnerException?.Message ?? ex.Message;
                    
                    MessageBox.Show($"LOI KHI HUY DON HANG!\n\n" +
                                  $"Don hang #{_orderId}\n" +
                                  $"Trang thai hien tai: {_currentOrder.Status}\n\n" +
                                  $"CHI TIET LOI:\n" +
                                  $"{errorDetails}\n\n" +
                                  $"NGUYEN NHAN:\n" +
                                  $"Database CHECK constraint khong cho phep trang thai 'Cancelled'.\n" +
                                  $"Cac trang thai hop le: Pending, Confirmed, Preparing, Shipping, Delivered, Completed\n\n" +
                                  $"GIAI PHAP THAY THE:\n" +
                                  $"1. Danh dau huy trong ghi chu (khong doi trang thai)\n" +
                                  $"2. Hoac lien he DBA de cap nhat database schema\n" +
                                  $"3. Thu giu trang thai hien tai nhung them thong tin huy", 
                                  "Loi database constraint", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void UpdateOrderStatus(string newStatus)
        {
            try
            {
                // Enhanced validation before updating
                if (_currentOrder == null)
                {
                    MessageBox.Show("KHONG TIM THAY THONG TIN DON HANG!\n\n" +
                                  "Vui long dong cua so va mo lai.", 
                                  "Loi du lieu", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Check if order is already cancelled
                if (_currentOrder.Notes?.Contains("[CANCELLED]") == true)
                {
                    MessageBox.Show("DON HANG DA BI HUY!\n\n" +
                                  $"Don hang #{_orderId} da bi huy truoc do.\n" +
                                  $"Khong the thay doi trang thai cua don hang da huy.\n\n" +
                                  $"THONG TIN HUY:\n" +
                                  $"{_currentOrder.Notes}\n\n" +
                                  $"HUONG DAN:\n" +
                                  $"- Neu can khoi phuc don hang, lien he IT\n" +
                                  $"- Hoac tao don hang moi", 
                                  "Don hang da bi huy", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Simple validation - just check if we're trying to use 'Cancelled'
                if (newStatus == "Cancelled")
                {
                    MessageBox.Show("TRANG THAI 'Cancelled' KHONG HOP LE!\n\n" +
                                  "Database chi cho phep cac trang thai:\n" +
                                  "- Pending\n- Confirmed\n- Preparing\n- Shipping\n- Delivered\n- Completed\n\n" +
                                  "GIAI PHAP:\n" +
                                  "1. Su dung nut 'Huy don hang' de danh dau huy\n" +
                                  "2. Hoac chon mot trang thai hop le khac", 
                                  "Loi constraint database", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Add debug logging
                System.Diagnostics.Debug.WriteLine($"Attempting to update order {_orderId} to status {newStatus}");
                
                if (_adminService.UpdateOrderStatus(_orderId, newStatus))
                {
                    _currentOrder.Status = newStatus;
                    UpdateStatusDisplay(newStatus);
                    
                    // Update combobox selection
                    foreach (ComboBoxItem item in StatusComboBox.Items)
                    {
                        if (item.Content?.ToString() == newStatus)
                        {
                            StatusComboBox.SelectedItem = item;
                            break;
                        }
                    }
                    
                    MessageBox.Show($"THANH CONG!\n\nDa cap nhat trang thai don hang #{_orderId}\n" +
                                  $"Trang thai moi: {GetStatusDisplayText(newStatus)}", 
                                  "Cap nhat thanh cong", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"LOI KHI CAP NHAT TRANG THAI!\n\n" +
                                  $"Don hang #{_orderId}\n" +
                                  $"Trang thai: {_currentOrder.Status} ? {newStatus}\n\n" +
                                  $"NGUYEN NHAN CO THE:\n" +
                                  $"• Database CHECK constraint violation\n" +
                                  $"• Don hang da bi thay doi boi nguoi khac\n" +
                                  $"• Conflict khi luu du lieu\n\n" +
                                  $"HUONG DAN XU LY:\n" +
                                  $"1. Thu lai sau vai giay\n" +
                                  $"2. Dong cua so va mo lai de cap nhat du lieu moi nhat\n" +
                                  $"3. Kiem tra cac trang thai hop le", 
                                  "Loi cap nhat trang thai", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"LOI HE THONG NGHIEM TRONG!\n\n" +
                              $"Don hang: #{_orderId}\n" +
                              $"Thao tac: Cap nhat trang thai sang '{newStatus}'\n\n" +
                              $"CHI TIET LOI:\n" +
                              $"{ex.Message}\n\n" +
                              $"INNER EXCEPTION:\n" +
                              $"{ex.InnerException?.Message}\n\n" +
                              $"HUONG DAN XU LY:\n" +
                              $"1. Dong cua so va mo lai\n" +
                              $"2. Kiem tra ket noi mang\n" +
                              $"3. Khoi dong lai ung dung neu can\n" +
                              $"4. Lien he IT voi thong tin loi tren", 
                              "Loi nghiem trong", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AssignCarrier_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chuc nang dang phat trien!", "Thong bao", 
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void StartShipping_Click(object sender, RoutedEventArgs e)
        {
            // Check if order is already cancelled
            if (_currentOrder?.Notes?.Contains("[CANCELLED]") == true)
            {
                MessageBox.Show("DON HANG DA BI HUY!\n\n" +
                              "Khong the bat dau giao hang cho don hang da bi huy.", 
                              "Don hang da bi huy", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Bat dau giao hang don hang #{_orderId}?", 
                                       "Xac nhan", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                UpdateOrderStatus("Shipping");
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
                    
                    MessageBox.Show("Da luu thay doi!", "Thanh cong", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi khi luu thay doi: {ex.Message}", "Loi", 
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