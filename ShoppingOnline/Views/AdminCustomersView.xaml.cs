using BLL.Services;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DAL;
using Microsoft.EntityFrameworkCore;

namespace ShoppingOnline.Views
{
    // Converters for better data display
    public class BoolToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isActive)
            {
                return isActive ? "Active" : "Locked";
            }
            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToToggleButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isActive)
            {
                return isActive ? "Lock" : "Activate";
            }
            return "N/A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToToggleButtonBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isActive)
            {
                return isActive ? "#FFF44336" : "#FF4CAF50"; // Red for deactivate, Green for activate
            }
            return "#FF9E9E9E"; // Gray for unknown
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class AdminCustomersView : UserControl, INotifyPropertyChanged
    {
        private readonly IAdminService _adminService;
        private ObservableCollection<Customer> _customers = new();
        private List<Customer> _allCustomers = new();

        public AdminCustomersView()
        {
            InitializeComponent();
            _adminService = new AdminService();
            DataContext = this;
            
            Loaded += AdminCustomersView_Loaded;
        }

        private void AdminCustomersView_Loaded(object sender, RoutedEventArgs e)
        {
            // Test database connection first
            TestDatabaseConnection();
            LoadCustomers();
        }
        
        private void TestDatabaseConnection()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("AdminCustomersView: Testing database connection...");
                
                using var context = new ShoppingOnlineContext();
                
                // Test if we can connect and query the database
                var customerCount = context.Customers.Count();
                var accountCount = context.Accounts.Count();
                
                System.Diagnostics.Debug.WriteLine($"AdminCustomersView: Database connection OK - Customers: {customerCount}, Accounts: {accountCount}");
                
                // If no customers exist, offer to create sample data
                if (customerCount == 0)
                {
                    var result = MessageBox.Show("C? s? d? li?u tr?ng! B?n có mu?n t?o d? li?u m?u không?", "Thông báo", 
                        MessageBoxButton.YesNo, MessageBoxImage.Question);
                        
                    if (result == MessageBoxResult.Yes)
                    {
                        CreateSampleData();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AdminCustomersView: Database connection failed: {ex.Message}");
                MessageBox.Show($"L?i k?t n?i c? s? d? li?u: {ex.Message}\n\nVui lòng ki?m tra:\n1. SQL Server ?ã ch?y\n2. Database 'ShoppingOnline' t?n t?i\n3. Connection string ?úng", 
                    "L?i Database", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void CreateSampleData()
        {
            try
            {
                using var context = new ShoppingOnlineContext();
                
                // Create sample customers with accounts
                var sampleCustomers = new[]
                {
                    new { 
                        Account = new Account { Username = "customer1", Email = "customer1@example.com", Password = "123456", AccountType = "Customer", CreatedDate = DateTime.Now, IsActive = true },
                        Customer = new Customer { FullName = "Nguy?n V?n A", Phone = "0901234567", Address = "123 ???ng ABC, TP.HCM", CreatedDate = DateTime.Now, UpdatedDate = DateTime.Now }
                    },
                    new { 
                        Account = new Account { Username = "customer2", Email = "customer2@example.com", Password = "123456", AccountType = "Customer", CreatedDate = DateTime.Now, IsActive = true },
                        Customer = new Customer { FullName = "Tr?n Th? B", Phone = "0907654321", Address = "456 ???ng XYZ, Hà N?i", CreatedDate = DateTime.Now, UpdatedDate = DateTime.Now }
                    },
                    new { 
                        Account = new Account { Username = "customer3", Email = "customer3@example.com", Password = "123456", AccountType = "Customer", CreatedDate = DateTime.Now, IsActive = false },
                        Customer = new Customer { FullName = "Lê V?n C", Phone = "0912345678", Address = "789 ???ng DEF, ?à N?ng", CreatedDate = DateTime.Now, UpdatedDate = DateTime.Now }
                    }
                };
                
                foreach (var sample in sampleCustomers)
                {
                    context.Accounts.Add(sample.Account);
                    context.SaveChanges();
                    
                    sample.Customer.AccountId = sample.Account.AccountId;
                    context.Customers.Add(sample.Customer);
                }
                
                context.SaveChanges();
                
                MessageBox.Show("?ã t?o d? li?u m?u thành công!", "Thành công", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                    
                System.Diagnostics.Debug.WriteLine("AdminCustomersView: Sample data created successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AdminCustomersView: Failed to create sample data: {ex.Message}");
                MessageBox.Show($"L?i khi t?o d? li?u m?u: {ex.Message}", "L?i", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public ObservableCollection<Customer> Customers
        {
            get => _customers;
            set
            {
                _customers = value;
                OnPropertyChanged(nameof(Customers));
            }
        }

        private void LoadCustomers()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("AdminCustomersView: Starting to load customers...");
                _allCustomers = _adminService.GetAllCustomers();
                System.Diagnostics.Debug.WriteLine($"AdminCustomersView: Loaded {_allCustomers.Count} customers from database");
                
                ApplyCustomerFilter();
                
                System.Diagnostics.Debug.WriteLine("AdminCustomersView: Customers loaded successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AdminCustomersView: Error loading customers: {ex.Message}");
                MessageBox.Show($"L?i khi t?i d? li?u khách hàng: {ex.Message}", "L?i", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyCustomerFilter()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"AdminCustomersView: Starting filter with {_allCustomers.Count} total customers");
                
                var filteredCustomers = _allCustomers.AsEnumerable();

                // Apply search filter
                var searchText = CustomerSearchBox?.Text?.Trim();
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    var searchLower = searchText.ToLower();
                    filteredCustomers = filteredCustomers.Where(c => 
                        (c.FullName?.ToLower().Contains(searchLower) == true) ||
                        (c.Account?.Email?.ToLower().Contains(searchLower) == true) ||
                        (c.Phone?.ToLower().Contains(searchLower) == true) ||
                        (c.Address?.ToLower().Contains(searchLower) == true));
                    
                    System.Diagnostics.Debug.WriteLine($"AdminCustomersView: After search filter: {filteredCustomers.Count()} customers");
                }

                // Apply status filter
                var selectedStatus = (StatusFilterComboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString();
                if (!string.IsNullOrEmpty(selectedStatus) && selectedStatus != "All Status")
                {
                    bool isActive = selectedStatus == "Active";
                    filteredCustomers = filteredCustomers.Where(c => c.Account?.IsActive == isActive);
                    
                    System.Diagnostics.Debug.WriteLine($"AdminCustomersView: After status filter: {filteredCustomers.Count()} customers");
                }

                // Update customers collection
                Customers.Clear();
                foreach (var customer in filteredCustomers.OrderBy(c => c.FullName))
                {
                    Customers.Add(customer);
                }

                // Update DataGrid
                if (CustomersDataGrid != null)
                {
                    CustomersDataGrid.ItemsSource = Customers;
                    System.Diagnostics.Debug.WriteLine($"AdminCustomersView: DataGrid updated with {Customers.Count} customers");
                }
                
                // Update count
                if (CustomerCountText != null)
                {
                    CustomerCountText.Text = $"Total: {filteredCustomers.Count()} customers";
                    System.Diagnostics.Debug.WriteLine($"AdminCustomersView: Count text updated to: {CustomerCountText.Text}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AdminCustomersView: Error in ApplyCustomerFilter: {ex.Message}");
                MessageBox.Show($"Error filtering customers: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Event Handlers
        private void CustomerSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CustomerSearchBox != null)
            {
                CustomerSearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(CustomerSearchBox.Text) 
                    ? Visibility.Visible : Visibility.Hidden;
                
                ApplyCustomerFilter();
            }
        }

        private void StatusFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyCustomerFilter();
        }

        private void RefreshCustomers_Click(object sender, RoutedEventArgs e)
        {
            LoadCustomers();
            MessageBox.Show("?ã làm m?i danh sách khách hàng!", "Thông báo", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddCustomer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("AdminCustomersView: Opening Add Customer window");
                var addCustomerWindow = new CustomerEditWindow();
                var result = addCustomerWindow.ShowDialog();
                
                System.Diagnostics.Debug.WriteLine($"AdminCustomersView: Add Customer window result: {result}");
                
                if (result == true)
                {
                    System.Diagnostics.Debug.WriteLine("AdminCustomersView: Customer added successfully, refreshing list");
                    LoadCustomers();
                    // Don't show additional success message here as CustomerEditWindow already shows one
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AdminCustomersView: Error in AddCustomer_Click: {ex.Message}");
                MessageBox.Show($"L?i khi m? c?a s? thêm khách hàng:\n{ex.Message}", "L?i", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int customerId)
            {
                try
                {
                    var customer = _allCustomers.FirstOrDefault(c => c.CustomerId == customerId);
                    if (customer != null)
                    {
                        var info = $"Thông tin khách hàng #{customerId}\n\n" +
                                  $"Tên: {customer.FullName}\n" +
                                  $"Email: {customer.Account?.Email}\n" +
                                  $"?i?n tho?i: {customer.Phone}\n" +
                                  $"??a ch?: {customer.Address}\n" +
                                  $"Ngày t?o: {customer.CreatedDate:dd/MM/yyyy}\n" +
                                  $"Tr?ng thái: {(customer.Account?.IsActive == true ? "Ho?t ??ng" : "Khóa")}";
                        
                        MessageBox.Show(info, "Thông tin khách hàng", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"L?i khi xem thông tin khách hàng: {ex.Message}", "L?i", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int customerId)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"AdminCustomersView: Editing customer {customerId}");
                    var customer = _allCustomers.FirstOrDefault(c => c.CustomerId == customerId);
                    if (customer == null)
                    {
                        MessageBox.Show($"Không tìm th?y khách hàng #{customerId}!\n\nKhách hàng có th? ?ã b? xóa ho?c không t?n t?i.", 
                            "L?i", MessageBoxButton.OK, MessageBoxImage.Warning);
                        LoadCustomers(); // Refresh the list
                        return;
                    }

                    var editCustomerWindow = new CustomerEditWindow(customer);
                    var result = editCustomerWindow.ShowDialog();
                    
                    System.Diagnostics.Debug.WriteLine($"AdminCustomersView: Edit Customer window result: {result}");
                    
                    if (result == true)
                    {
                        System.Diagnostics.Debug.WriteLine("AdminCustomersView: Customer updated successfully, refreshing list");
                        LoadCustomers();
                        // Don't show additional success message here as CustomerEditWindow already shows one
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"AdminCustomersView: Error in EditCustomer_Click: {ex.Message}");
                    MessageBox.Show($"L?i khi m? c?a s? s?a thông tin khách hàng:\n{ex.Message}", "L?i", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BlockCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int customerId)
            {
                try
                {
                    var customer = _allCustomers.FirstOrDefault(c => c.CustomerId == customerId);
                    if (customer?.Account != null)
                    {
                        bool newStatus = !customer.Account.IsActive.GetValueOrDefault();
                        string action = newStatus ? "kích ho?t" : "khóa";
                        
                        var result = MessageBox.Show($"B?n có mu?n {action} tài kho?n này?\n\n" +
                                                   $"• Tên: {customer.FullName}\n" +
                                                   $"• Email: {customer.Account?.Email}\n\n" +
                                                   $"Thao tác này s? {action} tài kho?n khách hàng.", 
                            $"Xác nh?n {action}", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        
                        if (result == MessageBoxResult.Yes)
                        {
                            if (_adminService.UpdateCustomerStatus(customerId, newStatus))
                            {
                                LoadCustomers();
                                MessageBox.Show($"?ã {action} tài kho?n thành công!", "Thành công", 
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show($"Không th? {action} tài kho?n!", "L?i", 
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"L?i khi thay ??i tr?ng thái: {ex.Message}", "L?i", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button))
                return;

            // robustly parse Tag (support boxed int, string, etc.)
            if (!TryGetCustomerIdFromTag(button.Tag, out int customerId))
                return;

            try
            {
                System.Diagnostics.Debug.WriteLine($"AdminCustomersView: Attempting to delete customer {customerId}");

                var customer = _adminService.GetCustomerById(customerId);
                if (customer == null)
                {
                    MessageBox.Show($"Không tìm thấy khách hàng #{customerId}.\nKhách hàng có thể đã bị xóa hoặc không tồn tại.",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    LoadCustomers();
                    return;
                }

                bool hasOrders = _adminService.CustomerHasOrders(customerId);
                System.Diagnostics.Debug.WriteLine($"AdminCustomersView: Customer {customerId} has orders: {hasOrders}");

                string caption = hasOrders ? "VÔ HIỆU HÓA TÀI KHOẢN" : "XÓA HOÀN TOÀN KHÁCH HÀNG";
                string message = hasOrders
                    ? $"Khách hàng #{customerId} đã có đơn hàng.\n\n• Tên: {customer.FullName}\n• Email: {customer.Account?.Email}\n\nBạn có chắc muốn vô hiệu hóa tài khoản?"
                    : $"Khách hàng #{customerId} chưa có đơn hàng.\n\n• Tên: {customer.FullName}\n• Email: {customer.Account?.Email}\n\nBạn có chắc muốn xóa hoàn toàn?";

                if (MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                {
                    System.Diagnostics.Debug.WriteLine($"AdminCustomersView: User cancelled deletion of customer {customerId}");
                    return;
                }

                bool deleteSuccess = _adminService.DeleteCustomer(customerId);
                System.Diagnostics.Debug.WriteLine($"AdminCustomersView: Delete operation result: {deleteSuccess}");

                if (deleteSuccess)
                {
                    LoadCustomers();
                    string successMessage = hasOrders
                        ? "Tài khoản khách hàng đã được vô hiệu hóa thành công!"
                        : "Khách hàng và tài khoản đã được xóa hoàn toàn thành công!";
                    MessageBox.Show(successMessage, "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Không thể xóa khách hàng. Vui lòng kiểm tra nhật ký hoặc thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AdminCustomersView: Error in DeleteCustomer_Click: {ex}");
                MessageBox.Show($"Lỗi khi xóa khách hàng:\n{ex.Message}\n\nVui lòng thử lại hoặc liên hệ quản trị viên.",
                                "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool TryGetCustomerIdFromTag(object tag, out int customerId)
        {
            customerId = 0;
            if (tag == null) return false;
            if (tag is int id) { customerId = id; return true; }
            if (int.TryParse(tag.ToString(), out id)) { customerId = id; return true; }
            return false;
        }

        private void ToggleCustomerStatus_Click(object sender, RoutedEventArgs e)
        {
            // This method is deprecated - use BlockCustomer_Click instead
            BlockCustomer_Click(sender, e);
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