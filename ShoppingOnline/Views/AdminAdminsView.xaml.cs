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

namespace ShoppingOnline.Views
{
    public partial class AdminAdminsView : UserControl, INotifyPropertyChanged
    {
        private readonly IAdminService _adminService;
        private ObservableCollection<Admin> _admins = new();
        private List<Admin> _allAdmins = new();

        public AdminAdminsView()
        {
            InitializeComponent();
            _adminService = new AdminService();
            DataContext = this;
            
            Loaded += AdminAdminsView_Loaded;
        }

        private void AdminAdminsView_Loaded(object sender, RoutedEventArgs e)
        {
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
                MessageBox.Show($"Error loading admin data: {ex.Message}", "Error", 
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
                    var searchLower = searchText.ToLower();
                    filteredAdmins = filteredAdmins.Where(a => 
                        (a.FullName?.ToLower().Contains(searchLower) == true) ||
                        (a.Account?.Email?.ToLower().Contains(searchLower) == true) ||
                        (a.Phone?.ToLower().Contains(searchLower) == true));
                }

                // Apply status filter
                var selectedStatus = (StatusFilterComboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString();
                if (!string.IsNullOrEmpty(selectedStatus) && selectedStatus != "All Status")
                {
                    bool isActive = selectedStatus == "Active";
                    filteredAdmins = filteredAdmins.Where(a => a.Account?.IsActive == isActive);
                }

                // Update admins collection
                Admins.Clear();
                foreach (var admin in filteredAdmins.OrderBy(a => a.FullName))
                {
                    Admins.Add(admin);
                }

                // Update DataGrid
                if (AdminsDataGrid != null)
                {
                    AdminsDataGrid.ItemsSource = Admins;
                }
                
                // Update count
                if (AdminCountText != null)
                {
                    AdminCountText.Text = $"Total: {filteredAdmins.Count()} admins";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error filtering admins: {ex.Message}", "Error", 
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

        private void StatusFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyAdminFilter();
        }

        private void RefreshAdmins_Click(object sender, RoutedEventArgs e)
        {
            LoadAdmins();
            MessageBox.Show("?� l�m m?i danh s�ch admin!", "Th�ng b�o", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddAdmin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("AdminAdminsView: Opening Add Admin window");
                var addAdminWindow = new AdminEditWindow();
                var result = addAdminWindow.ShowDialog();
                
                System.Diagnostics.Debug.WriteLine($"AdminAdminsView: Add Admin window result: {result}");
                
                if (result == true)
                {
                    System.Diagnostics.Debug.WriteLine("AdminAdminsView: Admin added successfully, refreshing list");
                    LoadAdmins();
                    // Don't show additional success message here as AdminEditWindow should show one
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AdminAdminsView: Error in AddAdmin_Click: {ex.Message}");
                MessageBox.Show($"L?i khi m? c?a s? th�m admin:\n{ex.Message}", "L?i", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewAdmin_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int adminId)
            {
                try
                {
                    var admin = _allAdmins.FirstOrDefault(a => a.AdminId == adminId);
                    if (admin == null)
                    {
                        MessageBox.Show($"Kh�ng t�m th?y admin #{adminId}!\n\nAdmin c� th? ?� b? x�a ho?c kh�ng t?n t?i.", 
                            "L?i", MessageBoxButton.OK, MessageBoxImage.Warning);
                        LoadAdmins(); // Refresh the list
                        return;
                    }

                    var info = $"Th�ng tin admin #{adminId}\n\n" +
                              $"T�n: {admin.FullName}\n" +
                              $"Email: {admin.Account?.Email}\n" +
                              $"?i?n tho?i: {admin.Phone}\n" +
                              $"Ng�y t?o: {admin.CreatedDate:dd/MM/yyyy}\n" +
                              $"Tr?ng th�i: {(admin.Account?.IsActive == true ? "Ho?t ??ng" : "Kh�a")}";
                    
                    MessageBox.Show(info, "Th�ng tin admin", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"L?i khi xem th�ng tin admin:\n{ex.Message}", "L?i", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditAdmin_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int adminId)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"AdminAdminsView: Editing admin {adminId}");
                    var admin = _allAdmins.FirstOrDefault(a => a.AdminId == adminId);
                    if (admin == null)
                    {
                        MessageBox.Show($"Kh�ng t�m th?y admin #{adminId}!\n\nAdmin c� th? ?� b? x�a ho?c kh�ng t?n t?i.", 
                            "L?i", MessageBoxButton.OK, MessageBoxImage.Warning);
                        LoadAdmins(); // Refresh the list
                        return;
                    }

                    var editAdminWindow = new AdminEditWindow(admin);
                    var result = editAdminWindow.ShowDialog();
                    
                    System.Diagnostics.Debug.WriteLine($"AdminAdminsView: Edit Admin window result: {result}");
                    
                    if (result == true)
                    {
                        System.Diagnostics.Debug.WriteLine("AdminAdminsView: Admin updated successfully, refreshing list");
                        LoadAdmins();
                        // Don't show additional success message here as AdminEditWindow should show one
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"AdminAdminsView: Error in EditAdmin_Click: {ex.Message}");
                    MessageBox.Show($"L?i khi m? c?a s? s?a th�ng tin admin:\n{ex.Message}", "L?i", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteAdmin_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int adminId)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"AdminAdminsView: Attempting to delete admin {adminId}");
                    
                    var admin = _allAdmins.FirstOrDefault(a => a.AdminId == adminId);
                    if (admin == null)
                    {
                        MessageBox.Show($"Kh�ng t�m th?y admin #{adminId}!\n\nAdmin c� th? ?� b? x�a ho?c kh�ng t?n t?i.", 
                            "L?i", MessageBoxButton.OK, MessageBoxImage.Warning);
                        LoadAdmins(); // Refresh the list
                        return;
                    }

                    // Enhanced confirmation message
                    var message = $"?? C?NH B�O: V� HI?U H�A T�I KHO?N ADMIN ??\n\n" +
                                 $"B?n c� ch?c mu?n v� hi?u h�a admin #{adminId}?\n\n" +
                                 $"� T�n: {admin.FullName}\n" +
                                 $"� Email: {admin.Account?.Email}\n\n" +
                                 $"THAO T�C N�Y S?:\n" +
                                 $"? V� HI?U H�A t�i kho?n admin\n" +
                                 $"? Admin kh�ng th? ??ng nh?p\n" +
                                 $"? C� th? kh�i ph?c sau n�y\n\n" +
                                 $"L?U �: Vi?c n�y s? ?nh h??ng ??n quy?n qu?n tr?!";
                    
                    var result = MessageBox.Show(message, "?? V� HI?U H�A ADMIN", 
                        MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Debug.WriteLine($"AdminAdminsView: User confirmed deletion of admin {adminId}");
                        
                        bool deleteSuccess = _adminService.DeleteAdmin(adminId);
                        System.Diagnostics.Debug.WriteLine($"AdminAdminsView: Delete operation result: {deleteSuccess}");
                        
                        if (deleteSuccess)
                        {
                            LoadAdmins();
                            MessageBox.Show("?� v� hi?u h�a t�i kho?n admin th�nh c�ng!", "Th�nh c�ng", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Kh�ng th? v� hi?u h�a admin!\n\n" +
                                           "C� th? do:\n" +
                                           "� Admin kh�ng t?n t?i\n" +
                                           "� L?i c? s? d? li?u\n" +
                                           "� R�ng bu?c d? li?u\n" +
                                           "� Admin n�y l� t�i kho?n duy nh?t", 
                                "L?i", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"AdminAdminsView: User cancelled deletion of admin {adminId}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"AdminAdminsView: Error in DeleteAdmin_Click: {ex.Message}");
                    MessageBox.Show($"L?i khi x�a admin:\n{ex.Message}\n\n" +
                                   "Vui l�ng th? l?i ho?c li�n h? qu?n tr? vi�n.", 
                        "L?i h? th?ng", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ToggleAdminStatus_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int adminId)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"AdminAdminsView: Toggling status for admin {adminId}");
                    
                    var admin = _allAdmins.FirstOrDefault(a => a.AdminId == adminId);
                    if (admin?.Account == null)
                    {
                        MessageBox.Show($"Kh�ng t�m th?y t�i kho?n admin #{adminId}!", "L?i", 
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        LoadAdmins(); // Refresh the list
                        return;
                    }

                    bool newStatus = !admin.Account.IsActive.GetValueOrDefault();
                    string statusText = newStatus ? "k�ch ho?t" : "v� hi?u h�a";
                    
                    var message = $"B?n c� mu?n {statusText} t�i kho?n admin n�y?\n\n" +
                                 $"� T�n: {admin.FullName}\n" +
                                 $"� Email: {admin.Account.Email}\n" +
                                 $"� Tr?ng th�i hi?n t?i: {(admin.Account.IsActive == true ? "Ho?t ??ng" : "Kh�a")}\n\n" +
                                 $"Thao t�c n�y s? {statusText} t�i kho?n admin.";
                    
                    var result = MessageBox.Show(message, $"X�c nh?n {statusText}", 
                        MessageBoxButton.YesNo, MessageBoxImage.Question);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Debug.WriteLine($"AdminAdminsView: User confirmed status change for admin {adminId} to {newStatus}");
                        
                        bool updateSuccess = _adminService.UpdateAccountStatus(admin.Account.AccountId, newStatus);
                        System.Diagnostics.Debug.WriteLine($"AdminAdminsView: Status update result: {updateSuccess}");
                        
                        if (updateSuccess)
                        {
                            LoadAdmins();
                            MessageBox.Show($"?� {statusText} t�i kho?n th�nh c�ng!", "Th�nh c�ng", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show($"Kh�ng th? {statusText} t�i kho?n!\n\n" +
                                           "C� th? do:\n" +
                                           "� T�i kho?n kh�ng t?n t?i\n" +
                                           "� L?i c? s? d? li?u\n" +
                                           "� R�ng bu?c d? li?u", 
                                "L?i", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"AdminAdminsView: User cancelled status change for admin {adminId}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"AdminAdminsView: Error in ToggleAdminStatus_Click: {ex.Message}");
                    MessageBox.Show($"L?i khi c?p nh?t tr?ng th�i:\n{ex.Message}\n\n" +
                                   "Vui l�ng th? l?i ho?c li�n h? qu?n tr? vi�n.", 
                        "L?i h? th?ng", MessageBoxButton.OK, MessageBoxImage.Error);
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