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
    public partial class AdminCarriersView : UserControl, INotifyPropertyChanged
    {
        private readonly CarrierService _carrierService;
        private ObservableCollection<Carrier> _carriers = new();
        private List<Carrier> _allCarriers = new();

        public AdminCarriersView()
        {
            InitializeComponent();
            _carrierService = new CarrierService();
            DataContext = this;
            
            Loaded += AdminCarriersView_Loaded;
        }

        private void AdminCarriersView_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCarriers();
        }

        public ObservableCollection<Carrier> Carriers
        {
            get => _carriers;
            set
            {
                _carriers = value;
                OnPropertyChanged(nameof(Carriers));
            }
        }

        private void LoadCarriers()
        {
            try
            {
                _allCarriers = _carrierService.GetAllCarriers();
                ApplyCarrierFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu nhà vận chuyển: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyCarrierFilter()
        {
            try
            {
                var filteredCarriers = _allCarriers.AsEnumerable();

                // Apply search filter
                var searchText = CarrierSearchBox?.Text?.Trim();
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    var searchLower = searchText.ToLower();
                    filteredCarriers = filteredCarriers.Where(c => 
                        (c.FullName?.ToLower().Contains(searchLower) == true) ||
                        (c.Account?.Email?.ToLower().Contains(searchLower) == true) ||
                        (c.Phone?.ToLower().Contains(searchLower) == true) ||
                        (c.VehicleNumber?.ToLower().Contains(searchLower) == true));
                }

                // Apply status filter
                var selectedStatus = (StatusFilterComboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString();
                if (!string.IsNullOrEmpty(selectedStatus) && selectedStatus != "All Status")
                {
                    bool isActive = selectedStatus == "Active";
                    // "Locked" corresponds to inactive accounts
                    filteredCarriers = filteredCarriers.Where(c => c.Account?.IsActive == isActive);
                }

                // Update carriers collection
                Carriers.Clear();
                foreach (var carrier in filteredCarriers.OrderBy(c => c.FullName))
                {
                    Carriers.Add(carrier);
                }

                // Update DataGrid
                if (CarriersDataGrid != null)
                {
                    CarriersDataGrid.ItemsSource = Carriers;
                }
                
                // Update count
                if (CarrierCountText != null)
                {
                    CarrierCountText.Text = $"Total: {filteredCarriers.Count()} carriers";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lọc nhà vận chuyển: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Event Handlers
        private void CarrierSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CarrierSearchBox != null)
            {
                CarrierSearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(CarrierSearchBox.Text) 
                    ? Visibility.Visible : Visibility.Hidden;
                
                ApplyCarrierFilter();
            }
        }

        private void StatusFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyCarrierFilter();
        }

        private void RefreshCarriers_Click(object sender, RoutedEventArgs e)
        {
            LoadCarriers();
            MessageBox.Show(" làm mới danh sách nhà vận chuyển!", "Thông báo", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddCarrier_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var addCarrierWindow = new CarrierEditWindow();
                if (addCarrierWindow.ShowDialog() == true)
                {
                    LoadCarriers();
                    MessageBox.Show("Thêm nhà vận chuyển thành công!", "Thành công", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm nhà vận chuyển: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewCarrier_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int carrierId)
            {
                try
                {
                    var carrier = _allCarriers.FirstOrDefault(c => c.CarrierId == carrierId);
                    if (carrier != null)
                    {
                        var info = $"Thông tin nhà vận chuyển #{carrierId}\n\n" +
                                  $"Tên: {carrier.FullName}\n" +
                                  $"Email: {carrier.Account?.Email}\n" +
                                  $"Điện thoại: {carrier.Phone}\n" +
                                  $"Số xe: {carrier.VehicleNumber}\n" +
                                  $"Ngày tạo: {carrier.CreatedDate:dd/MM/yyyy}\n" +
                                  $"Trạng thái TK: {(carrier.Account?.IsActive == true ? "Hoạt động" : "Khóa")}\n" +
                                  $"Sẵn sàng giao hàng: {(carrier.IsAvailable == true ? "Có" : "Không")}";

                        MessageBox.Show(info, "Thông tin nhà vận chuyển", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xem thông tin nhà vận chuyển: {ex.Message}", "Lỗi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditCarrier_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int carrierId)
            {
                try
                {
                    var carrier = _allCarriers.FirstOrDefault(c => c.CarrierId == carrierId);
                    if (carrier != null)
                    {
                        var editCarrierWindow = new CarrierEditWindow(carrier);
                        if (editCarrierWindow.ShowDialog() == true)
                        {
                            LoadCarriers();
                            MessageBox.Show("Cập nhật thông tin nhà vận chuyển thành công!", "Thành công", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi sửa thông tin nhà vận chuyển: {ex.Message}", "Lỗi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteCarrier_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int carrierId)
            {
                var result = MessageBox.Show($"Bạn có chắc muốn xóa nhà vận chuyển #{carrierId}?\n" +
                    "Việc này sẽ vô hiệu hóa tài khoản của nhà vận chuyển.", 
                    "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (_carrierService.DeleteCarrier(carrierId))
                        {
                            LoadCarriers();
                            MessageBox.Show("Đã xóa nhà vận chuyển thành công!", "Thành công", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Không thể xóa nhà vận chuyển!", "Lỗi", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi xóa nhà vận chuyển: {ex.Message}", "Lỗi", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ToggleCarrierAvailable_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int carrierId)
            {
                try
                {
                    var carrier = _allCarriers.FirstOrDefault(c => c.CarrierId == carrierId);
                    if (carrier != null)
                    {
                        bool newStatus = !carrier.IsAvailable.GetValueOrDefault();
                        string statusText = newStatus ? "đặt sẵn sàng" : "đặt không sẵn sàng";

                        var result = MessageBox.Show($"Bạn có muốn {statusText} giao hàng cho nhà vận chuyển này?", 
                            "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        
                        if (result == MessageBoxResult.Yes)
                        {
                            if (_carrierService.UpdateCarrierAvailable(carrierId, newStatus))
                            {
                                LoadCarriers();
                                MessageBox.Show($"Đã {statusText} giao hàng thành công!", "Thành công", 
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show("Không thể cập nhật trạng thái sẵn sàng!", "Lỗi", 
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi cập nhật trạng thái: {ex.Message}", "Lỗi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ToggleCarrierStatus_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int carrierId)
            {
                try
                {
                    var carrier = _allCarriers.FirstOrDefault(c => c.CarrierId == carrierId);
                    if (carrier?.Account != null)
                    {
                        bool newStatus = !carrier.Account.IsActive.GetValueOrDefault();
                        string statusText = newStatus ? "kích hoạt" : "vô hiệu hóa";

                        var result = MessageBox.Show($"Bạn có muốn {statusText} tài khoản nhà vận chuyển này?", 
                            "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        
                        if (result == MessageBoxResult.Yes)
                        {
                            if (_carrierService.UpdateAccountStatus(carrier.Account.AccountId, newStatus))
                            {
                                LoadCarriers();
                                MessageBox.Show($"Đã {statusText} tài khoản thành công!", "Thành công", 
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show("Không thể cập nhật trạng thái tài khoản!", "Lỗi", 
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi cập nhật trạng thái: {ex.Message}", "Lỗi", 
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
    }
}