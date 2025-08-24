using BLL.Services;
using DAL.Entities;
using System;
using System.Windows;

namespace ShoppingOnline
{
    public partial class AccountSettingsWindow : Window
    {
        private readonly AccountService _accountService;
        private Customer _currentCustomer;

        public AccountSettingsWindow()
        {
            InitializeComponent();
            _accountService = new AccountService();
            
            LoadCustomerData();
        }

        private void LoadCustomerData()
        {
            if (UserSession.IsLoggedIn && UserSession.CustomerId.HasValue)
            {
                try
                {
                    _currentCustomer = _accountService.GetCustomerById(UserSession.CustomerId.Value);
                    if (_currentCustomer != null)
                    {
                        FullNameTextBox.Text = _currentCustomer.FullName;
                        PhoneTextBox.Text = _currentCustomer.Phone ?? "";
                        AddressTextBox.Text = _currentCustomer.Address ?? "";
                        
                        if (_currentCustomer.Account != null)
                        {
                            EmailTextBox.Text = _currentCustomer.Account.Email;
                        }
                        
                        // Set default notification settings
                        EmailNotificationCheckBox.IsChecked = true;
                        SMSNotificationCheckBox.IsChecked = true;
                        PromotionNotificationCheckBox.IsChecked = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi tải thông tin tài khoản: {ex.Message}", "Lỗi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(FullNameTextBox.Text))
                {
                    MessageBox.Show("Vui lòng nhập họ và tên!", "Thông báo", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    FullNameTextBox.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(PhoneTextBox.Text))
                {
                    MessageBox.Show("Vui lòng nhập số điện thoại!", "Thông báo", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    PhoneTextBox.Focus();
                    return;
                }

                // Check if password is being changed
                if (!string.IsNullOrEmpty(NewPasswordBox.Password))
                {
                    if (string.IsNullOrEmpty(CurrentPasswordBox.Password))
                    {
                        MessageBox.Show("Vui lòng nhập mật khẩu hiện tại!", "Thông báo", 
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        CurrentPasswordBox.Focus();
                        return;
                    }

                    if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
                    {
                        MessageBox.Show("Mật khẩu xác nhận không khớp!", "Thông báo", 
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        ConfirmPasswordBox.Focus();
                        return;
                    }

                    if (NewPasswordBox.Password.Length < 6)
                    {
                        MessageBox.Show("Mật khẩu phải có ít nhất 6 ký tự!", "Thông báo", 
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        NewPasswordBox.Focus();
                        return;
                    }

                    // Validate current password
                    if (_currentCustomer?.Account?.Password != CurrentPasswordBox.Password)
                    {
                        MessageBox.Show("Mật khẩu hiện tại không đúng!", "Thông báo", 
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        CurrentPasswordBox.Focus();
                        return;
                    }
                }

                // Update customer information
                if (_currentCustomer != null)
                {
                    _currentCustomer.FullName = FullNameTextBox.Text.Trim();
                    _currentCustomer.Phone = PhoneTextBox.Text.Trim();
                    _currentCustomer.Address = AddressTextBox.Text.Trim();
                    _currentCustomer.UpdatedDate = DateTime.Now;

                    // Update password if provided
                    if (!string.IsNullOrEmpty(NewPasswordBox.Password) && _currentCustomer.Account != null)
                    {
                        _currentCustomer.Account.Password = NewPasswordBox.Password;
                    }

                    // Save changes
                    _accountService.UpdateCustomer(_currentCustomer);

                    MessageBox.Show("Cập nhật thông tin thành công!", "Thành công", 
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Update UserSession
                    UserSession.CustomerName = _currentCustomer.FullName;

                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật thông tin: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
