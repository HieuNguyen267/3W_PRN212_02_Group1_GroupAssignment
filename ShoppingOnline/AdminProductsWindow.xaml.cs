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

namespace ShoppingOnline
{
    public partial class AdminProductsWindow : Window, INotifyPropertyChanged
    {
        private readonly IAdminService _adminService;
        private ObservableCollection<Product> _products = new();
        private List<Product> _allProducts = new();

        public AdminProductsWindow()
        {
            InitializeComponent();
            _adminService = new AdminService();
            DataContext = this;
            
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
                CategoryFilterComboBox.Items.Clear();
                CategoryFilterComboBox.Items.Add(new ComboBoxItem { Content = "Tat ca danh muc", IsSelected = true });
                
                foreach (var category in categories)
                {
                    CategoryFilterComboBox.Items.Add(new ComboBoxItem { Content = category.CategoryName, Tag = category.CategoryId });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi t?i danh m?c: {ex.Message}", "L?i", 
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
                MessageBox.Show($"L?i khi t?i d? li?u s?n ph?m: {ex.Message}", "L?i", 
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
                ProductsDataGrid.ItemsSource = Products;
                
                // Update count
                ProductCountText.Text = $"T?ng: {filteredProducts.Count()} s?n ph?m";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi l?c s?n ph?m: {ex.Message}", "L?i", 
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
            MessageBox.Show("?ã làm m?i danh sách s?n ph?m!", "Thông báo", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tính n?ng thêm s?n ph?m s? ???c phát tri?n sau!", "Thông báo", 
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
                    MessageBox.Show($"L?i khi xem s?n ph?m: {ex.Message}", "L?i", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string productId)
            {
                MessageBox.Show($"Tính n?ng s?a s?n ph?m {productId} s? ???c phát tri?n sau!", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string productId)
            {
                var result = MessageBox.Show($"B?n có ch?c mu?n xóa s?n ph?m {productId}?", 
                    "Xác nh?n xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    MessageBox.Show("Tính n?ng xóa s?n ph?m s? ???c phát tri?n sau!", "Thông báo", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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

    // Converter for stock status display
    public class LowStockConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int stock)
            {
                return stock <= 10 && stock > 0; // Low stock threshold
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StockStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int stock)
            {
                return stock switch
                {
                    0 => "H?t hàng",
                    <= 10 => "S?p h?t",
                    _ => "Còn hàng"
                };
            }
            return "N/A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}