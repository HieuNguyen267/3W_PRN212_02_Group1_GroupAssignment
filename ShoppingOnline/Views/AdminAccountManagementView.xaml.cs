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
    public partial class AdminAccountManagementView : UserControl, INotifyPropertyChanged
    {
        private readonly IAdminService _adminService;
        private ObservableCollection<Account> _accounts = new();
        private List<Account> _allAccounts = new();

        public AdminAccountManagementView()
        {
            InitializeComponent();
            _adminService = new AdminService();
            DataContext = this;
            
            Loaded += AdminAccountManagementView_Loaded;
        }

        private void AdminAccountManagementView_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAccounts();
        }

        public ObservableCollection<Account> Accounts
        {
            get => _accounts;
            set
            {
                _accounts = value;
                OnPropertyChanged(nameof(Accounts));
            }
        }

        private void LoadAccounts()
        {
            try
            {
                _allAccounts = _adminService.GetAllAccounts();
                ApplyAccountFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi t?i d? li?u t�i kho?n: {ex.Message}", "L?i", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyAccountFilter()
        {
            try
            {
                var filteredAccounts = _allAccounts.AsEnumerable();

                // Apply search filter
                var searchText = AccountSearchBox?.Text?.Trim();
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    var searchLower = searchText.ToLower();
                    filteredAccounts = filteredAccounts.Where(a => 
                        (a.Username?.ToLower().Contains(searchLower) == true) ||
                        (a.Email?.ToLower().Contains(searchLower) == true));
                }

                // Apply account type filter
                var selectedAccountType = (AccountTypeFilterComboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString();
                if (!string.IsNullOrEmpty(selectedAccountType) && selectedAccountType != "T?t c? lo?i TK")
                {
                    filteredAccounts = filteredAccounts.Where(a => a.AccountType == selectedAccountType);
                }

                // Apply status filter
                var selectedStatus = (StatusFilterComboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString();
                if (!string.IsNullOrEmpty(selectedStatus) && selectedStatus != "T?t c? tr?ng th�i")
                {
                    bool isActive = selectedStatus == "Ho?t ??ng";
                    filteredAccounts = filteredAccounts.Where(a => a.IsActive == isActive);
                }

                // Update accounts collection
                Accounts.Clear();
                foreach (var account in filteredAccounts.OrderByDescending(a => a.CreatedDate))
                {
                    Accounts.Add(account);
                }

                // Update DataGrid
                if (AccountsDataGrid != null)
                {
                    AccountsDataGrid.ItemsSource = Accounts;
                }
                
                // Update count
                if (AccountCountText != null)
                {
                    AccountCountText.Text = $"T?ng: {filteredAccounts.Count()} t�i kho?n";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi l?c t�i kho?n: {ex.Message}", "L?i", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Event Handlers
        private void AccountSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AccountSearchBox != null)
            {
                AccountSearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(AccountSearchBox.Text) 
                    ? Visibility.Visible : Visibility.Hidden;
                
                ApplyAccountFilter();
            }
        }

        private void AccountTypeFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyAccountFilter();
        }

        private void StatusFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyAccountFilter();
        }

        private void RefreshAccounts_Click(object sender, RoutedEventArgs e)
        {
            LoadAccounts();
            MessageBox.Show("?� l�m m?i danh s�ch t�i kho?n!", "Th�ng b�o", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ViewAccount_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int accountId)
            {
                try
                {
                    var account = _allAccounts.FirstOrDefault(a => a.AccountId == accountId);
                    if (account != null)
                    {
                        var info = $"Th�ng tin t�i kho?n #{accountId}\n\n" +
                                  $"T�n ??ng nh?p: {account.Username}\n" +
                                  $"Email: {account.Email}\n" +
                                  $"Lo?i t�i kho?n: {account.AccountType}\n" +
                                  $"Ng�y t?o: {account.CreatedDate:dd/MM/yyyy}\n" +
                                  $"Tr?ng th�i: {(account.IsActive == true ? "Ho?t ??ng" : "Kh�a")}";
                        
                        MessageBox.Show(info, "Th�ng tin t�i kho?n", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"L?i khi xem th�ng tin t�i kho?n: {ex.Message}", "L?i", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditAccount_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int accountId)
            {
                try
                {
                    var account = _allAccounts.FirstOrDefault(a => a.AccountId == accountId);
                    if (account != null)
                    {
                        var editAccountWindow = new AccountEditWindow(account);
                        if (editAccountWindow.ShowDialog() == true)
                        {
                            LoadAccounts();
                            MessageBox.Show("C?p nh?t th�ng tin t�i kho?n th�nh c�ng!", "Th�nh c�ng", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"L?i khi s?a th�ng tin t�i kho?n: {ex.Message}", "L?i", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ToggleAccountStatus_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int accountId)
            {
                try
                {
                    var account = _allAccounts.FirstOrDefault(a => a.AccountId == accountId);
                    if (account != null)
                    {
                        bool newStatus = !account.IsActive.GetValueOrDefault();
                        string statusText = newStatus ? "k�ch ho?t" : "v� hi?u h�a";
                        
                        var result = MessageBox.Show($"B?n c� mu?n {statusText} t�i kho?n n�y?", 
                            "X�c nh?n", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        
                        if (result == MessageBoxResult.Yes)
                        {
                            if (_adminService.UpdateAccountStatus(accountId, newStatus))
                            {
                                LoadAccounts();
                                MessageBox.Show($"?� {statusText} t�i kho?n th�nh c�ng!", "Th�nh c�ng", 
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show("Kh�ng th? c?p nh?t tr?ng th�i t�i kho?n!", "L?i", 
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"L?i khi c?p nh?t tr?ng th�i: {ex.Message}", "L?i", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
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