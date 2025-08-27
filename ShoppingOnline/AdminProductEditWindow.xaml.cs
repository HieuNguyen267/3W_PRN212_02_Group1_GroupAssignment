using BLL.Services;
using DAL.Entities;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ShoppingOnline
{
    public partial class AdminProductEditWindow : Window, INotifyPropertyChanged
    {
        private readonly IAdminService _adminService;
        private readonly Product? _editingProduct;
        private readonly bool _isEditMode;
        private ObservableCollection<ProductImage> _productImages;

        public bool IsEditMode => _isEditMode;

        public AdminProductEditWindow(Product? product = null)
        {
            InitializeComponent();
            _adminService = new AdminService();
            _editingProduct = product;
            _isEditMode = product != null;
            _productImages = new ObservableCollection<ProductImage>();

            DataContext = this;
            LoadCategories();
            LoadProductData();
            SetupImageList();
        }

        private void LoadCategories()
        {
            try
            {
                var categories = _adminService.GetAllCategories();
                CategoryComboBox.ItemsSource = categories;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh mục: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadProductData()
        {
            if (_isEditMode && _editingProduct != null)
            {
                ProductIdTextBox.Text = _editingProduct.ProductId;
                ProductNameTextBox.Text = _editingProduct.ProductName;
                PriceTextBox.Text = _editingProduct.Price.ToString();
                StockQuantityTextBox.Text = _editingProduct.StockQuantity.ToString();
                DescriptionTextBox.Text = _editingProduct.Description ?? "";

                // Set category
                if (_editingProduct.Category != null)
                {
                    CategoryComboBox.SelectedValue = _editingProduct.CategoryId;
                }

                // Set status
                if (_editingProduct.IsActive == true)
                {
                    ActiveRadioButton.IsChecked = true;
                }
                else
                {
                    InactiveRadioButton.IsChecked = true;
                }

                // Load existing images
                if (_editingProduct.ProductImages != null)
                {
                    foreach (var image in _editingProduct.ProductImages)
                    {
                        _productImages.Add(image);
                    }
                }
            }
        }

        private void SetupImageList()
        {
            ImagesListBox.ItemsSource = _productImages;
        }

        private void AddImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Title = "Chọn hình ảnh sản phẩm",
                    Filter = "Hình ảnh|*.jpg;*.jpeg;*.png;*.gif;*.bmp|Tất cả files|*.*",
                    Multiselect = false
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    var selectedFilePath = openFileDialog.FileName;
                    var fileName = Path.GetFileName(selectedFilePath);
                    
                    // Use existing img folder
                    var imagesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img");
                    if (!Directory.Exists(imagesDir))
                    {
                        Directory.CreateDirectory(imagesDir);
                    }
                    
                    // Copy file to project img directory
                    var destinationPath = Path.Combine(imagesDir, fileName);
                    File.Copy(selectedFilePath, destinationPath, true);
                    
                    // Create relative path for database
                    var relativePath = Path.Combine("img", fileName);
                    
                    var productImage = new ProductImage
                    {
                        ImageUrl = relativePath.Replace('\\', '/'),
                        IsPrimary = _productImages.Count == 0 // First image is primary
                    };
                    
                    _productImages.Add(productImage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm hình ảnh: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveImage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ProductImage image)
            {
                var result = MessageBox.Show("Bạn có chắc muốn xóa hình ảnh này?", 
                    "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    _productImages.Remove(image);
                    
                    // If we removed the primary image and there are other images, make the first one primary
                    if (image.IsPrimary == true && _productImages.Count > 0)
                    {
                        _productImages[0].IsPrimary = true;
                    }
                }
            }
        }

        private void SetPrimaryImage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ProductImage selectedImage)
            {
                foreach (var image in _productImages)
                {
                    image.IsPrimary = (image == selectedImage);
                }
            }
        }

        private void ViewImage_Click(object sender, RoutedEventArgs e)
        {
            if (ImagesListBox.SelectedItem is ProductImage selectedImage)
            {
                try
                {
                    var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, selectedImage.ImageUrl);
                    if (File.Exists(imagePath))
                    {
                        var bitmap = new BitmapImage(new Uri(imagePath));
                        ImagePreview.Source = bitmap;
                        ImagePreviewBorder.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        // Try alternative path for existing images
                        var altPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img", Path.GetFileName(selectedImage.ImageUrl));
                        if (File.Exists(altPath))
                        {
                            var bitmap = new BitmapImage(new Uri(altPath));
                            ImagePreview.Source = bitmap;
                            ImagePreviewBorder.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy file hình ảnh!", "Lỗi", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi hiển thị hình ảnh: {ex.Message}", "Lỗi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một hình ảnh để xem!", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool ValidateInput()
        {
            bool isValid = true;
            
            // Clear previous errors
            ProductIdError.Visibility = Visibility.Collapsed;
            ProductNameError.Visibility = Visibility.Collapsed;
            CategoryError.Visibility = Visibility.Collapsed;
            PriceError.Visibility = Visibility.Collapsed;
            StockQuantityError.Visibility = Visibility.Collapsed;

            // Validate Product ID
            var productId = ProductIdTextBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(productId))
            {
                ProductIdError.Text = "Mã sản phẩm không được để trống!";
                ProductIdError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (productId.Length < 3 || productId.Length > 20)
            {
                ProductIdError.Text = "Mã sản phẩm phải từ 3-20 ký tự!";
                ProductIdError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (!_isEditMode && _adminService.GetAllProducts().Any(p => p.ProductId == productId))
            {
                ProductIdError.Text = "Mã sản phẩm đã tồn tại!";
                ProductIdError.Visibility = Visibility.Visible;
                isValid = false;
            }

            // Validate Product Name
            var productName = ProductNameTextBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(productName))
            {
                ProductNameError.Text = "Tên sản phẩm không được để trống!";
                ProductNameError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (productName.Length < 5 || productName.Length > 100)
            {
                ProductNameError.Text = "Tên sản phẩm phải từ 5-100 ký tự!";
                ProductNameError.Visibility = Visibility.Visible;
                isValid = false;
            }

            // Validate Category
            if (CategoryComboBox.SelectedItem == null)
            {
                CategoryError.Text = "Vui lòng chọn danh mục!";
                CategoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            // Validate Price
            if (!decimal.TryParse(PriceTextBox.Text, out decimal price) || price < 0)
            {
                PriceError.Text = "Giá phải là số dương!";
                PriceError.Visibility = Visibility.Visible;
                isValid = false;
            }

            // Validate Stock Quantity
            if (!int.TryParse(StockQuantityTextBox.Text, out int stockQuantity) || stockQuantity < 0)
            {
                StockQuantityError.Text = "Số lượng tồn kho phải là số nguyên dương!";
                StockQuantityError.Visibility = Visibility.Visible;
                isValid = false;
            }

            return isValid;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
            {
                return;
            }

            try
            {
                var product = new Product
                {
                    ProductId = ProductIdTextBox.Text.Trim(),
                    ProductName = ProductNameTextBox.Text.Trim(),
                    CategoryId = CategoryComboBox.SelectedValue?.ToString(),
                    Price = decimal.Parse(PriceTextBox.Text),
                    StockQuantity = int.Parse(StockQuantityTextBox.Text),
                    Description = DescriptionTextBox.Text?.Trim(),
                    IsActive = ActiveRadioButton.IsChecked == true,
                    ProductImages = _productImages.ToList()
                };

                bool success;
                if (_isEditMode)
                {
                    success = _adminService.UpdateProduct(product);
                    if (success)
                    {
                        MessageBox.Show("Cập nhật sản phẩm thành công!", "Thành công", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    success = _adminService.AddProduct(product);
                    if (success)
                    {
                        MessageBox.Show("Thêm sản phẩm thành công!", "Thành công", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }

                if (success)
                {
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Có lỗi xảy ra khi lưu sản phẩm!", "Lỗi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu sản phẩm: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
