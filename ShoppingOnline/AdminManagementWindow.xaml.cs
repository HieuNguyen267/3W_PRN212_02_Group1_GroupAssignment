using BLL.Services;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ShoppingOnline
{
    public partial class AdminManagementWindow : Window, INotifyPropertyChanged
    {
        private readonly IAdminService _adminService;
        private ObservableCollection<Admin> _admins = new();
        private List<Admin> _allAdmins = new();

        public AdminManagementWindow()
        {
            InitializeComponent();
            _adminService = new AdminService();
            DataContext = this;
            
            LoadAdmins();
        }

        public ObservableCollection<Admin> Admins
        {
            get => _admins;
            set
            {
                _admins = value;
                OnPropertyChanged(nameof(Admins));
            }
        }

        private void LoadAdmins()
        {
            try
            {
                _allAdmins = _adminService.GetAllAdmins();
                ApplyAdminFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi khi tai danh sach admin: {ex.Message}", "Loi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyAdminFilter()
        {
            try
            {
                var filteredAdmins = _allAdmins.AsEnumerable();

                // Apply search filter
                var searchText = AdminSearchBox?.Text?.Trim();
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    filteredAdmins = filteredAdmins.Where(a => 
                        (a.FullName?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true) ||
                        (a.Account?.Email?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true) ||
                        (a.Phone?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true));
                }

                // Update admins collection
                Admins.Clear();
                foreach (var admin in filteredAdmins.OrderBy(a => a.FullName))
                {
                    Admins.Add(admin);
                }

                // Update DataGrid
                AdminsDataGrid.ItemsSource = Admins;
                
                // Update count
                AdminCountText.Text = $"Tong: {filteredAdmins.Count()} admin";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi khi loc admin: {ex.Message}", "Loi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Event Handlers
        private void AdminSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AdminSearchBox != null)
            {
                AdminSearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(AdminSearchBox.Text) 
                    ? Visibility.Visible : Visibility.Hidden;
                
                ApplyAdminFilter();
            }
        }

        private void RefreshAdmins_Click(object sender, RoutedEventArgs e)
        {
            LoadAdmins();
            MessageBox.Show("Da lam moi danh sach admin!", "Thong bao", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddAdmin_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tinh nang them admin se duoc phat trien sau!", "Thong bao", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void EditAdmin_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int adminId)
            {
                MessageBox.Show($"Tinh nang sua admin {adminId} se duoc phat trien sau!", "Thong bao", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ToggleAdmin_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int adminId)
            {
                var admin = _allAdmins.FirstOrDefault(a => a.AdminId == adminId);
                if (admin?.Account != null)
                {
                    var action = admin.Account.IsActive == true ? "khoa" : "mo khoa";
                    var result = MessageBox.Show($"Ban co chac muon {action} tai khoan admin {admin.FullName}?", 
                        "Xac nhan", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        MessageBox.Show($"Tinh nang {action} admin se duoc phat trien sau!", "Thong bao", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
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