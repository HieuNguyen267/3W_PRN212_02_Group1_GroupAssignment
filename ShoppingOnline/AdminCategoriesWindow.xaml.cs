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
    public partial class AdminCategoriesWindow : Window, INotifyPropertyChanged
    {
        private readonly IAdminService _adminService;
        private ObservableCollection<Category> _categories = new();
        private List<Category> _allCategories = new();

        public AdminCategoriesWindow()
        {
            InitializeComponent();
            _adminService = new AdminService();
            DataContext = this;
            
            LoadCategories();
        }

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                OnPropertyChanged(nameof(Categories));
            }
        }

        private void LoadCategories()
        {
            try
            {
                _allCategories = _adminService.GetAllCategories();
                ApplyCategoryFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi khi tai du lieu danh muc: {ex.Message}", "Loi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyCategoryFilter()
        {
            try
            {
                var filteredCategories = _allCategories.AsEnumerable();

                // Apply search filter
                var searchText = CategorySearchBox?.Text?.Trim();
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    filteredCategories = filteredCategories.Where(c => 
                        (c.CategoryName?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true) ||
                        (c.Description?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true));
                }

                // Update categories collection
                Categories.Clear();
                foreach (var category in filteredCategories.OrderBy(c => c.CategoryName))
                {
                    Categories.Add(category);
                }

                // Update DataGrid
                CategoriesDataGrid.ItemsSource = Categories;
                
                // Update count
                CategoryCountText.Text = $"Tong: {filteredCategories.Count()} danh muc";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi khi loc danh muc: {ex.Message}", "Loi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Event Handlers
        private void CategorySearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CategorySearchBox != null)
            {
                CategorySearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(CategorySearchBox.Text) 
                    ? Visibility.Visible : Visibility.Hidden;
                
                ApplyCategoryFilter();
            }
        }

        private void RefreshCategories_Click(object sender, RoutedEventArgs e)
        {
            LoadCategories();
            MessageBox.Show("Da lam moi danh sach danh muc!", "Thong bao", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tinh nang them danh muc se duoc phat trien sau!", "Thong bao", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void EditCategory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string categoryId)
            {
                MessageBox.Show($"Tinh nang sua danh muc {categoryId} se duoc phat trien sau!", "Thong bao", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string categoryId)
            {
                var result = MessageBox.Show($"Ban co chac muon xoa danh muc {categoryId}?", 
                    "Xac nhan xoa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (_adminService.DeleteCategory(categoryId))
                        {
                            MessageBox.Show("Da xoa danh muc thanh cong!", "Thanh cong", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadCategories(); // Refresh
                        }
                        else
                        {
                            MessageBox.Show("Loi khi xoa danh muc!", "Loi", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Loi: {ex.Message}", "Loi", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BackToDashboard_Click(object sender, RoutedEventArgs e)
        {
            // Not needed anymore in single-window navigation
            MessageBox.Show("Navigation ?ã ???c c?p nh?t! Vui lòng s? d?ng menu bên trái ?? chuy?n trang.", "Thông báo", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // Not needed anymore in single-window navigation
            AdminSession.EndNavigation();
            base.OnClosing(e);
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