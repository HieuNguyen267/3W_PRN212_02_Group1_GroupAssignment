using BLL.Services;
using DAL.Entities;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ShoppingOnline
{
    public partial class AddressManagementWindow : Window, INotifyPropertyChanged
    {
        private readonly AccountService _accountService;
        private Customer _currentCustomer;
        private AddressViewModel _currentEditingAddress;
        private bool _isEditing = false;

        public AddressManagementWindow()
        {
            InitializeComponent();
            _accountService = new AccountService();
            Addresses = new ObservableCollection<AddressViewModel>();
            
            LoadAddresses();
            AddressListItemsControl.ItemsSource = Addresses;
            DataContext = this;
        }

        public ObservableCollection<AddressViewModel> Addresses { get; set; }



        private void LoadAddresses()
        {
            if (UserSession.IsLoggedIn && UserSession.CustomerId.HasValue)
            {
                Addresses.Clear();
                
                try
                {
                    _currentCustomer = _accountService.GetCustomerById(UserSession.CustomerId.Value);
                    if (_currentCustomer != null && !string.IsNullOrEmpty(_currentCustomer.Address))
                    {
                        // Parse the address from Customer.Address field
                        var address = ParseAddressFromCustomer(_currentCustomer);
                        Addresses.Add(address);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi tải địa chỉ: {ex.Message}", "Lỗi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                
                // Update visibility of no addresses text
                if (Addresses.Count == 0)
                {
                    NoAddressesText.Visibility = Visibility.Visible;
                }
                else
                {
                    NoAddressesText.Visibility = Visibility.Collapsed;
                }
            }
        }

        private AddressViewModel ParseAddressFromCustomer(Customer customer)
        {
            // Parse the address string from Customer.Address
            // Format: "123 Nguyễn Trãi, Hà Nội" or similar
            var addressParts = customer.Address?.Split(',') ?? new string[0];
            
            return new AddressViewModel
            {
                Id = 1, // Default ID since we only have one address
                FullName = customer.FullName ?? "",
                Phone = customer.Phone ?? "",
                Province = addressParts.Length > 1 ? addressParts[1].Trim() : "",
                District = "", // Not available in current schema
                Ward = "", // Not available in current schema
                DetailAddress = addressParts.Length > 0 ? addressParts[0].Trim() : customer.Address ?? "",
                IsDefault = true // This is the only address
            };
        }

        private void AddNewAddress_Click(object sender, RoutedEventArgs e)
        {
            _isEditing = false;
            _currentEditingAddress = null;
            ClearForm();
            FormTitleText.Text = "Thêm địa chỉ mới";
            SaveButton.Content = "Lưu địa chỉ";
        }

        private void EditAddress_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is AddressViewModel address)
            {
                _isEditing = true;
                _currentEditingAddress = address;
                LoadAddressToForm(address);
                FormTitleText.Text = "Sửa địa chỉ";
                SaveButton.Content = "Cập nhật địa chỉ";
            }
        }

        private void DeleteAddress_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is AddressViewModel address)
            {
                var result = MessageBox.Show($"Bạn có chắc muốn xóa địa chỉ này?", "Xác nhận xóa", 
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Since we only have one address in Customer.Address, we'll clear it
                        if (_currentCustomer != null)
                        {
                            _currentCustomer.Address = "";
                            _accountService.UpdateCustomer(_currentCustomer);
                            LoadAddresses();
                            MessageBox.Show("Đã xóa địa chỉ thành công!", "Thông báo", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi xóa địa chỉ: {ex.Message}", "Lỗi", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void SetDefaultAddress_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is AddressViewModel address)
            {
                try
                {
                    // Set this address as default (update Customer.Address)
                    if (_currentCustomer != null)
                    {
                        _currentCustomer.Address = address.FullAddress;
                        _accountService.UpdateCustomer(_currentCustomer);
                        LoadAddresses();
                        MessageBox.Show("Đã đặt làm địa chỉ mặc định!", "Thông báo", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi đặt địa chỉ mặc định: {ex.Message}", "Lỗi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveAddress_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateForm())
            {
                try
                {
                    var address = new AddressViewModel
                    {
                        Id = _isEditing ? _currentEditingAddress?.Id ?? 1 : 1,
                        FullName = FullNameTextBox.Text.Trim(),
                        Phone = PhoneTextBox.Text.Trim(),
                        Province = (ProvinceComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "",
                        District = (DistrictComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "",
                        Ward = (WardComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "",
                        DetailAddress = DetailAddressTextBox.Text.Trim(),
                        IsDefault = DefaultAddressCheckBox.IsChecked ?? false
                    };

                    // Update Customer.Address with the new address
                    if (_currentCustomer != null)
                    {
                        _currentCustomer.Address = address.FullAddress;
                        _accountService.UpdateCustomer(_currentCustomer);
                        
                        LoadAddresses();
                        ClearForm();
                        
                        var message = _isEditing ? "Cập nhật địa chỉ thành công!" : "Thêm địa chỉ thành công!";
                        MessageBox.Show(message, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi lưu địa chỉ: {ex.Message}", "Lỗi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CancelForm_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            FormTitleText.Text = "Thêm địa chỉ mới";
            SaveButton.Content = "Lưu địa chỉ";
            _isEditing = false;
            _currentEditingAddress = null;
        }

        private void LoadAddressToForm(AddressViewModel address)
        {
            FullNameTextBox.Text = address.FullName;
            PhoneTextBox.Text = address.Phone;
            DetailAddressTextBox.Text = address.DetailAddress;
            
            // Set province
            foreach (ComboBoxItem item in ProvinceComboBox.Items)
            {
                if (item.Content.ToString() == address.Province)
                {
                    ProvinceComboBox.SelectedItem = item;
                    break;
                }
            }
            
            // Set district
            foreach (ComboBoxItem item in DistrictComboBox.Items)
            {
                if (item.Content.ToString() == address.District)
                {
                    DistrictComboBox.SelectedItem = item;
                    break;
                }
            }
            
            // Set ward
            foreach (ComboBoxItem item in WardComboBox.Items)
            {
                if (item.Content.ToString() == address.Ward)
                {
                    WardComboBox.SelectedItem = item;
                    break;
                }
            }
            
            DefaultAddressCheckBox.IsChecked = address.IsDefault;
        }

        private void ClearForm()
        {
            FullNameTextBox.Text = "";
            PhoneTextBox.Text = "";
            DetailAddressTextBox.Text = "";
            ProvinceComboBox.SelectedIndex = -1;
            DistrictComboBox.SelectedIndex = -1;
            WardComboBox.SelectedIndex = -1;
            DefaultAddressCheckBox.IsChecked = false;
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(FullNameTextBox.Text))
            {
                MessageBox.Show("Vui lòng nhập họ và tên!", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                FullNameTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(PhoneTextBox.Text))
            {
                MessageBox.Show("Vui lòng nhập số điện thoại!", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                PhoneTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(DetailAddressTextBox.Text))
            {
                MessageBox.Show("Vui lòng nhập địa chỉ chi tiết!", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                DetailAddressTextBox.Focus();
                return false;
            }

            return true;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
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

    public class AddressViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Province { get; set; } = "";
        public string District { get; set; } = "";
        public string Ward { get; set; } = "";
        public string DetailAddress { get; set; } = "";
        public bool IsDefault { get; set; }

        public string FullAddress
        {
            get
            {
                var parts = new[] { DetailAddress, Ward, District, Province }
                    .Where(p => !string.IsNullOrEmpty(p))
                    .ToArray();
                return string.Join(", ", parts);
            }
        }
    }

    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
