using BLL.Services;
using DAL.Entities;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ShoppingOnline.Views
{
    public partial class AdminCategoriesView : UserControl
    {
        private readonly IAdminService _adminService;

        public AdminCategoriesView()
        {
            InitializeComponent();
            _adminService = new AdminService();
            LoadCategories();
        }

        // Load danh sách
        private void LoadCategories()
        {
            try
            {
                // Get all categories including inactive ones for admin view
                var categories = _adminService.GetAllCategoriesForAdmin();
                ApplyCategoryFilter(categories);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh mục: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyCategoryFilter(List<Category> allCategories)
        {
            try
            {
                var filteredCategories = allCategories.AsEnumerable();

                // Apply search filter
                var searchText = CategorySearchBox?.Text?.Trim();
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    filteredCategories = filteredCategories.Where(c => 
                        c.CategoryName.ToLower().Contains(searchText.ToLower()) ||
                        (!string.IsNullOrEmpty(c.Description) && c.Description.ToLower().Contains(searchText.ToLower())));
                }

                // Apply active/inactive filter
                if (ShowInactiveCheckBox?.IsChecked != true)
                {
                    // Only show active categories by default
                    filteredCategories = filteredCategories.Where(c => c.IsActive == true);
                }

                var result = filteredCategories.ToList();
                CategoriesDataGrid.ItemsSource = result;
                CategoryCountText.Text = $"Tổng: {result.Count} danh mục";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lọc danh mục: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Search
        private void CategorySearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CategorySearchBox != null)
            {
                CategorySearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(CategorySearchBox.Text) 
                    ? Visibility.Visible : Visibility.Hidden;
                
                var categories = _adminService.GetAllCategoriesForAdmin();
                ApplyCategoryFilter(categories);
            }
        }

        // Refresh
        private void RefreshCategories_Click(object sender, RoutedEventArgs e)
        {
            CategorySearchBox.Clear();
            LoadCategories();
            MessageBox.Show("Đã làm mới danh sách danh mục!", "Thông báo", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowInactiveCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            var categories = _adminService.GetAllCategoriesForAdmin();
            ApplyCategoryFilter(categories);
        }

        // Add
        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var window = new AdminCategoriesEditWindow();
                window.Owner = Window.GetWindow(this);
                if (window.ShowDialog() == true)
                {
                    LoadCategories();
                    MessageBox.Show("Đã thêm danh mục thành công!", "Thành công", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Notify that admin operations are completed
                    AdminSession.NotifyOperationsCompleted();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm danh mục: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Edit
        private void EditCategory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string categoryId)
            {
                try
                {
                    var category = _adminService.GetCategoryById(categoryId);
                    if (category != null)
                    {
                        var window = new AdminCategoriesEditWindow(category);
                        window.Owner = Window.GetWindow(this);
                        if (window.ShowDialog() == true)
                        {
                            LoadCategories();
                            MessageBox.Show("Đã cập nhật danh mục thành công!", "Thành công", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            
                            // Notify that admin operations are completed
                            AdminSession.NotifyOperationsCompleted();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy danh mục!", "Lỗi", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi sửa danh mục: {ex.Message}", "Lỗi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Soft Delete (Disable)
        private void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string categoryId)
            {
                try
                {
                    var category = _adminService.GetCategoryById(categoryId);
                    if (category != null)
                    {
                        var result = MessageBox.Show($"Bạn có chắc muốn vô hiệu hóa danh mục '{category.CategoryName}'?\n\nLưu ý: Việc vô hiệu hóa danh mục sẽ ẩn danh mục này khỏi giao diện người dùng.",
                            "Xác nhận vô hiệu hóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                        if (result == MessageBoxResult.Yes)
                        {
                            try
                            {
                                _adminService.DeleteCategory(categoryId);
                                LoadCategories();
                                MessageBox.Show("Đã vô hiệu hóa danh mục thành công!", "Thành công", 
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                                
                                // Notify that admin operations are completed
                                AdminSession.NotifyOperationsCompleted();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Lỗi khi vô hiệu hóa danh mục: {ex.Message}", "Lỗi", 
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy danh mục!", "Lỗi", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi vô hiệu hóa danh mục: {ex.Message}", "Lỗi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // View
        private void ViewCategory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string categoryId)
            {
                try
                {
                    var category = _adminService.GetCategoryById(categoryId);
                    if (category != null)
                    {
                        MessageBox.Show($"ID: {category.CategoryId}\nTên: {category.CategoryName}\nMô tả: {category.Description}\nTrạng thái: {(category.IsActive == true ? "Hoạt động" : "Vô hiệu hóa")}",
                            "Thông tin danh mục", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xem thông tin danh mục: {ex.Message}", "Lỗi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
