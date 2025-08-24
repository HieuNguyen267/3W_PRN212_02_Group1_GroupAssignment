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

        public LoginWindow()
        {
            InitializeComponent();
            _accountService = new AccountService();
            // Set default password for demo
            PasswordBox.Password = "123456";
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
                
                if (result.IsSuccess && result.Customer != null)
                {
                    IsLoggedIn = true;
                    LoggedInCustomerId = result.Customer.CustomerId;
                    LoggedInCustomerName = result.Customer.FullName;
                    
                    MessageBox.Show(result.Message, "Thành công", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    this.DialogResult = true;
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
