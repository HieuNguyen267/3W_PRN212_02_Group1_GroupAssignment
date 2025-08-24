using BLL.Services;
using DAL.Entities;
using System;
using System.ComponentModel;
using System.Windows;

namespace ShoppingOnline
{
    public partial class AdminDashboardWindow : Window, INotifyPropertyChanged
    {
        private readonly IAdminService _adminService;

        public AdminDashboardWindow()
        {
            InitializeComponent();
            _adminService = new AdminService();
            DataContext = this;
            
            // Check if admin is logged in
            if (!AdminSession.IsLoggedIn)
            {
                MessageBox.Show("Vui long dang nhap voi tai khoan Admin!", "Thong bao", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                this.Close();
                return;
            }
            
            InitializeAdminInfo();
            LoadDashboardData();
        }

        private void InitializeAdminInfo()
        {
            AdminNameText.Text = AdminSession.AdminName ?? "Admin User";
            AdminEmailText.Text = AdminSession.Email ?? "admin@shop.com";
        }

        private void LoadDashboardData()
        {
            try
            {
                // Load statistics
                var totalCustomers = _adminService.GetTotalCustomers();
                var totalProducts = _adminService.GetTotalProducts();
                var totalOrders = _adminService.GetTotalOrders();
                var totalRevenue = _adminService.GetTotalRevenue();
                var todayOrders = _adminService.GetTodayOrders();
                var todayRevenue = _adminService.GetTodayRevenue();

                // Update UI
                TotalCustomersText.Text = totalCustomers.ToString("N0");
                TotalProductsText.Text = totalProducts.ToString("N0");
                TotalOrdersText.Text = totalOrders.ToString("N0");
                TotalRevenueText.Text = $"{totalRevenue:N0}?";
                TodayOrdersText.Text = todayOrders.ToString("N0");
                TodayRevenueText.Text = $"{todayRevenue:N0}?";

                // Load recent orders
                var recentOrders = _adminService.GetRecentOrders(10);
                RecentOrdersGrid.ItemsSource = recentOrders;

                // Load low stock products
                var lowStockProducts = _adminService.GetLowStockProducts(10);
                LowStockGrid.ItemsSource = lowStockProducts;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi t?i d? li?u dashboard: {ex.Message}", "L?i", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Navigation Methods
        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            // Already on dashboard, just refresh data
            LoadDashboardData();
        }

        private void Customers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var customersWindow = new AdminCustomersWindow();
                customersWindow.Owner = this;
                customersWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi khi mo quan ly khach hang: {ex.Message}", "Loi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Products_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var productsWindow = new AdminProductsWindow();
                productsWindow.Owner = this;
                productsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi khi mo quan ly san pham: {ex.Message}", "Loi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Orders_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ordersWindow = new AdminOrdersWindow();
                ordersWindow.Owner = this;
                ordersWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi khi mo quan ly don hang: {ex.Message}", "Loi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Categories_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var categoriesWindow = new AdminCategoriesWindow();
                categoriesWindow.Owner = this;
                categoriesWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi khi mo quan ly danh muc: {ex.Message}", "Loi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Admins_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var adminManagementWindow = new AdminManagementWindow();
                adminManagementWindow.Owner = this;
                adminManagementWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi khi mo quan ly admin: {ex.Message}", "Loi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var reportsWindow = new AdminReportsWindow();
                reportsWindow.Owner = this;
                reportsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi khi mo bao cao: {ex.Message}", "Loi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Event Handlers
        private void ViewAllOrders_Click(object sender, RoutedEventArgs e)
        {
            Orders_Click(sender, e);
        }

        private void ManageStock_Click(object sender, RoutedEventArgs e)
        {
            Products_Click(sender, e);
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("B?n có mu?n ??ng xu?t kh?i Admin Panel?", 
                "Xác nh?n ??ng xu?t", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                AdminSession.Logout();
                MessageBox.Show("?ã ??ng xu?t thành công!", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Close the current admin dashboard
                this.Close();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            var result = MessageBox.Show("B?n có mu?n thoát Admin Dashboard?", 
                "Xác nh?n thoát", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                AdminSession.Logout();
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}