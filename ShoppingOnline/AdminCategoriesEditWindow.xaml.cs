using System.Windows;
using DAL.Entities;

namespace ShoppingOnline
{
    public partial class AdminCategoriesEditWindow : Window
    {
        public Category Category { get; private set; }
        private bool _isEditMode;

        // Add mode
        public AdminCategoriesEditWindow()
        {
            InitializeComponent();
            _isEditMode = false;
            Category = new Category();
            HeaderTitle.Text = "Them danh muc moi";
            StatusGrid.Visibility = Visibility.Collapsed;
        }

        // Edit mode
        public AdminCategoriesEditWindow(Category category) : this()
        {
            if (category != null)
            {
                _isEditMode = true;
                Category = new Category
                {
                    CategoryId = category.CategoryId,
                    CategoryName = category.CategoryName,
                    Description = category.Description,
                    IsActive = category.IsActive
                };

                HeaderTitle.Text = "Cap nhat danh muc";
                CategoryNameTextBox.Text = Category.CategoryName;
                DescriptionTextBox.Text = Category.Description;
                IsActiveCheckBox.IsChecked = Category.IsActive;
                StatusGrid.Visibility = Visibility.Visible;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CategoryNameTextBox.Text))
            {
                MessageBox.Show("Ten danh muc khong duoc de trong!", "Loi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Category.CategoryName = CategoryNameTextBox.Text.Trim();
            Category.Description = DescriptionTextBox.Text.Trim();
            Category.IsActive = IsActiveCheckBox.IsChecked ?? false;

            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
