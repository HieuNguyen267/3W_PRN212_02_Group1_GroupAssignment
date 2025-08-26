using BLL.Services;
using DAL.Entities;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ShoppingOnline.Views;

namespace ShoppingOnline
{
    public partial class AdminDashboardWindow : Window, INotifyPropertyChanged
    {
        private readonly IAdminService _adminService;
        private AdminDashboardView? _dashboardView;
        private AdminProductsView? _productsView;
        private AdminOrdersView? _ordersView;

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
            NavigateToDashboard(); // Load dashboard by default
        }

        private void InitializeAdminInfo()
        {
            AdminNameText.Text = AdminSession.AdminName ?? "Admin User";
            AdminEmailText.Text = AdminSession.Email ?? "admin@shop.com";
        }

        private void NavigateToDashboard()
        {
            if (_dashboardView == null)
                _dashboardView = new AdminDashboardView();
            
            MainContentControl.Content = _dashboardView;
            UpdatePageTitle("Dashboard", "Tong quan he thong");
            UpdateActiveButton("Dashboard");
            
            // Refresh dashboard data
            _dashboardView.LoadDashboardData();
        }

        private void NavigateToProducts()
        {
            if (_productsView == null)
                _productsView = new AdminProductsView();
            
            MainContentControl.Content = _productsView;
            UpdatePageTitle("Quan ly San pham", "Danh sach va thong tin san pham");
            UpdateActiveButton("Products");
        }

        private void NavigateToOrders()
        {
            if (_ordersView == null)
                _ordersView = new AdminOrdersView();
            
            MainContentControl.Content = _ordersView;
            UpdatePageTitle("Quan ly Don hang", "Danh sach va trang thai don hang");
            UpdateActiveButton("Orders");
        }

        private void NavigateToCustomers()
        {
            // Use the actual AdminCustomersView instead of placeholder
            var customersView = new AdminCustomersView();
            
            MainContentControl.Content = customersView;
            UpdatePageTitle("Quan ly Khach hang", "Danh sach khach hang");
            UpdateActiveButton("Customers");
        }

        private void NavigateToCategories()
        {
            // Use placeholder since AdminCategoriesView doesn't exist yet
            var categoriesView = new TextBlock 
            { 
                Text = "Quan ly Danh muc - Dang phat trien", 
                FontSize = 24,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(50)
            };
            MainContentControl.Content = categoriesView;
            
            UpdatePageTitle("Quan ly Danh muc", "Danh sach danh muc san pham");
            UpdateActiveButton("Categories");
        }

        private void NavigateToAdmins()
        {
            // Use the actual AdminAdminsView instead of placeholder
            var adminsView = new AdminAdminsView();
            
            MainContentControl.Content = adminsView;
            UpdatePageTitle("Quan ly Admin", "Quan ly tai khoan admin");
            UpdateActiveButton("Admins");
        }

        private void NavigateToReports()
        {
            // Use placeholder for reports
            var reportsView = new TextBlock 
            { 
                Text = "Bao cao - Dang phat trien", 
                FontSize = 24,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(50)
            };
            MainContentControl.Content = reportsView;
            UpdatePageTitle("Bao cao", "Thong ke va bao cao");
            UpdateActiveButton("Reports");
        }

        private void NavigateToCarriers()
        {
            // Use the actual AdminCarriersView 
            var carriersView = new AdminCarriersView();
            
            MainContentControl.Content = carriersView;
            UpdatePageTitle("Quan ly Nha van chuyen", "Danh sach nha van chuyen");
            UpdateActiveButton("Carriers");
        }

        private void UpdatePageTitle(string title, string subtitle)
        {
            PageTitle.Text = title;
            PageSubtitle.Text = subtitle;
        }

        private void UpdateActiveButton(string activeButton)
        {
            // Reset all buttons to normal style
            DashboardBtn.Style = (Style)FindResource("SidebarButton");
            CustomersBtn.Style = (Style)FindResource("SidebarButton");
            ProductsBtn.Style = (Style)FindResource("SidebarButton");
            OrdersBtn.Style = (Style)FindResource("SidebarButton");
            CategoriesBtn.Style = (Style)FindResource("SidebarButton");
            AdminsBtn.Style = (Style)FindResource("SidebarButton");
            ReportsBtn.Style = (Style)FindResource("SidebarButton");
            CarriersBtn.Style = (Style)FindResource("SidebarButton");

            // Set active button style
            switch (activeButton)
            {
                case "Dashboard":
                    DashboardBtn.Style = (Style)FindResource("ActiveSidebarButton");
                    break;
                case "Customers":
                    CustomersBtn.Style = (Style)FindResource("ActiveSidebarButton");
                    break;
                case "Products":
                    ProductsBtn.Style = (Style)FindResource("ActiveSidebarButton");
                    break;
                case "Orders":
                    OrdersBtn.Style = (Style)FindResource("ActiveSidebarButton");
                    break;
                case "Categories":
                    CategoriesBtn.Style = (Style)FindResource("ActiveSidebarButton");
                    break;
                case "Admins":
                    AdminsBtn.Style = (Style)FindResource("ActiveSidebarButton");
                    break;
                case "Reports":
                    ReportsBtn.Style = (Style)FindResource("ActiveSidebarButton");
                    break;
                case "Carriers":
                    CarriersBtn.Style = (Style)FindResource("ActiveSidebarButton");
                    break;
            }
        }

        #region Navigation Methods
        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            NavigateToDashboard();
        }

        private void Customers_Click(object sender, RoutedEventArgs e)
        {
            NavigateToCustomers();
        }

        private void Products_Click(object sender, RoutedEventArgs e)
        {
            NavigateToProducts();
        }

        private void Orders_Click(object sender, RoutedEventArgs e)
        {
            NavigateToOrders();
        }

        private void Categories_Click(object sender, RoutedEventArgs e)
        {
            NavigateToCategories();
        }

        private void Admins_Click(object sender, RoutedEventArgs e)
        {
            NavigateToAdmins();
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            NavigateToReports();
        }

        private void Carriers_Click(object sender, RoutedEventArgs e)
        {
            NavigateToCarriers();
        }
        #endregion

        #region Event Handlers
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("B?n có mu?n ??ng xu?t kh?i Admin Panel?", 
                "Xác nh?n ??ng xu?t", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                AdminSession.Logout();
                MessageBox.Show("?ã ??ng xu?t thành công!", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                
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