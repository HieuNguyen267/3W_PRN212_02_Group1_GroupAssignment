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
                    var result = MessageBox.Show("C? s? d? li?u tr?ng! B?n c� mu?n t?o d? li?u m?u kh�ng?", "Th�ng b�o", 
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
                MessageBox.Show($"L?i k?t n?i c? s? d? li?u: {ex.Message}\n\nVui l�ng ki?m tra:\n1. SQL Server ?� ch?y\n2. Database 'ShoppingOnline' t?n t?i\n3. Connection string ?�ng", 
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
                        Customer = new Customer { FullName = "Tr?n Th? B", Phone = "0907654321", Address = "456 ???ng XYZ, H� N?i", CreatedDate = DateTime.Now, UpdatedDate = DateTime.Now }
                    },
                    new { 
                        Account = new Account { Username = "customer3", Email = "customer3@example.com", Password = "123456", AccountType = "Customer", CreatedDate = DateTime.Now, IsActive = false },
                        Customer = new Customer { FullName = "L� V?n C", Phone = "0912345678", Address = "789 ???ng DEF, ?� N?ng", CreatedDate = DateTime.Now, UpdatedDate = DateTime.Now }
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
                
                MessageBox.Show("?� t?o d? li?u m?u th�nh c�ng!", "Th�nh c�ng", 
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
                MessageBox.Show($"L?i khi t?i d? li?u kh�ch h�ng: {ex.Message}", "L?i", 
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
            MessageBox.Show("?� l�m m?i danh s�ch kh�ch h�ng!", "Th�ng b�o", 
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
                MessageBox.Show($"L?i khi m? c?a s? th�m kh�ch h�ng:\n{ex.Message}", "L?i", 
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
                        var info = $"Th�ng tin kh�ch h�ng #{customerId}\n\n" +
                                  $"T�n: {customer.FullName}\n" +
                                  $"Email: {customer.Account?.Email}\n" +
                                  $"?i?n tho?i: {customer.Phone}\n" +
                                  $"??a ch?: {customer.Address}\n" +
                                  $"Ng�y t?o: {customer.CreatedDate:dd/MM/yyyy}\n" +
                                  $"Tr?ng th�i: {(customer.Account?.IsActive == true ? "Ho?t ??ng" : "Kh�a")}";
                        
                        MessageBox.Show(info, "Th�ng tin kh�ch h�ng", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"L?i khi xem th�ng tin kh�ch h�ng: {ex.Message}", "L?i", 
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
                        MessageBox.Show($"Kh�ng t�m th?y kh�ch h�ng #{customerId}!\n\nKh�ch h�ng c� th? ?� b? x�a ho?c kh�ng t?n t?i.", 
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
                    MessageBox.Show($"L?i khi m? c?a s? s?a th�ng tin kh�ch h�ng:\n{ex.Message}", "L?i", 
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
                        string action = newStatus ? "k�ch ho?t" : "kh�a";
                        
                        var result = MessageBox.Show($"B?n c� mu?n {action} t�i kho?n n�y?\n\n" +
                                                   $"� T�n: {customer.FullName}\n" +
                                                   $"� Email: {customer.Account?.Email}\n\n" +
                                                   $"Thao t�c n�y s? {action} t�i kho?n kh�ch h�ng.", 
                            $"X�c nh?n {action}", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        
                        if (result == MessageBoxResult.Yes)
                        {
                            if (_adminService.UpdateCustomerStatus(customerId, newStatus))
                            {
                                LoadCustomers();
                                MessageBox.Show($"?� {action} t�i kho?n th�nh c�ng!", "Th�nh c�ng", 
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show($"Kh�ng th? {action} t�i kho?n!", "L?i", 
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"L?i khi thay ??i tr?ng th�i: {ex.Message}", "L?i", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int customerId)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"AdminCustomersView: Attempting to delete customer {customerId}");
                    
                    var customer = _allCustomers.FirstOrDefault(c => c.CustomerId == customerId);
                    if (customer == null)
                    {
                        MessageBox.Show($"Kh�ng t�m th?y kh�ch h�ng #{customerId}!\n\nKh�ch h�ng c� th? ?� b? x�a ho?c kh�ng t?n t?i.", 
                            "L?i", MessageBoxButton.OK, MessageBoxImage.Warning);
                        LoadCustomers(); // Refresh the list
                        return;
                    }
                    
                    // Check if customer has orders
                    bool hasOrders = _adminService.CustomerHasOrders(customerId);
                    System.Diagnostics.Debug.WriteLine($"AdminCustomersView: Customer {customerId} has orders: {hasOrders}");
                    
                    string message;
                    if (hasOrders)
                    {
                        message = $"?? TH�NG B�O: KH�CH H�NG C� ??N H�NG ??\n\n" +
                                 $"Kh�ch h�ng #{customerId} ?� c� ??n h�ng trong h? th?ng.\n\n" +
                                 $"� T�n: {customer.FullName}\n" +
                                 $"� Email: {customer.Account?.Email}\n\n" +
                                 $"THAO T�C N�Y S?:\n" +
                                 $"? V� HI?U H�A t�i kho?n (kh�ng x�a ho�n to�n)\n" +
                                 $"? Gi? l?i d? li?u ??n h�ng\n" +
                                 $"? Kh�ch h�ng kh�ng th? ??ng nh?p\n\n" +
                                 $"B?n c� mu?n v� hi?u h�a t�i kho?n n�y?";
                    }
                    else
                    {
                        message = $"?? C?NH B�O: X�A HO�N TO�N KH�CH H�NG ??\n\n" +
                                 $"Kh�ch h�ng #{customerId} ch?a c� ??n h�ng n�o.\n\n" +
                                 $"� T�n: {customer.FullName}\n" +
                                 $"� Email: {customer.Account?.Email}\n\n" +
                                 $"THAO T�C N�Y S?:\n" +
                                 $"? X�A HO�N TO�N kh�ch h�ng\n" +
                                 $"? X�A HO�N TO�N t�i kho?n\n" +
                                 $"? KH�NG TH? KH�I PH?C\n\n" +
                                 $"B?n c� ch?c ch?n mu?n x�a ho�n to�n?";
                    }
                    
                    var result = MessageBox.Show(message, hasOrders ? "V� HI?U H�A T�I KHO?N" : "?? X�A HO�N TO�N KH�CH H�NG", 
                        MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Debug.WriteLine($"AdminCustomersView: User confirmed deletion of customer {customerId}");
                        
                        bool deleteSuccess = _adminService.DeleteCustomer(customerId);
                        System.Diagnostics.Debug.WriteLine($"AdminCustomersView: Delete operation result: {deleteSuccess}");
                        
                        if (deleteSuccess)
                        {
                            LoadCustomers();
                            string successMessage = hasOrders ? 
                                "?� v� hi?u h�a t�i kho?n kh�ch h�ng th�nh c�ng!" : 
                                "?� x�a ho�n to�n kh�ch h�ng th�nh c�ng!";
                            MessageBox.Show(successMessage, "Th�nh c�ng", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Kh�ng th? x�a kh�ch h�ng!\n\n" +
                                           "C� th? do:\n" +
                                           "� Kh�ch h�ng kh�ng t?n t?i\n" +
                                           "� L?i c? s? d? li?u\n" +
                                           "� R�ng bu?c d? li?u\n" +
                                           "� Kh�ch h�ng ?ang c� giao d?ch ?ang x? l�", 
                                "L?i", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"AdminCustomersView: User cancelled deletion of customer {customerId}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"AdminCustomersView: Error in DeleteCustomer_Click: {ex.Message}");
                    MessageBox.Show($"L?i khi x�a kh�ch h�ng:\n{ex.Message}\n\n" +
                                   "Vui l�ng th? l?i ho?c li�n h? qu?n tr? vi�n.", 
                        "L?i h? th?ng", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
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