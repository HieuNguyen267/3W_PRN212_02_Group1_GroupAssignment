using System.Windows;
using BLL.Services;
using DAL.Entities;
using DAL.Repositories;

namespace ShoppingOnline
{
    public partial class AdminCategoriesEditWindow : Window
    {
        private readonly CategoryService _service;
        private readonly Category _editingCategory;
        private readonly bool _isEditMode;

        // constructor thêm mới
        public AdminCategoriesEditWindow()
        {
            InitializeComponent();
            _service = new CategoryService(new CategoryRepo());
            _isEditMode = false;
            HeaderTitle.Text = "Thêm danh mục mới";
        }

        // constructor sửa
        public AdminCategoriesEditWindow(Category category)
        {
            InitializeComponent();
            _service = new CategoryService(new CategoryRepo());
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
            if (string.IsNullOrWhiteSpace(CategoryNameTextBox.Text))
            {
                MessageBox.Show("Tên danh mục không được để trống!");
                return;
            }

            if (_isEditMode)
            {
                // update
                _editingCategory.CategoryName = CategoryNameTextBox.Text;
                _editingCategory.Description = DescriptionTextBox.Text;
                _editingCategory.IsActive = IsActiveCheckBox.IsChecked ?? false;

                _service.UpdateCategory(_editingCategory);
                MessageBox.Show("Cập nhật danh mục thành công!");
            }
            else
            {
                var newCategory = new Category
                {
                    CategoryId = _service.GenerateNewCategoryId(),
                    CategoryName = CategoryNameTextBox.Text,
                    Description = DescriptionTextBox.Text,
                    IsActive = true
                };

                _service.AddCategory(newCategory);
                MessageBox.Show("Thêm danh mục thành công!");
            }


            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
