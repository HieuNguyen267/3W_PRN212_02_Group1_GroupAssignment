using BLL.Services;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ShoppingOnline
{
    public partial class CarrierSelectionDialog : Window, INotifyPropertyChanged
    {
        private readonly IAdminService _adminService;
        private readonly int _orderId;
        private Order? _currentOrder;
        private List<Carrier>? _availableCarriers;
        private Carrier? _selectedCarrier;

        public CarrierSelectionDialog(int orderId)
        {
            InitializeComponent();
            _adminService = new AdminService();
            _orderId = orderId;
            this.DataContext = this;
            
            LoadOrderAndCarriers();
        }

        public Carrier? SelectedCarrier
        {
            get => _selectedCarrier;
            set
            {
                _selectedCarrier = value;
                OnPropertyChanged(nameof(SelectedCarrier));
            }
        }

        private void LoadOrderAndCarriers()
        {
            try
            {
                // Load order information
                _currentOrder = _adminService.GetOrderById(_orderId);
                if (_currentOrder == null)
                {
                    MessageBox.Show("Không tìm thấy đơn hàng!", "Lỗi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                    return;
                }

                // Update title
                OrderTitleText.Text = $"Chon nguoi giao hang cho don hang #{_orderId}";

                // Show current carrier if any
                UpdateCurrentCarrierDisplay();

                // Load available carriers
                LoadCarriers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải thông tin: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateCurrentCarrierDisplay()
        {
            if (_currentOrder?.CarrierId.HasValue == true && _currentOrder.Carrier != null)
            {
                CurrentCarrierText.Text = $"Ten: {_currentOrder.Carrier.FullName}\n" +
                                        $"So dien thoai: {_currentOrder.Carrier.Phone ?? "Khong co"}\n" +
                                        $"So xe: {_currentOrder.Carrier.VehicleNumber ?? "Khong co"}";
            }
            else
            {
                CurrentCarrierText.Text = "Chua chon nguoi giao hang";
            }
        }

        private void LoadCarriers()
        {
            try
            {
                _availableCarriers = _adminService.GetAvailableCarriers();
                
                if (_availableCarriers?.Any() == true)
                {
                    CarriersGrid.ItemsSource = _availableCarriers;
                    CarriersGrid.Visibility = Visibility.Visible;
                    EmptyStatePanel.Visibility = Visibility.Collapsed;
                }
                else
                {
                    CarriersGrid.Visibility = Visibility.Collapsed;
                    EmptyStatePanel.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách người giao hàng: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectCarrier_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int carrierId)
            {
                try
                {
                    var selectedCarrier = _availableCarriers?.FirstOrDefault(c => c.CarrierId == carrierId);
                    if (selectedCarrier == null)
                    {
                        MessageBox.Show("Không tìm thấy thông tin người giao hàng!", "Lỗi", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Check if the same carrier is already assigned
                    if (_currentOrder?.CarrierId == carrierId)
                    {
                        MessageBox.Show($"Nguoi giao hang {selectedCarrier.FullName} da duoc gan cho don hang nay!", 
                            "Thong bao", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    // Show confirmation dialog
                    var result = MessageBox.Show($"CHON NGUOI GIAO HANG\n\n" +
                                               $"Đơn hàng: #{_orderId}\n" +
                                               $"Người giao hàng: {selectedCarrier.FullName}\n" +
                                               $"Số điện thoại: {selectedCarrier.Phone ?? "Không có"}\n" +
                                               $"Số xe: {selectedCarrier.VehicleNumber ?? "Không có"}\n\n" +
                                               $"Bạn có chắc chắn muốn gán người giao hàng này?", 
                        "Xác nhận chọn người giao hàng", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Assign carrier to order
                        if (_adminService.AssignCarrierToOrder(_orderId, carrierId))
                        {
                            // Update current order
                            if (_currentOrder != null)
                            {
                                _currentOrder.CarrierId = carrierId;
                                _currentOrder.Carrier = selectedCarrier;
                            }

                            // Update display
                            UpdateCurrentCarrierDisplay();

                            MessageBox.Show($"THÀNH CÔNG!\n\n" +
                                          $"Đã gán người giao hàng {selectedCarrier.FullName} cho đơn hàng #{_orderId}.\n\n" +
                                          $"Thông tin liên hệ:\n" +
                                          $"- Tên: {selectedCarrier.FullName}\n" +
                                          $"- Số điện thoại: {selectedCarrier.Phone ?? "Không có"}\n" +
                                          $"- Số xe: {selectedCarrier.VehicleNumber ?? "Không có"}", 
                                          "Gán người giao hàng thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                            // Set dialog result and close
                            this.DialogResult = true;
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Lỗi khi gán người giao hàng. Vui lòng thử lại!", "Lỗi", 
                                          MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi chọn người giao hàng: {ex.Message}", "Lỗi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}