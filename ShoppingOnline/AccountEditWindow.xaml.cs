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
                    MessageBox.Show("Kh�ng th? c?p nh?t th�ng tin t�i kho?n. C� th? email ho?c t�n ??ng nh?p ?� t?n t?i!", 
                        "L?i", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi l?u th�ng tin: {ex.Message}", "L?i", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            // Validate username
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                MessageBox.Show("Vui l�ng nh?p t�n ??ng nh?p!", "L?i validation", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                UsernameTextBox.Focus();
                return false;
            }

            // Validate email
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text) || !IsValidEmail(EmailTextBox.Text))
            {
                MessageBox.Show("Vui l�ng nh?p email h?p l?!", "L?i validation", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                EmailTextBox.Focus();
                return false;
            }

            // Validate account type
            if (AccountTypeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Vui l�ng ch?n lo?i t�i kho?n!", "L?i validation", 
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