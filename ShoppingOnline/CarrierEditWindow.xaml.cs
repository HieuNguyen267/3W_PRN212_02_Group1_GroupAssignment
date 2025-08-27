using BLL.Services;
using DAL.Entities;
using System;
using System.Windows;

namespace ShoppingOnline
{
    public partial class CarrierEditWindow : Window
    {
        private readonly CarrierService _carrierService;
        private readonly Carrier? _editingCarrier;
        private readonly bool _isEditMode;

        // Constructor for adding new carrier
        public CarrierEditWindow()
        {
            InitializeComponent();
            _carrierService = new CarrierService();
            _isEditMode = false;
            HeaderTitle.Text = "Thêm nhà vận chuyển mới";
        }

        // Constructor for editing existing carrier
        public CarrierEditWindow(Carrier carrier)
        {
            InitializeComponent();
            _carrierService = new CarrierService();
            _editingCarrier = carrier;
            _isEditMode = true;
            HeaderTitle.Text = "Chỉnh sửa thông tin nhà vận chuyển";
            
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
                    if (!AddNewCarrier())
                    {
                        // Keep dialog open when add fails
                        return;
                    }
                }

                DialogResult = true;
                Close();
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
                MessageBox.Show("Vui lòng nhập tên đăng nhập!", "Lỗi xác thực", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                UsernameTextBox.Focus();
                return false;
            }

            // Validate email
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text) || !IsValidEmail(EmailTextBox.Text))
            {
                MessageBox.Show("Vui lòng nhập email hợp lệ!", "Lỗi xác thực", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                EmailTextBox.Focus();
                return false;
            }

            // Validate password (required for new carrier, optional for edit)
            if (!_isEditMode && string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu!", "Lỗi xác thực", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                PasswordBox.Focus();
                return false;
            }

            // Validate full name
            if (string.IsNullOrWhiteSpace(FullNameTextBox.Text))
            {
                MessageBox.Show("Vui lòng nhập họ và tên!", "Lỗi xác thực", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                FullNameTextBox.Focus();
                return false;
            }

            // Validate phone
            if (string.IsNullOrWhiteSpace(PhoneTextBox.Text))
            {
                MessageBox.Show("Vui lòng nhập số điện thoại!", "Lỗi xác thực", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                PhoneTextBox.Focus();
                return false;
            }

            // Validate vehicle number
            if (string.IsNullOrWhiteSpace(VehicleNumberTextBox.Text))
            {
                MessageBox.Show("Vui lòng nhập số xe!", "Lỗi xác thực", 
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

        private bool AddNewCarrier()
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

            // Pre-check duplicate username/email
            if (_carrierService.UsernameOrEmailExists(account.Username!, account.Email!))
            {
                MessageBox.Show("Email hoặc tên đăng nhập đã tồn tại. Vui lòng chọn thông tin khác.", 
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!_carrierService.AddCarrier(carrier, account))
            {
                MessageBox.Show("Không thể thêm nhà vận chuyển. Vui lòng thử lại.", 
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
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

            if (!_carrierService.UpdateCarrier(_editingCarrier))
            {
                MessageBox.Show("Không thể cập nhật thông tin nhà vận chuyển. Có thể email hoặc tên đăng nhập đã tồn tại!", 
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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