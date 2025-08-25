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
        private readonly IAdminService _adminService;
        private ObservableCollection<Carrier> _carriers = new();
        private List<Carrier> _allCarriers = new();

        public AdminCarriersView()
        {
            InitializeComponent();
            _adminService = new AdminService();
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
                _allCarriers = _adminService.GetAllCarriers();
                ApplyCarrierFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi t?i d? li?u nhà v?n chuy?n: {ex.Message}", "L?i", 
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
                if (!string.IsNullOrEmpty(selectedStatus) && selectedStatus != "T?t c? tr?ng thái")
                {
                    bool isActive = selectedStatus == "Ho?t ??ng";
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
                    CarrierCountText.Text = $"T?ng: {filteredCarriers.Count()} nhà v?n chuy?n";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi l?c nhà v?n chuy?n: {ex.Message}", "L?i", 
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
            MessageBox.Show("?ã làm m?i danh sách nhà v?n chuy?n!", "Thông báo", 
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
                    MessageBox.Show("Thêm nhà v?n chuy?n thành công!", "Thành công", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi thêm nhà v?n chuy?n: {ex.Message}", "L?i", 
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
                        var info = $"Thông tin nhà v?n chuy?n #{carrierId}\n\n" +
                                  $"Tên: {carrier.FullName}\n" +
                                  $"Email: {carrier.Account?.Email}\n" +
                                  $"?i?n tho?i: {carrier.Phone}\n" +
                                  $"S? xe: {carrier.VehicleNumber}\n" +
                                  $"Ngày t?o: {carrier.CreatedDate:dd/MM/yyyy}\n" +
                                  $"Tr?ng thái TK: {(carrier.Account?.IsActive == true ? "Ho?t ??ng" : "Khóa")}\n" +
                                  $"S?n sàng giao hàng: {(carrier.IsAvailable == true ? "Có" : "Không")}";
                        
                        MessageBox.Show(info, "Thông tin nhà v?n chuy?n", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"L?i khi xem thông tin nhà v?n chuy?n: {ex.Message}", "L?i", 
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
                            MessageBox.Show("C?p nh?t thông tin nhà v?n chuy?n thành công!", "Thành công", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"L?i khi s?a thông tin nhà v?n chuy?n: {ex.Message}", "L?i", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteCarrier_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int carrierId)
            {
                var result = MessageBox.Show($"B?n có ch?c mu?n xóa nhà v?n chuy?n #{carrierId}?\n" +
                    "Vi?c này s? vô hi?u hóa tài kho?n c?a nhà v?n chuy?n.", 
                    "Xác nh?n xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (_adminService.DeleteCarrier(carrierId))
                        {
                            LoadCarriers();
                            MessageBox.Show("?ã xóa nhà v?n chuy?n thành công!", "Thành công", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Không th? xóa nhà v?n chuy?n!", "L?i", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"L?i khi xóa nhà v?n chuy?n: {ex.Message}", "L?i", 
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
                        string statusText = newStatus ? "??t s?n sàng" : "??t không s?n sàng";
                        
                        var result = MessageBox.Show($"B?n có mu?n {statusText} giao hàng cho nhà v?n chuy?n này?", 
                            "Xác nh?n", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        
                        if (result == MessageBoxResult.Yes)
                        {
                            if (_adminService.UpdateCarrierStatus(carrierId, newStatus))
                            {
                                LoadCarriers();
                                MessageBox.Show($"?ã {statusText} giao hàng thành công!", "Thành công", 
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show("Không th? c?p nh?t tr?ng thái s?n sàng!", "L?i", 
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"L?i khi c?p nh?t tr?ng thái: {ex.Message}", "L?i", 
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
                        string statusText = newStatus ? "kích ho?t" : "vô hi?u hóa";
                        
                        var result = MessageBox.Show($"B?n có mu?n {statusText} tài kho?n nhà v?n chuy?n này?", 
                            "Xác nh?n", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        
                        if (result == MessageBoxResult.Yes)
                        {
                            if (_adminService.UpdateAccountStatus(carrier.Account.AccountId, newStatus))
                            {
                                LoadCarriers();
                                MessageBox.Show($"?ã {statusText} tài kho?n thành công!", "Thành công", 
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show("Không th? c?p nh?t tr?ng thái tài kho?n!", "L?i", 
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"L?i khi c?p nh?t tr?ng thái: {ex.Message}", "L?i", 
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