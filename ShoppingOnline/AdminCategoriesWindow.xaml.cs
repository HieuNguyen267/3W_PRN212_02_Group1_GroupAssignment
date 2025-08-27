using BLL.Services;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

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

            Loaded += AdminCategoriesWindow_Loaded;
        }

        private void AdminCategoriesWindow_Loaded(object sender, RoutedEventArgs e)
        {
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
                MessageBox.Show($"Lỗi khi tải danh mục: {ex.Message}", "Lỗi",
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
                        (c.CategoryName?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true));
                }

                // Update collection
                Categories.Clear();
                foreach (var cat in filteredCategories.OrderBy(c => c.CategoryName))
                {
                    Categories.Add(cat);
                }

                // Bind DataGrid
                if (CategoriesDataGrid != null)
                {
                    CategoriesDataGrid.ItemsSource = Categories;
                }

                // Count
                if (CategoryCountText != null)
                {
                    CategoryCountText.Text = $"Tổng: {filteredCategories.Count()} danh mục";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lọc danh mục: {ex.Message}", "Lỗi",
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
            MessageBox.Show("Đã làm mới danh sách danh mục!", "Thông báo",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tính năng thêm danh mục sẽ được phát triển sau!", "Thông báo",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void EditCategory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag?.ToString(), out int categoryId))
            {
                MessageBox.Show($"Tính năng sửa danh mục {categoryId} sẽ được phát triển sau!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string categoryId)
            {
                var result = MessageBox.Show($"Bạn có chắc muốn xóa danh mục {categoryId}?",
                    "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    MessageBox.Show("Tính năng xóa danh mục sẽ được phát triển sau!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        private void ViewCategory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string categoryId)
            {
                var category = _allCategories.FirstOrDefault(c => c.CategoryId == categoryId);
                if (category != null)
                {
                    MessageBox.Show(
                        $"ID: {category.CategoryId}\nTên: {category.CategoryName}\nMô tả: {category.Description}",
                        "Thông tin danh mục",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
            }
        }


        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BackToDashboard_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Navigation đã được cập nhật! Vui lòng dùng menu bên trái để chuyển trang.", "Thông báo",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
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
