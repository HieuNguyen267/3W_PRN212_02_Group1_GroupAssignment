using DAL.Repositories;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ShoppingOnline.Views
{
    public partial class AdminCategoriesView : UserControl
    {
        private readonly ICategoryRepository _categoryRepository;

        public AdminCategoriesView()
        {
            InitializeComponent();
            _categoryRepository = new CategoryRepository();
            LoadCategories();
        }

        // Load danh sách
        private void LoadCategories()
        {
            try
            {
                var categories = _categoryRepository.GetAll().ToList();
                CategoriesDataGrid.ItemsSource = categories;
                CategoryCountText.Text = $"Tong: {categories.Count} danh muc";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi khi load danh muc: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Search
        private void CategorySearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = CategorySearchBox.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                LoadCategories();
                CategorySearchPlaceholder.Visibility = Visibility.Visible;
            }
            else
            {
                CategorySearchPlaceholder.Visibility = string.IsNullOrEmpty(CategorySearchBox.Text) ? Visibility.Visible : Visibility.Collapsed;
                var categories = _categoryRepository.GetAll()
                    .Where(c => c.CategoryName.ToLower().Contains(keyword)
                             || (!string.IsNullOrEmpty(c.Description) && c.Description.ToLower().Contains(keyword)))
                    .ToList();

                CategoriesDataGrid.ItemsSource = categories;
                CategoryCountText.Text = $"Tong: {categories.Count} danh muc";
            }
        }

        // Refresh
        private void RefreshCategories_Click(object sender, RoutedEventArgs e)
        {
            CategorySearchBox.Clear();
            LoadCategories();
        }

        // Add
        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            var window = new AdminCategoriesEditWindow();
            if (window.ShowDialog() == true)
            {
                LoadCategories();
            }
        }

        // Edit
        private void EditCategory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string categoryId)
            {
                var category = _categoryRepository.GetById(categoryId);
                if (category != null)
                {
                    var window = new AdminCategoriesEditWindow(category);
                    if (window.ShowDialog() == true)
                    {
                        LoadCategories();
                    }
                }
            }
        }

        // Delete
        private void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string categoryId)
            {
                var category = _categoryRepository.GetById(categoryId);
                if (category != null)
                {
                    var result = MessageBox.Show($"Ban co chac muon xoa danh muc '{category.CategoryName}'?",
                        "Xac nhan xoa", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        _categoryRepository.Delete(categoryId);
                        LoadCategories();
                    }
                }
            }
        }

        // View
        private void ViewCategory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string categoryId)
            {
                var category = _categoryRepository.GetById(categoryId);
                if (category != null)
                {
                    MessageBox.Show($"ID: {category.CategoryId}\nTen: {category.CategoryName}\nMo ta: {category.Description}",
                        "Thong tin danh muc", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }
}
