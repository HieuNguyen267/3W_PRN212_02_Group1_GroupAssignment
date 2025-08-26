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
            MessageBox.Show("?ã làm m?i danh sách admin!", "Thông báo", 
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
                MessageBox.Show($"L?i khi m? c?a s? thêm admin:\n{ex.Message}", "L?i", 
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
                        MessageBox.Show($"Không tìm th?y admin #{adminId}!\n\nAdmin có th? ?ã b? xóa ho?c không t?n t?i.", 
                            "L?i", MessageBoxButton.OK, MessageBoxImage.Warning);
                        LoadAdmins(); // Refresh the list
                        return;
                    }

                    var info = $"Thông tin admin #{adminId}\n\n" +
                              $"Tên: {admin.FullName}\n" +
                              $"Email: {admin.Account?.Email}\n" +
                              $"?i?n tho?i: {admin.Phone}\n" +
                              $"Ngày t?o: {admin.CreatedDate:dd/MM/yyyy}\n" +
                              $"Tr?ng thái: {(admin.Account?.IsActive == true ? "Ho?t ??ng" : "Khóa")}";
                    
                    MessageBox.Show(info, "Thông tin admin", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"L?i khi xem thông tin admin:\n{ex.Message}", "L?i", 
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
                        MessageBox.Show($"Không tìm th?y admin #{adminId}!\n\nAdmin có th? ?ã b? xóa ho?c không t?n t?i.", 
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
                    MessageBox.Show($"L?i khi m? c?a s? s?a thông tin admin:\n{ex.Message}", "L?i", 
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
                        MessageBox.Show($"Không tìm th?y admin #{adminId}!\n\nAdmin có th? ?ã b? xóa ho?c không t?n t?i.", 
                            "L?i", MessageBoxButton.OK, MessageBoxImage.Warning);
                        LoadAdmins(); // Refresh the list
                        return;
                    }

                    // Enhanced confirmation message
                    var message = $"?? C?NH BÁO: VÔ HI?U HÓA TÀI KHO?N ADMIN ??\n\n" +
                                 $"B?n có ch?c mu?n vô hi?u hóa admin #{adminId}?\n\n" +
                                 $"• Tên: {admin.FullName}\n" +
                                 $"• Email: {admin.Account?.Email}\n\n" +
                                 $"THAO TÁC NÀY S?:\n" +
                                 $"? VÔ HI?U HÓA tài kho?n admin\n" +
                                 $"? Admin không th? ??ng nh?p\n" +
                                 $"? Có th? khôi ph?c sau này\n\n" +
                                 $"L?U Ý: Vi?c này s? ?nh h??ng ??n quy?n qu?n tr?!";
                    
                    var result = MessageBox.Show(message, "?? VÔ HI?U HÓA ADMIN", 
                        MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Debug.WriteLine($"AdminAdminsView: User confirmed deletion of admin {adminId}");
                        
                        bool deleteSuccess = _adminService.DeleteAdmin(adminId);
                        System.Diagnostics.Debug.WriteLine($"AdminAdminsView: Delete operation result: {deleteSuccess}");
                        
                        if (deleteSuccess)
                        {
                            LoadAdmins();
                            MessageBox.Show("?ã vô hi?u hóa tài kho?n admin thành công!", "Thành công", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Không th? vô hi?u hóa admin!\n\n" +
                                           "Có th? do:\n" +
                                           "• Admin không t?n t?i\n" +
                                           "• L?i c? s? d? li?u\n" +
                                           "• Ràng bu?c d? li?u\n" +
                                           "• Admin này là tài kho?n duy nh?t", 
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
                    MessageBox.Show($"L?i khi xóa admin:\n{ex.Message}\n\n" +
                                   "Vui lòng th? l?i ho?c liên h? qu?n tr? viên.", 
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
                        MessageBox.Show($"Không tìm th?y tài kho?n admin #{adminId}!", "L?i", 
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        LoadAdmins(); // Refresh the list
                        return;
                    }

                    bool newStatus = !admin.Account.IsActive.GetValueOrDefault();
                    string statusText = newStatus ? "kích ho?t" : "vô hi?u hóa";
                    
                    var message = $"B?n có mu?n {statusText} tài kho?n admin này?\n\n" +
                                 $"• Tên: {admin.FullName}\n" +
                                 $"• Email: {admin.Account.Email}\n" +
                                 $"• Tr?ng thái hi?n t?i: {(admin.Account.IsActive == true ? "Ho?t ??ng" : "Khóa")}\n\n" +
                                 $"Thao tác này s? {statusText} tài kho?n admin.";
                    
                    var result = MessageBox.Show(message, $"Xác nh?n {statusText}", 
                        MessageBoxButton.YesNo, MessageBoxImage.Question);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Debug.WriteLine($"AdminAdminsView: User confirmed status change for admin {adminId} to {newStatus}");
                        
                        bool updateSuccess = _adminService.UpdateAccountStatus(admin.Account.AccountId, newStatus);
                        System.Diagnostics.Debug.WriteLine($"AdminAdminsView: Status update result: {updateSuccess}");
                        
                        if (updateSuccess)
                        {
                            LoadAdmins();
                            MessageBox.Show($"?ã {statusText} tài kho?n thành công!", "Thành công", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show($"Không th? {statusText} tài kho?n!\n\n" +
                                           "Có th? do:\n" +
                                           "• Tài kho?n không t?n t?i\n" +
                                           "• L?i c? s? d? li?u\n" +
                                           "• Ràng bu?c d? li?u", 
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
                    MessageBox.Show($"L?i khi c?p nh?t tr?ng thái:\n{ex.Message}\n\n" +
                                   "Vui lòng th? l?i ho?c liên h? qu?n tr? viên.", 
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