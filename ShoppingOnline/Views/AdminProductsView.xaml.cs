using BLL.Services;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ShoppingOnline.Views
{
    public partial class AdminProductsView : UserControl, INotifyPropertyChanged
    {
        private readonly IAdminService _adminService;
        private ObservableCollection<Product> _products = new();
        private List<Product> _allProducts = new();

        public AdminProductsView()
        {
            InitializeComponent();
            _adminService = new AdminService();
            DataContext = this;
            
            Loaded += AdminProductsView_Loaded;
        }

        private void AdminProductsView_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCategories();
            LoadProducts();
        }

        public ObservableCollection<Product> Products
        {
            get => _products;
            set
            {
                _products = value;
                OnPropertyChanged(nameof(Products));
            }
        }

        private void LoadCategories()
        {
            try
            {
                var categories = _adminService.GetAllCategories();
                if (CategoryFilterComboBox != null)
                {
                    CategoryFilterComboBox.Items.Clear();
                    CategoryFilterComboBox.Items.Add(new ComboBoxItem { Content = "Tat ca danh muc", IsSelected = true });
                    
                    foreach (var category in categories)
                    {
                        CategoryFilterComboBox.Items.Add(new ComboBoxItem { Content = category.CategoryName, Tag = category.CategoryId });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh mục: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadProducts()
        {
            try
            {
                _allProducts = _adminService.GetAllProducts();
                ApplyProductFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu sản phẩm: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyProductFilter()
        {
            try
            {
                var filteredProducts = _allProducts.AsEnumerable();

                // Apply search filter
                var searchText = ProductSearchBox?.Text?.Trim();
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    filteredProducts = filteredProducts.Where(p => 
                        (p.ProductName?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true) ||
                        (p.Category?.CategoryName?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true) ||
                        (p.Description?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true));
                }

                // Apply category filter
                var selectedCategory = CategoryFilterComboBox?.SelectedItem as ComboBoxItem;
                var categoryText = selectedCategory?.Content?.ToString();
                if (!string.IsNullOrEmpty(categoryText) && categoryText != "Tat ca danh muc")
                {
                    filteredProducts = filteredProducts.Where(p => p.Category?.CategoryName == categoryText);
                }

                // Update products collection
                Products.Clear();
                foreach (var product in filteredProducts.OrderBy(p => p.ProductName))
                {
                    Products.Add(product);
                }

                // Update DataGrid
                if (ProductsDataGrid != null)
                {
                    ProductsDataGrid.ItemsSource = Products;
                }
                
                // Update count
                if (ProductCountText != null)
                {
                    ProductCountText.Text = $"Tổng: {filteredProducts.Count()} sản phẩm";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lọc sản phẩm: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Event Handlers
        private void ProductSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ProductSearchBox != null)
            {
                ProductSearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(ProductSearchBox.Text) 
                    ? Visibility.Visible : Visibility.Hidden;
                
                ApplyProductFilter();
            }
        }

        private void CategoryFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyProductFilter();
        }

        private void RefreshProducts_Click(object sender, RoutedEventArgs e)
        {
            LoadProducts();
            MessageBox.Show("Đã làm mới danh sách sản phẩm!", "Thông báo", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tính năng thêm sản phẩm sẽ được phát triển sau!", "Thông báo", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ViewProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string productId)
            {
                try
                {
                    var detailWindow = new DetailWindow(productId);
                    detailWindow.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xem sản phẩm: {ex.Message}", "Lỗi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string productId)
            {
                MessageBox.Show($"Tính năng sửa sản phẩm {productId} sẽ được phát triển sau!", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string productId)
            {
                var result = MessageBox.Show($"Bạn có chắc muốn xóa sản phẩm {productId}?", 
                    "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    MessageBox.Show("Tính năng xóa sản phẩm sẽ được phát triển sau!", "Thông báo", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
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