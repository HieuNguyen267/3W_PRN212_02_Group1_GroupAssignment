using BLL.Services;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ShoppingOnline.Views
{
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
                MessageBox.Show($"L?i khi t?i d? li?u khách hàng: {ex.Message}", "L?i", 
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

                // Apply status filter
                var selectedStatus = (StatusFilterComboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString();
                if (!string.IsNullOrEmpty(selectedStatus) && selectedStatus != "Tat ca trang thai")
                {
                    bool isActive = selectedStatus == "Hoat dong";
                    filteredCustomers = filteredCustomers.Where(c => c.Account?.IsActive == isActive);
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
                }
                
                // Update count
                if (CustomerCountText != null)
                {
                    CustomerCountText.Text = $"Tong: {filteredCustomers.Count()} khach hang";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi l?c khách hàng: {ex.Message}", "L?i", 
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
                MessageBox.Show($"Tính n?ng s?a thông tin khách hàng #{customerId} s? ???c phát tri?n sau!", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int customerId)
            {
                var result = MessageBox.Show($"B?n có ch?c mu?n xóa khách hàng #{customerId}?", 
                    "Xác nh?n xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    MessageBox.Show("Tính n?ng xóa khách hàng s? ???c phát tri?n sau!", "Thông báo", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
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