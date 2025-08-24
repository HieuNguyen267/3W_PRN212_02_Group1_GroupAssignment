using BLL.Services;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
                MessageBox.Show($"Loi khi tai du lieu dashboard: {ex.Message}", "Loi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Navigation Methods
        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            ShowContent("Dashboard");
            PageTitle.Text = "[D] Dashboard";
            PageSubtitle.Text = "Tong quan he thong";
            LoadDashboardData();
        }

        private void Customers_Click(object sender, RoutedEventArgs e)
        {
            ShowContent("Customers");
            PageTitle.Text = "[U] Quan ly Khach hang";
            PageSubtitle.Text = "Danh sach va thong tin khach hang";
            LoadCustomersData();
        }

        private void Products_Click(object sender, RoutedEventArgs e)
        {
            ShowContent("Products");
            PageTitle.Text = "[P] Quan ly San pham";
            PageSubtitle.Text = "Danh sach va thong tin san pham";
            LoadProductsData();
        }

        private void Orders_Click(object sender, RoutedEventArgs e)
        {
            ShowContent("Orders");
            PageTitle.Text = "[O] Quan ly Don hang";
            PageSubtitle.Text = "Danh sach va trang thai don hang";
            LoadOrdersData();
        }

        private void Categories_Click(object sender, RoutedEventArgs e)
        {
            ShowContent("Categories");
            PageTitle.Text = "[C] Quan ly Danh muc";
            PageSubtitle.Text = "Danh sach va phan loai san pham";
            LoadCategoriesData();
        }

        private void Admins_Click(object sender, RoutedEventArgs e)
        {
            ShowContent("Admins");
            PageTitle.Text = "[A] Quan ly Admin";
            PageSubtitle.Text = "Danh sach quan tri vien";
            LoadAdminsData();
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            ShowContent("Reports");
            PageTitle.Text = "[R] Bao cao";
            PageSubtitle.Text = "Thong ke va phan tich";
            LoadReportsData();
        }

        private void ShowContent(string contentName)
        {
            // Hide all content panels
            DashboardContent.Visibility = Visibility.Collapsed;
            CustomersContent.Visibility = Visibility.Collapsed;
            ProductsContent.Visibility = Visibility.Collapsed;
            OrdersContent.Visibility = Visibility.Collapsed;
            CategoriesContent.Visibility = Visibility.Collapsed;
            AdminsContent.Visibility = Visibility.Collapsed;
            ReportsContent.Visibility = Visibility.Collapsed;

            // Show selected content
            switch (contentName)
            {
                case "Dashboard":
                    DashboardContent.Visibility = Visibility.Visible;
                    break;
                case "Customers":
                    CustomersContent.Visibility = Visibility.Visible;
                    break;
                case "Products":
                    ProductsContent.Visibility = Visibility.Visible;
                    break;
                case "Orders":
                    OrdersContent.Visibility = Visibility.Visible;
                    break;
                case "Categories":
                    CategoriesContent.Visibility = Visibility.Visible;
                    break;
                case "Admins":
                    AdminsContent.Visibility = Visibility.Visible;
                    break;
                case "Reports":
                    ReportsContent.Visibility = Visibility.Visible;
                    break;
            }
        }
        #endregion

        #region Data Loading Methods
        private void LoadCustomersData()
        {
            try
            {
                // Load customers data
                var customers = _adminService.GetAllCustomers();
                // TODO: Populate customers content
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi t?i d? li?u khách hàng: {ex.Message}", "L?i", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadProductsData()
        {
            try
            {
                // Load products data
                var products = _adminService.GetAllProducts();
                // TODO: Populate products content
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi t?i d? li?u s?n ph?m: {ex.Message}", "L?i", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadOrdersData()
        {
            try
            {
                // Load orders data
                var orders = _adminService.GetAllOrders();
                // TODO: Populate orders content
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi t?i d? li?u ??n hàng: {ex.Message}", "L?i", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCategoriesData()
        {
            try
            {
                // Load categories data
                var categories = _adminService.GetAllCategories();
                // TODO: Populate categories content
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi t?i d? li?u danh m?c: {ex.Message}", "L?i", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAdminsData()
        {
            try
            {
                // Load admins data
                var admins = _adminService.GetAllAdmins();
                // TODO: Populate admins content
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi t?i d? li?u admin: {ex.Message}", "L?i", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadReportsData()
        {
            try
            {
                // Load reports data
                // TODO: Populate reports content
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi t?i d? li?u báo cáo: {ex.Message}", "L?i", 
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
            var result = MessageBox.Show($"Ban co muon dang xuat khoi Admin Panel?", 
                "Xac nhan dang xuat", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                AdminSession.Logout();
                MessageBox.Show("Da dang xuat thanh cong!", "Thong bao", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Close the current admin dashboard
                this.Close();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            var result = MessageBox.Show("Ban co muon thoat Admin Dashboard?", 
                "Xac nhan thoat", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
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