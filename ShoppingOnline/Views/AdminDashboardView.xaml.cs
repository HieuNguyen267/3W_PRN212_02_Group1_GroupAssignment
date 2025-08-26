using BLL.Services;
using DAL.Entities;
using System;
using System.ComponentModel;
using System.Linq;
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
                var totalOrderValue = _adminService.GetTotalOrderValue(); // Changed from GetTotalRevenue

                // Debug logging
                System.Diagnostics.Debug.WriteLine($"Dashboard Stats - Customers: {totalCustomers}, Products: {totalProducts}, Orders: {totalOrders}, Order Value: {totalOrderValue}");

                // Update UI with VND currency
                TotalCustomersText.Text = totalCustomers.ToString("N0");
                TotalProductsText.Text = totalProducts.ToString("N0");
                TotalOrdersText.Text = totalOrders.ToString("N0");
                TotalRevenueText.Text = totalOrderValue > 0 ? $"{totalOrderValue:N0} VND" : "0 VND"; // Changed to show total order value

                // Load recent orders with product information
                var recentOrders = _adminService.GetRecentOrders(10);
                var recentOrdersWithProducts = recentOrders.Select(o => 
                {
                    // Get order details to show product information
                    var orderDetails = _adminService.GetOrderDetailsByOrderId(o.OrderId);
                    var productNames = orderDetails.Any() 
                        ? string.Join(", ", orderDetails.Take(2).Select(od => od.Product?.ProductName).Where(name => !string.IsNullOrEmpty(name)))
                        : "Khong co san pham";
                    
                    // Add "..." if there are more than 2 products
                    if (orderDetails.Count > 2)
                    {
                        productNames += "...";
                    }
                    
                    var totalQuantity = orderDetails.Sum(od => od.Quantity);
                    
                    return new DashboardOrderViewModel
                    {
                        OrderId = o.OrderId,
                        Customer = o.Customer,
                        TotalAmount = o.TotalAmount,
                        Status = o.Status,
                        ProductNames = productNames,
                        TotalQuantity = totalQuantity
                    };
                }).ToList();
                
                RecentOrdersGrid.ItemsSource = recentOrdersWithProducts;

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

    // ViewModel for dashboard recent orders with product information
    public class DashboardOrderViewModel
    {
        public int OrderId { get; set; }
        public Customer? Customer { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Status { get; set; }
        public string ProductNames { get; set; } = "";
        public int TotalQuantity { get; set; }
    }
}