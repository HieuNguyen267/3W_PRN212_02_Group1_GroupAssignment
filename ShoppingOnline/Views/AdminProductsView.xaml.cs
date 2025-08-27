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
                MessageBox.Show($"Loi khi tai danh muc: {ex.Message}", "Loi", 
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
                MessageBox.Show($"Loi khi tai du lieu san pham: {ex.Message}", "Loi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyProductFilter()
        {
            try
            {
                var filteredProducts = _allProducts
                    .Where(p => p.IsActive == true) // hide soft-deleted products by default
                    .AsEnumerable();

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
                    ProductCountText.Text = $"Tong: {filteredProducts.Count()} san pham";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi khi loc san pham: {ex.Message}", "Loi", 
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
            MessageBox.Show("Da lam moi danh sach san pham!", "Thong bao", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new AdminProductEditWindow();
                win.Owner = Window.GetWindow(this);
                var result = win.ShowDialog();
                if (result == true)
                {
                    LoadProducts();
                    ProductsDataGrid?.Items.Refresh();
                    MessageBox.Show("Da them san pham thanh cong!", "Thanh cong", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi khi them san pham: {ex.Message}", "Loi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                    MessageBox.Show($"Loi khi xem san pham: {ex.Message}", "Loi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string productId)
            {
                try
                {
                    var product = _allProducts.FirstOrDefault(p => p.ProductId == productId);
                    if (product == null)
                    {
                        MessageBox.Show("Khong tim thay san pham!", "Loi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var win = new AdminProductEditWindow(product);
                    win.Owner = Window.GetWindow(this);
                    var result = win.ShowDialog();
                    if (result == true)
                    {
                        LoadProducts();
                        ProductsDataGrid?.Items.Refresh();
                        MessageBox.Show("Da cap nhat san pham!", "Thanh cong", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Loi khi sua san pham: {ex.Message}", "Loi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string productId)
            {
                var result = MessageBox.Show($"Ban co chac muon xoa (vo hieu hoa) san pham {productId}?", 
                    "Xac nhan xoa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var ok = _adminService.DeleteProduct(productId);
                        if (ok)
                        {
                            LoadProducts();
                            ProductsDataGrid?.Items.Refresh();
                            MessageBox.Show("Da vo hieu hoa san pham!", "Thanh cong", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Khong the xoa san pham!", "Loi", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Loi khi xoa san pham: {ex.Message}", "Loi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
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