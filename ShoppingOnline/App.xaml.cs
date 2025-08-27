using System.Configuration;
using System.Data;
using System.Windows;
using BLL.Services;
using ShoppingOnline.Constants;

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
            // Admin account creation removed - no default data
        }
    }
}
