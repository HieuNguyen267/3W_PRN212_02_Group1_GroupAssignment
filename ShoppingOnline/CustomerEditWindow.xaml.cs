using BLL.Services;
using DAL.Entities;
using System;
using System.Windows;

namespace ShoppingOnline
{
    public partial class CustomerEditWindow : Window
    {
        private readonly IAdminService _adminService;
        private readonly Customer? _editingCustomer;
        private readonly bool _isEditMode;

        // Constructor for adding new customer
        public CustomerEditWindow()
        {
            InitializeComponent();
            _adminService = new AdminService();
            _isEditMode = false;
            HeaderTitle.Text = "Thêm khách hàng m?i";
        }

        // Constructor for editing existing customer
        public CustomerEditWindow(Customer customer)
        {
            InitializeComponent();
            _adminService = new AdminService();
            _editingCustomer = customer;
            _isEditMode = true;
            HeaderTitle.Text = "Ch?nh s?a thông tin khách hàng";
            
            LoadCustomerData();
        }

        private void LoadCustomerData()
        {
            if (_editingCustomer != null)
            {
                // Load account data
                if (_editingCustomer.Account != null)
                {
                    UsernameTextBox.Text = _editingCustomer.Account.Username;
                    EmailTextBox.Text = _editingCustomer.Account.Email;
                    IsActiveCheckBox.IsChecked = _editingCustomer.Account.IsActive;
                }

                // Load customer data
                FullNameTextBox.Text = _editingCustomer.FullName;
                PhoneTextBox.Text = _editingCustomer.Phone;
                AddressTextBox.Text = _editingCustomer.Address;

                // Show status section for edit mode
                StatusGrid.Visibility = Visibility.Visible;
                
                // In edit mode, password is optional
                PasswordBox.Password = ""; // Don't show existing password
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                if (_isEditMode)
                {
                    UpdateCustomer();
                }
                else
                {
                    AddNewCustomer();
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi l?u thông tin khách hàng:\n{ex.Message}", "L?i", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private bool ValidateInput()
        {
            // Validate username
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                MessageBox.Show("Vui lòng nh?p tên ??ng nh?p!", "L?i validation", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                UsernameTextBox.Focus();
                return false;
            }

            // Validate username length and format
            if (UsernameTextBox.Text.Trim().Length < 3)
            {
                MessageBox.Show("Tên ??ng nh?p ph?i có ít nh?t 3 ký t?!", "L?i validation", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                UsernameTextBox.Focus();
                return false;
            }

            // Validate email
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text) || !IsValidEmail(EmailTextBox.Text))
            {
                MessageBox.Show("Vui lòng nh?p email h?p l?!", "L?i validation", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                EmailTextBox.Focus();
                return false;
            }

            // Validate password (required for new customer, optional for edit)
            if (!_isEditMode && string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Vui lòng nh?p m?t kh?u!", "L?i validation", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                PasswordBox.Focus();
                return false;
            }

            // Validate password length for new accounts
            if (!_isEditMode && PasswordBox.Password.Length < 6)
            {
                MessageBox.Show("M?t kh?u ph?i có ít nh?t 6 ký t?!", "L?i validation", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                PasswordBox.Focus();
                return false;
            }

            // Validate full name
            if (string.IsNullOrWhiteSpace(FullNameTextBox.Text))
            {
                MessageBox.Show("Vui lòng nh?p h? và tên!", "L?i validation", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                FullNameTextBox.Focus();
                return false;
            }

            // Validate phone
            if (string.IsNullOrWhiteSpace(PhoneTextBox.Text))
            {
                MessageBox.Show("Vui lòng nh?p s? ?i?n tho?i!", "L?i validation", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                PhoneTextBox.Focus();
                return false;
            }

            // Validate phone format (basic)
            var phone = PhoneTextBox.Text.Trim();
            if (phone.Length < 10 || !System.Text.RegularExpressions.Regex.IsMatch(phone, @"^[0-9+\-\s()]+$"))
            {
                MessageBox.Show("S? ?i?n tho?i không h?p l?!", "L?i validation", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                PhoneTextBox.Focus();
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

        private void AddNewCustomer()
        {
            try
            {
                // Check for duplicates first
                if (_adminService.IsUsernameOrEmailExists(UsernameTextBox.Text.Trim(), EmailTextBox.Text.Trim()))
                {
                    MessageBox.Show("Không th? thêm khách hàng!\n\n" +
                                   "Email ho?c tên ??ng nh?p ?ã ???c s? d?ng.\n\n" +
                                   "Vui lòng s? d?ng email và tên ??ng nh?p khác.", 
                        "Trùng l?p thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Create new account
                var account = new Account
                {
                    Username = UsernameTextBox.Text.Trim(),
                    Email = EmailTextBox.Text.Trim(),
                    Password = PasswordBox.Password,
                    AccountType = "Customer",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };

                // Create new customer
                var customer = new Customer
                {
                    FullName = FullNameTextBox.Text.Trim(),
                    Phone = PhoneTextBox.Text.Trim(),
                    Address = AddressTextBox.Text.Trim(),
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                // Add customer using admin service
                bool success = _adminService.AddCustomer(customer, account);
                
                if (!success)
                {
                    MessageBox.Show("Không th? thêm khách hàng!\n\n" +
                                   "Có th? do:\n" +
                                   "• L?i c? s? d? li?u\n" +
                                   "• Ràng bu?c d? li?u\n" +
                                   "• Email ho?c tên ??ng nh?p ?ã t?n t?i", 
                        "L?i thêm khách hàng", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Success - this line should only be reached if the operation was successful
                MessageBox.Show($"Thêm khách hàng thành công!\n\n" +
                               $"• Tên: {customer.FullName}\n" +
                               $"• Email: {account.Email}\n" +
                               $"• Tên ??ng nh?p: {account.Username}", 
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi thêm khách hàng:\n{ex.Message}\n\n" +
                               "Vui lòng th? l?i ho?c liên h? qu?n tr? viên.", 
                    "L?i h? th?ng", MessageBoxButton.OK, MessageBoxImage.Error);
                throw; // Re-throw to be caught by Save_Click
            }
        }

        private void UpdateCustomer()
        {
            if (_editingCustomer == null)
                return;

            try
            {
                // Check for duplicates (excluding current customer's account)
                int? excludeAccountId = _editingCustomer.Account?.AccountId;
                if (_adminService.IsUsernameOrEmailExists(UsernameTextBox.Text.Trim(), EmailTextBox.Text.Trim(), excludeAccountId))
                {
                    MessageBox.Show("Không th? c?p nh?t thông tin!\n\n" +
                                   "Email ho?c tên ??ng nh?p ?ã ???c s? d?ng b?i tài kho?n khác.\n\n" +
                                   "Vui lòng s? d?ng email và tên ??ng nh?p khác.", 
                        "Trùng l?p thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Update customer info
                _editingCustomer.FullName = FullNameTextBox.Text.Trim();
                _editingCustomer.Phone = PhoneTextBox.Text.Trim();
                _editingCustomer.Address = AddressTextBox.Text.Trim();
                _editingCustomer.UpdatedDate = DateTime.Now;

                // Update account info if exists
                if (_editingCustomer.Account == null)
                    _editingCustomer.Account = new Account();

                _editingCustomer.Account.Username = UsernameTextBox.Text.Trim();
                _editingCustomer.Account.Email = EmailTextBox.Text.Trim();
                _editingCustomer.Account.IsActive = IsActiveCheckBox.IsChecked ?? true;

                // Only update password if provided
                if (!string.IsNullOrWhiteSpace(PasswordBox.Password))
                {
                    _editingCustomer.Account.Password = PasswordBox.Password;
                }

                bool success = _adminService.UpdateCustomer(_editingCustomer);
                
                if (!success)
                {
                    MessageBox.Show("Không th? c?p nh?t thông tin khách hàng!\n\n" +
                                   "Có th? do:\n" +
                                   "• Email ho?c tên ??ng nh?p ?ã t?n t?i\n" +
                                   "• L?i c? s? d? li?u\n" +
                                   "• Ràng bu?c d? li?u", 
                        "L?i c?p nh?t", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Success
                MessageBox.Show($"C?p nh?t thông tin khách hàng thành công!\n\n" +
                               $"• Tên: {_editingCustomer.FullName}\n" +
                               $"• Email: {_editingCustomer.Account.Email}\n" +
                               $"• Tr?ng thái: {(_editingCustomer.Account.IsActive == true ? "Ho?t ??ng" : "Khóa")}", 
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi c?p nh?t thông tin khách hàng:\n{ex.Message}\n\n" +
                               "Vui lòng th? l?i ho?c liên h? qu?n tr? viên.", 
                    "L?i h? th?ng", MessageBoxButton.OK, MessageBoxImage.Error);
                throw; // Re-throw to be caught by Save_Click
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}