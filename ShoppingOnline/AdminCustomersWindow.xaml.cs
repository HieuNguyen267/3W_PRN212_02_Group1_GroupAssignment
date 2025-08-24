using BLL.Services;
using DAL.Entities;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ShoppingOnline
{
    public partial class AdminCustomersWindow : Window, INotifyPropertyChanged
    {
        private readonly IAdminService _adminService;
        private ObservableCollection<Customer> _customers = new();
        private List<Customer> _allCustomers = new();

        public AdminCustomersWindow()
        {
            InitializeComponent();
            _adminService = new AdminService();
            DataContext = this;
            
            LoadCustomers();
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
                _allCustomers = _adminService.GetAllCustomers();
                ApplyCustomerFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi t?i d? li?u kh�ch h�ng: {ex.Message}", "L?i", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyCustomerFilter()
        {
            try
            {
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
                }

                // Update customers collection
                Customers.Clear();
                foreach (var customer in filteredCustomers.OrderBy(c => c.FullName))
                {
                    Customers.Add(customer);
                }

                // Update DataGrid
                CustomersDataGrid.ItemsSource = Customers;
                
                // Update count
                CustomerCountText.Text = $"T?ng: {filteredCustomers.Count()} kh�ch h�ng";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi l?c kh�ch h�ng: {ex.Message}", "L?i", 
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

        private void RefreshCustomers_Click(object sender, RoutedEventArgs e)
        {
            LoadCustomers();
            MessageBox.Show("?� l�m m?i danh s�ch kh�ch h�ng!", "Th�ng b�o", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddCustomer_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("T�nh n?ng th�m kh�ch h�ng s? ???c ph�t tri?n sau!", "Th�ng b�o", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ViewCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int customerId)
            {
                try
                {
                    var customer = _adminService.GetCustomerById(customerId);
                    if (customer != null)
                    {
                        var details = $"Kh�ch h�ng #{customerId}\n" +
                                     $"T�n: {customer.FullName}\n" +
                                     $"Email: {customer.Account?.Email}\n" +
                                     $"S?T: {customer.Phone}\n" +
                                     $"??a ch?: {customer.Address}\n" +
                                     $"Ng�y t?o: {customer.CreatedDate:dd/MM/yyyy}";
                        
                        MessageBox.Show(details, "Th�ng tin kh�ch h�ng", 
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
                MessageBox.Show($"T�nh n?ng s?a th�ng tin kh�ch h�ng #{customerId} s? ???c ph�t tri?n sau!", "Th�ng b�o", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int customerId)
            {
                var result = MessageBox.Show($"B?n c� ch?c mu?n x�a kh�ch h�ng #{customerId}?", 
                    "X�c nh?n x�a", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    MessageBox.Show("T�nh n?ng x�a kh�ch h�ng s? ???c ph�t tri?n sau!", "Th�ng b�o", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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