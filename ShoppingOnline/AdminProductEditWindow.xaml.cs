using BLL.Services;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Win32;

namespace ShoppingOnline
{
    public partial class AdminProductEditWindow : Window
    {
        private readonly IAdminService _adminService;
        private readonly bool _isEdit;
        private Product _product;
        private List<ProductImage> _images = new();

        public AdminProductEditWindow() : this(null) { }

        public AdminProductEditWindow(Product? product)
        {
            InitializeComponent();
            _adminService = new AdminService();
            _isEdit = product != null;
            _product = product ?? new Product
            {
                ProductId = string.Empty,
                IsActive = true,
                StockQuantity = 0,
                Price = 0
            };

            HeaderText.Text = _isEdit ? "Chinh sua San pham" : "Them San pham";
            ProductIdTextBox.IsEnabled = !_isEdit; // cannot edit id when editing

            LoadCategories();
            BindForm();

            // Load images if edit
            if (_isEdit)
            {
                _images = (_product.ProductImages?.ToList() ?? new List<ProductImage>())
                    .OrderBy(i => i.DisplayOrder ?? int.MaxValue).ToList();
                ImagesListBox.ItemsSource = _images;
            }
        }

        private void LoadCategories()
        {
            try
            {
                var categories = _adminService.GetAllCategories();
                CategoryComboBox.ItemsSource = categories;
                if (!string.IsNullOrEmpty(_product.CategoryId))
                {
                    CategoryComboBox.SelectedValue = _product.CategoryId;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi tai danh muc: {ex.Message}", "Loi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BindForm()
        {
            ProductIdTextBox.Text = _product.ProductId;
            ProductNameTextBox.Text = _product.ProductName ?? string.Empty;
            PriceTextBox.Text = _product.Price.ToString();
            StockTextBox.Text = _product.StockQuantity.ToString();
            IsActiveCheckBox.IsChecked = _product.IsActive ?? true;
            DescriptionTextBox.Text = _product.Description ?? string.Empty;
        }

        private bool ValidateAndCollect(out Product product)
        {
            product = _product;

            // Basic validation
            if (string.IsNullOrWhiteSpace(ProductIdTextBox.Text) && !_isEdit)
            {
                MessageBox.Show("Vui long nhap Ma san pham!", "Thieu thong tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(ProductNameTextBox.Text))
            {
                MessageBox.Show("Vui long nhap Ten san pham!", "Thieu thong tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (CategoryComboBox.SelectedValue == null)
            {
                MessageBox.Show("Vui long chon Danh muc!", "Thieu thong tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (!decimal.TryParse(PriceTextBox.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Gia khong hop le!", "Loi du lieu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (!int.TryParse(StockTextBox.Text, out int stock) || stock < 0)
            {
                MessageBox.Show("So luong ton khong hop le!", "Loi du lieu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Additional validation rules
            if (ProductIdTextBox.Text?.IndexOf(' ') >= 0)
            {
                MessageBox.Show("Ma san pham khong duoc chua khoang trang!", "Loi du lieu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (ProductNameTextBox.Text.Length < 3)
            {
                MessageBox.Show("Ten san pham phai tu 3 ky tu tro len!", "Loi du lieu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (price > 1000000000m)
            {
                MessageBox.Show("Gia qua lon ( > 1,000,000,000 )!", "Loi du lieu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (stock > 100000)
            {
                MessageBox.Show("So luong ton qua lon ( > 100,000 )!", "Loi du lieu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // collect
            product.ProductId = _isEdit ? _product.ProductId : ProductIdTextBox.Text.Trim();
            product.ProductName = ProductNameTextBox.Text.Trim();
            product.CategoryId = CategoryComboBox.SelectedValue?.ToString();
            product.Price = price;
            product.StockQuantity = stock;
            product.IsActive = IsActiveCheckBox.IsChecked == true;
            product.Description = DescriptionTextBox.Text.Trim();
            product.UpdatedDate = DateTime.Now;
            if (!_isEdit)
            {
                product.CreatedDate = DateTime.Now;
            }
            
            // attach images to product
            product.ProductImages = _images;
            return true;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateAndCollect(out var product)) return;

                bool ok = _isEdit ? _adminService.UpdateProduct(product) : _adminService.AddProduct(product);
                if (ok)
                {
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Khong the luu san pham!", "Loi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi: {ex.Message}", "Loi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void AddImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new OpenFileDialog
                {
                    Filter = "Image Files (*.jpg;*.jpeg;*.png;*.gif)|*.jpg;*.jpeg;*.png;*.gif|All files (*.*)|*.*",
                    Multiselect = true
                };
                if (dlg.ShowDialog(this) == true)
                {
                    int nextOrder = (_images.Any() ? (_images.Max(i => i.DisplayOrder ?? 0) + 1) : 1);
                    foreach (var file in dlg.FileNames)
                    {
                        _images.Add(new ProductImage
                        {
                            ProductId = _product.ProductId,
                            ImageUrl = file, // store local path; ideally should be copied to an assets folder
                            IsPrimary = _images.Count == 0, // first image default primary
                            DisplayOrder = nextOrder++
                        });
                    }
                    ImagesListBox.ItemsSource = null;
                    ImagesListBox.ItemsSource = _images.OrderBy(i => i.DisplayOrder ?? int.MaxValue).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi khi them hinh: {ex.Message}", "Loi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveImage_Click(object sender, RoutedEventArgs e)
        {
            var selected = ImagesListBox.SelectedItem as ProductImage;
            if (selected == null)
            {
                MessageBox.Show("Vui long chon hinh de xoa!", "Thong bao", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            _images.Remove(selected);
            ImagesListBox.ItemsSource = null;
            ImagesListBox.ItemsSource = _images.OrderBy(i => i.DisplayOrder ?? int.MaxValue).ToList();
        }

        private void SetPrimaryImage_Click(object sender, RoutedEventArgs e)
        {
            var selected = ImagesListBox.SelectedItem as ProductImage;
            if (selected == null)
            {
                MessageBox.Show("Vui long chon hinh de dat lam anh chinh!", "Thong bao", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            foreach (var img in _images)
            {
                img.IsPrimary = false;
            }
            selected.IsPrimary = true;
            ImagesListBox.ItemsSource = null;
            ImagesListBox.ItemsSource = _images.OrderBy(i => i.DisplayOrder ?? int.MaxValue).ToList();
        }
    }
}

