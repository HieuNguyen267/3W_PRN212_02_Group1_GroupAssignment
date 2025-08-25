using BLL.Services;
using DAL.Entities;
using System;
using System.Windows;

namespace ShoppingOnline
{
    public partial class CarrierEditWindow : Window
    {
        private readonly IAdminService _adminService;
        private readonly Carrier? _editingCarrier;
        private readonly bool _isEditMode;

        // Constructor for adding new carrier
        public CarrierEditWindow()
        {
            InitializeComponent();
            _adminService = new AdminService();
            _isEditMode = false;
            HeaderTitle.Text = "Thêm nhà v?n chuy?n m?i";
        }

        // Constructor for editing existing carrier
        public CarrierEditWindow(Carrier carrier)
        {
            InitializeComponent();
            _adminService = new AdminService();
            _editingCarrier = carrier;
            _isEditMode = true;
            HeaderTitle.Text = "Ch?nh s?a thông tin nhà v?n chuy?n";
            
            LoadCarrierData();
        }

        private void LoadCarrierData()
        {
            if (_editingCarrier != null)
            {
                // Load account data
                if (_editingCarrier.Account != null)
                {
                    UsernameTextBox.Text = _editingCarrier.Account.Username;
                    EmailTextBox.Text = _editingCarrier.Account.Email;
                    IsActiveCheckBox.IsChecked = _editingCarrier.Account.IsActive;
                }

                // Load carrier data
                FullNameTextBox.Text = _editingCarrier.FullName;
                PhoneTextBox.Text = _editingCarrier.Phone;
                VehicleNumberTextBox.Text = _editingCarrier.VehicleNumber;
                IsAvailableCheckBox.IsChecked = _editingCarrier.IsAvailable;

                // Show status section for edit mode
                StatusGrid.Visibility = Visibility.Visible;
                
                // In edit mode, password is optional
                PasswordBox.Password = ""; // Don't show existing password
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate input
                if (!ValidateInput())
                    return;

                if (_isEditMode)
                {
                    // Update existing carrier
                    UpdateCarrier();
                }
                else
                {
                    // Add new carrier
                    AddNewCarrier();
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi l?u thông tin: {ex.Message}", "L?i", 
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

            // Validate email
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text) || !IsValidEmail(EmailTextBox.Text))
            {
                MessageBox.Show("Vui lòng nh?p email h?p l?!", "L?i validation", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                EmailTextBox.Focus();
                return false;
            }

            // Validate password (required for new carrier, optional for edit)
            if (!_isEditMode && string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Vui lòng nh?p m?t kh?u!", "L?i validation", 
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

            // Validate vehicle number
            if (string.IsNullOrWhiteSpace(VehicleNumberTextBox.Text))
            {
                MessageBox.Show("Vui lòng nh?p s? xe!", "L?i validation", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                VehicleNumberTextBox.Focus();
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

        private void AddNewCarrier()
        {
            // Create new account
            var account = new Account
            {
                Username = UsernameTextBox.Text.Trim(),
                Email = EmailTextBox.Text.Trim(),
                Password = PasswordBox.Password,
                AccountType = "Carrier"
            };

            // Create new carrier
            var carrier = new Carrier
            {
                FullName = FullNameTextBox.Text.Trim(),
                Phone = PhoneTextBox.Text.Trim(),
                VehicleNumber = VehicleNumberTextBox.Text.Trim()
            };

            if (!_adminService.AddCarrier(carrier, account))
            {
                MessageBox.Show("Không th? thêm nhà v?n chuy?n. Có th? email ho?c tên ??ng nh?p ?ã t?n t?i!", 
                    "L?i", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void UpdateCarrier()
        {
            if (_editingCarrier == null)
                return;

            // Update carrier info
            _editingCarrier.FullName = FullNameTextBox.Text.Trim();
            _editingCarrier.Phone = PhoneTextBox.Text.Trim();
            _editingCarrier.VehicleNumber = VehicleNumberTextBox.Text.Trim();
            _editingCarrier.IsAvailable = IsAvailableCheckBox.IsChecked;

            // Update account info if exists
            if (_editingCarrier.Account == null)
                _editingCarrier.Account = new Account();

            _editingCarrier.Account.Username = UsernameTextBox.Text.Trim();
            _editingCarrier.Account.Email = EmailTextBox.Text.Trim();
            _editingCarrier.Account.IsActive = IsActiveCheckBox.IsChecked;

            // Only update password if provided
            if (!string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                _editingCarrier.Account.Password = PasswordBox.Password;
            }

            if (!_adminService.UpdateCarrier(_editingCarrier))
            {
                MessageBox.Show("Không th? c?p nh?t thông tin nhà v?n chuy?n. Có th? email ho?c tên ??ng nh?p ?ã t?n t?i!", 
                    "L?i", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}