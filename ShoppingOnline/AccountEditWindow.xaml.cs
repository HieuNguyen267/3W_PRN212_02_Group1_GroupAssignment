using BLL.Services;
using DAL.Entities;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ShoppingOnline
{
    public partial class AccountEditWindow : Window
    {
        private readonly IAdminService _adminService;
        private readonly Account _editingAccount;

        public AccountEditWindow(Account account)
        {
            InitializeComponent();
            _adminService = new AdminService();
            _editingAccount = account;
            
            LoadAccountData();
        }

        private void LoadAccountData()
        {
            if (_editingAccount != null)
            {
                UsernameTextBox.Text = _editingAccount.Username;
                EmailTextBox.Text = _editingAccount.Email;
                IsActiveCheckBox.IsChecked = _editingAccount.IsActive;
                
                // Set account type
                foreach (ComboBoxItem item in AccountTypeComboBox.Items)
                {
                    if (item.Content.ToString() == _editingAccount.AccountType)
                    {
                        AccountTypeComboBox.SelectedItem = item;
                        break;
                    }
                }
                
                // Password is left empty (optional update)
                PasswordBox.Password = "";
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate input
                if (!ValidateInput())
                    return;

                // Update account info
                _editingAccount.Username = UsernameTextBox.Text.Trim();
                _editingAccount.Email = EmailTextBox.Text.Trim();
                _editingAccount.AccountType = (AccountTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? _editingAccount.AccountType;
                _editingAccount.IsActive = IsActiveCheckBox.IsChecked;

                // Only update password if provided
                if (!string.IsNullOrWhiteSpace(PasswordBox.Password))
                {
                    _editingAccount.Password = PasswordBox.Password;
                }

                if (_adminService.UpdateAccount(_editingAccount))
                {
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Không thể cập nhật thông tin tài khoảng. Có thể email hoặc tên đăng nhập bị lỗi!", 
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu thông tin: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            // Validate username
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                MessageBox.Show("Vui lòng nhấp tên đăng nhập!", "Lỗi xác thực", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                UsernameTextBox.Focus();
                return false;
            }

            // Validate email
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text) || !IsValidEmail(EmailTextBox.Text))
            {
                MessageBox.Show("Vui lòng nh?p email h?p l?!", "Lỗi xác thực", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                EmailTextBox.Focus();
                return false;
            }

            // Validate account type
            if (AccountTypeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn loại tài khoảng!", "Lỗi xác thực", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                AccountTypeComboBox.Focus();
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}