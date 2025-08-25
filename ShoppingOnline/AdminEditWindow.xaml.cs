using BLL.Services;
using DAL.Entities;
using System;
using System.Windows;

namespace ShoppingOnline
{
    public partial class AdminEditWindow : Window
    {
        private readonly IAdminService _adminService;
        private readonly Admin? _editingAdmin;
        private readonly bool _isEditMode;

        // Constructor for adding new admin
        public AdminEditWindow()
        {
            InitializeComponent();
            _adminService = new AdminService();
            _isEditMode = false;
            HeaderTitle.Text = "Thêm admin m?i";
        }

        // Constructor for editing existing admin
        public AdminEditWindow(Admin admin)
        {
            InitializeComponent();
            _adminService = new AdminService();
            _editingAdmin = admin;
            _isEditMode = true;
            HeaderTitle.Text = "Ch?nh s?a thông tin admin";
            
            LoadAdminData();
        }

        private void LoadAdminData()
        {
            if (_editingAdmin != null)
            {
                // Load account data
                if (_editingAdmin.Account != null)
                {
                    UsernameTextBox.Text = _editingAdmin.Account.Username;
                    EmailTextBox.Text = _editingAdmin.Account.Email;
                    IsActiveCheckBox.IsChecked = _editingAdmin.Account.IsActive;
                }

                // Load admin data
                FullNameTextBox.Text = _editingAdmin.FullName;
                PhoneTextBox.Text = _editingAdmin.Phone;

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
                    UpdateAdmin();
                }
                else
                {
                    AddNewAdmin();
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi l?u thông tin admin:\n{ex.Message}", "L?i", 
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

            // Validate password (required for new admin, optional for edit)
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

        private void AddNewAdmin()
        {
            try
            {
                // Check for duplicates first
                if (_adminService.IsUsernameOrEmailExists(UsernameTextBox.Text.Trim(), EmailTextBox.Text.Trim()))
                {
                    MessageBox.Show("Không th? thêm admin!\n\n" +
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
                    AccountType = "Admin",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };

                // Create new admin
                var admin = new Admin
                {
                    FullName = FullNameTextBox.Text.Trim(),
                    Phone = PhoneTextBox.Text.Trim(),
                    CreatedDate = DateTime.Now
                };

                // Add admin using admin service
                bool success = _adminService.AddAdmin(admin, account);
                
                if (!success)
                {
                    MessageBox.Show("Không th? thêm admin!\n\n" +
                                   "Có th? do:\n" +
                                   "• L?i c? s? d? li?u\n" +
                                   "• Ràng bu?c d? li?u\n" +
                                   "• Email ho?c tên ??ng nh?p ?ã t?n t?i", 
                        "L?i thêm admin", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Success - this line should only be reached if the operation was successful
                MessageBox.Show($"Thêm admin thành công!\n\n" +
                               $"• Tên: {admin.FullName}\n" +
                               $"• Email: {account.Email}\n" +
                               $"• Tên ??ng nh?p: {account.Username}", 
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi thêm admin:\n{ex.Message}\n\n" +
                               "Vui lòng th? l?i ho?c liên h? qu?n tr? viên.", 
                    "L?i h? th?ng", MessageBoxButton.OK, MessageBoxImage.Error);
                throw; // Re-throw to be caught by Save_Click
            }
        }

        private void UpdateAdmin()
        {
            if (_editingAdmin == null)
                return;

            try
            {
                // Check for duplicates (excluding current admin's account)
                int? excludeAccountId = _editingAdmin.Account?.AccountId;
                if (_adminService.IsUsernameOrEmailExists(UsernameTextBox.Text.Trim(), EmailTextBox.Text.Trim(), excludeAccountId))
                {
                    MessageBox.Show("Không th? c?p nh?t thông tin!\n\n" +
                                   "Email ho?c tên ??ng nh?p ?ã ???c s? d?ng b?i tài kho?n khác.\n\n" +
                                   "Vui lòng s? d?ng email và tên ??ng nh?p khác.", 
                        "Trùng l?p thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Update admin info
                _editingAdmin.FullName = FullNameTextBox.Text.Trim();
                _editingAdmin.Phone = PhoneTextBox.Text.Trim();

                // Update account info if exists
                if (_editingAdmin.Account == null)
                    _editingAdmin.Account = new Account();

                _editingAdmin.Account.Username = UsernameTextBox.Text.Trim();
                _editingAdmin.Account.Email = EmailTextBox.Text.Trim();
                _editingAdmin.Account.IsActive = IsActiveCheckBox.IsChecked ?? true;

                // Only update password if provided
                if (!string.IsNullOrWhiteSpace(PasswordBox.Password))
                {
                    _editingAdmin.Account.Password = PasswordBox.Password;
                }

                bool success = _adminService.UpdateAdmin(_editingAdmin);
                
                if (!success)
                {
                    MessageBox.Show("Không th? c?p nh?t thông tin admin!\n\n" +
                                   "Có th? do:\n" +
                                   "• Email ho?c tên ??ng nh?p ?ã t?n t?i\n" +
                                   "• L?i c? s? d? li?u\n" +
                                   "• Ràng bu?c d? li?u", 
                        "L?i c?p nh?t", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Success
                MessageBox.Show($"C?p nh?t thông tin admin thành công!\n\n" +
                               $"• Tên: {_editingAdmin.FullName}\n" +
                               $"• Email: {_editingAdmin.Account.Email}\n" +
                               $"• Tr?ng thái: {(_editingAdmin.Account.IsActive == true ? "Ho?t ??ng" : "Khóa")}", 
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi c?p nh?t thông tin admin:\n{ex.Message}\n\n" +
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