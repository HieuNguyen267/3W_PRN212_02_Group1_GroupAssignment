using System.Windows;
using BLL.Services;
using DAL.Entities;

namespace ShoppingOnline
{
    public partial class AdminCategoriesEditWindow : Window
    {
        private readonly IAdminService _adminService;
        private readonly Category _editingCategory;
        private readonly bool _isEditMode;

        // constructor thêm mới
        public AdminCategoriesEditWindow()
        {
            InitializeComponent();
            _adminService = new AdminService();
            _isEditMode = false;
            HeaderTitle.Text = "Thêm danh mục mới";
        }

        // constructor sửa
        public AdminCategoriesEditWindow(Category category)
        {
            InitializeComponent();
            _adminService = new AdminService();
            _editingCategory = category;
            _isEditMode = true;
            HeaderTitle.Text = "Cập nhật danh mục";

            // fill dữ liệu lên form
            CategoryNameTextBox.Text = category.CategoryName;
            DescriptionTextBox.Text = category.Description;
            StatusGrid.Visibility = Visibility.Visible;
            IsActiveCheckBox.IsChecked = category.IsActive;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Clear previous errors
                CategoryNameError.Visibility = Visibility.Collapsed;
                DescriptionError.Visibility = Visibility.Collapsed;

                // Validate input
                var categoryName = CategoryNameTextBox.Text?.Trim();
                var description = DescriptionTextBox.Text?.Trim();

                bool isValid = true;

                if (string.IsNullOrWhiteSpace(categoryName))
                {
                    CategoryNameError.Text = "Tên danh mục không được để trống!";
                    CategoryNameError.Visibility = Visibility.Visible;
                    CategoryNameTextBox.Focus();
                    isValid = false;
                }
                else if (categoryName.Length < 2 || categoryName.Length > 100)
                {
                    CategoryNameError.Text = "Tên danh mục phải từ 2-100 ký tự!";
                    CategoryNameError.Visibility = Visibility.Visible;
                    CategoryNameTextBox.Focus();
                    isValid = false;
                }

                if (description != null && description.Length > 500)
                {
                    DescriptionError.Text = "Mô tả không được quá 500 ký tự!";
                    DescriptionError.Visibility = Visibility.Visible;
                    DescriptionTextBox.Focus();
                    isValid = false;
                }

                if (!isValid)
                {
                    return;
                }

                if (_isEditMode)
                {
                    // Update existing category
                    _editingCategory.CategoryName = categoryName;
                    _editingCategory.Description = description;
                    _editingCategory.IsActive = IsActiveCheckBox.IsChecked ?? false;

                    _adminService.UpdateCategory(_editingCategory);
                }
                else
                {
                    // Create new category
                    var newCategory = new Category
                    {
                        CategoryId = _adminService.GenerateNewCategoryId(),
                        CategoryName = categoryName,
                        Description = description,
                        IsActive = true
                    };

                    _adminService.AddCategory(newCategory);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu danh mục: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
