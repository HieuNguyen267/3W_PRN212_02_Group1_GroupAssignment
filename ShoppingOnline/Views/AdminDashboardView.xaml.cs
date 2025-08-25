using BLL.Services;
using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace ShoppingOnline.Views
{
    public partial class AdminDashboardView : UserControl, INotifyPropertyChanged
    {
        private readonly IAdminService _adminService;

        public AdminDashboardView()
        {
            InitializeComponent();
            _adminService = new AdminService();
            DataContext = this;
            
            Loaded += AdminDashboardView_Loaded;
        }

        private void AdminDashboardView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            LoadDashboardData();
        }

        public void LoadDashboardData()
        {
            try
            {
                // Load statistics
                var totalCustomers = _adminService.GetTotalCustomers();
                var totalProducts = _adminService.GetTotalProducts();
                var totalOrders = _adminService.GetTotalOrders();
                var totalRevenue = _adminService.GetTotalRevenue();

                // Update UI
                TotalCustomersText.Text = totalCustomers.ToString("N0");
                TotalProductsText.Text = totalProducts.ToString("N0");
                TotalOrdersText.Text = totalOrders.ToString("N0");
                TotalRevenueText.Text = $"{totalRevenue:N0}?";

                // Load recent orders
                var recentOrders = _adminService.GetRecentOrders(10);
                RecentOrdersGrid.ItemsSource = recentOrders;

                // Load low stock products
                var lowStockProducts = _adminService.GetLowStockProducts(10);
                LowStockGrid.ItemsSource = lowStockProducts;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"L?i khi t?i d? li?u dashboard: {ex.Message}", "L?i", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}