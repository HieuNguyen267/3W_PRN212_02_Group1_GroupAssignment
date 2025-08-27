using BLL.Services;
using System;
using System.Windows;

namespace ShoppingOnline
{
    public partial class RegisterWindow : Window
    {
        private readonly AccountService _accountService;
        public RegisterWindow()
        {
            InitializeComponent();
            _accountService = new AccountService();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            var fullName = FullNameTextBox.Text.Trim();
            var username = UsernameTextBox.Text.Trim();
            var email = EmailTextBox.Text.Trim();
            var password = PasswordBox.Password.Trim();
            var confirmPassword = ConfirmPasswordBox.Password.Trim();
            var phone = PhoneTextBox.Text.Trim();
            var address = AddressTextBox.Text.Trim();

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email)
                || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var result = _accountService.Register(fullName, username, email, password, phone, address);
                if (result.IsSuccess)
                {
                    MessageBox.Show(result.Message, "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show(result.Message, "Lỗi đăng ký", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi hệ thống: {ex.Message}", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
