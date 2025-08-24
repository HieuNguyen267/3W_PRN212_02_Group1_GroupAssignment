using BLL.Services;
using System;
using System.Windows;

namespace ShoppingOnline
{
    public partial class LoginWindow : Window
    {
        private readonly AccountService _accountService;
        public bool IsLoggedIn { get; private set; } = false;
        public int? LoggedInCustomerId { get; private set; } = null;
        public string? LoggedInCustomerName { get; private set; } = null;
        public string? LoggedInEmail { get; private set; } = null;
        public string? LoggedInPhone { get; private set; } = null;

        public LoginWindow()
        {
            InitializeComponent();
            _accountService = new AccountService();
            // Remove mock password - use database authentication only
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            var emailOrPhone = UsernameTextBox.Text.Trim();
            var password = PasswordBox.Password.Trim();

            if (string.IsNullOrEmpty(emailOrPhone) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var result = _accountService.Login(emailOrPhone, password);
                
                if (result.IsSuccess)
                {
                    MessageBox.Show(result.Message, "Thành công", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    this.DialogResult = true;
                    
                    // Check if admin or customer
                    if (result.IsAdmin && result.Admin != null)
                    {
                        // Admin login - set up AdminSession and open AdminDashboard
                        AdminSession.Login(result.Admin);
                        var adminDashboard = new AdminDashboardWindow();
                        adminDashboard.Show();
                    }
                    else if (result.Customer != null)
                    {
                        // Customer login - set up session properties
                        IsLoggedIn = true;
                        LoggedInCustomerId = result.Customer.CustomerId;
                        LoggedInCustomerName = result.Customer.FullName;
                        LoggedInEmail = result.Customer.Account?.Email;
                        LoggedInPhone = result.Customer.Phone;
                    }
                    
                    this.Close();
                }
                else
                {
                    MessageBox.Show(result.Message, "Lỗi đăng nhập", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối database: {ex.Message}", "Lỗi hệ thống", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tính năng quên mật khẩu sẽ được phát triển sau!", "Thông báo", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tính năng đăng ký sẽ được phát triển sau!", "Thông báo", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
