using System.Configuration;
using System.Data;
using System.Windows;
using DAL.Entities;

namespace ShoppingOnline
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Create default admin account if not exists
            CreateDefaultAdminAccount();
        }

        private void CreateDefaultAdminAccount()
        {
            try
            {
                using var context = new ShoppingOnlineContext();
                
                // Check if any admin account exists
                var adminExists = context.Accounts.Any(a => a.AccountType == "Admin");
                
                if (!adminExists)
                {
                    // Create default admin account
                    var adminAccount = new Account
                    {
                        Email = "admin@shop.com",
                        Password = "admin123",
                        AccountType = "Admin",
                        IsActive = true,
                        CreatedDate = DateTime.Now
                    };
                    
                    context.Accounts.Add(adminAccount);
                    context.SaveChanges();
                    
                    // Create admin profile
                    var admin = new Admin
                    {
                        AccountId = adminAccount.AccountId,
                        FullName = "Administrator",
                        Phone = "0901234567",
                        CreatedDate = DateTime.Now
                    };
                    
                    context.Admins.Add(admin);
                    context.SaveChanges();
                    
                    MessageBox.Show("Tài khoản admin mặc định đã được tạo!\nEmail: admin@shop.com\nPassword: admin123", 
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo tài khoản admin: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
