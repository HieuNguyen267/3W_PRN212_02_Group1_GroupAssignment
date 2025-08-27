using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public interface IProductRepo
    {
        List<Product> GetAllProducts();
        List<Product> GetActiveProducts();
        Product? GetProductById(string productId);
        List<Product> GetProductsByCategory(string categoryId);
        bool AddProduct(Product product);
        bool UpdateProduct(Product product);
        bool DeleteProduct(string productId);
    }

    public class ProductRepo : IProductRepo
    {
        private readonly ShoppingOnlineContext _context;

        public ProductRepo()
        {
            _context = new ShoppingOnlineContext();
        }

        public List<Product> GetAllProducts()
        {
            return _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .ToList();
        }

        public List<Product> GetActiveProducts()
        {
            return _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => p.IsActive == true)
                .ToList();
        }

        public Product? GetProductById(string productId)
        {
            return _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefault(p => p.ProductId == productId);
        }

        public List<Product> GetProductsByCategory(string categoryId)
        {
            return _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => p.CategoryId == categoryId && p.IsActive == true)
                .ToList();
        }

        public bool AddProduct(Product product)
        {
            try
            {
                _context.Products.Add(product);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddProduct Error: {ex.Message}");
                return false;
            }
        }

        public bool UpdateProduct(Product product)
        {
            try
            {
                // Get existing product with images
                var existingProduct = _context.Products
                    .Include(p => p.ProductImages)
                    .FirstOrDefault(p => p.ProductId == product.ProductId);

                if (existingProduct == null)
                    return false;

                // Update basic properties
                existingProduct.ProductName = product.ProductName;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.Price = product.Price;
                existingProduct.StockQuantity = product.StockQuantity;
                existingProduct.Description = product.Description;
                existingProduct.IsActive = product.IsActive;

                // Handle ProductImages
                if (product.ProductImages != null)
                {
                    // Remove existing images that are not in the new list
                    var imagesToRemove = existingProduct.ProductImages
                        .Where(existing => !product.ProductImages.Any(newImg => newImg.ImageUrl == existing.ImageUrl))
                        .ToList();

                    foreach (var image in imagesToRemove)
                    {
                        _context.ProductImages.Remove(image);
                    }

                    // Add new images
                    foreach (var newImage in product.ProductImages)
                    {
                        var existingImage = existingProduct.ProductImages
                            .FirstOrDefault(img => img.ImageUrl == newImage.ImageUrl);

                        if (existingImage == null)
                        {
                            // This is a new image
                            newImage.ProductId = product.ProductId;
                            _context.ProductImages.Add(newImage);
                        }
                        else
                        {
                            // Update existing image properties
                            existingImage.IsPrimary = newImage.IsPrimary;
                        }
                    }
                }

                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateProduct Error: {ex.Message}");
                return false;
            }
        }

        public bool DeleteProduct(string productId)
        {
            try
            {
                var product = _context.Products.Find(productId);
                if (product != null)
                {
                    product.IsActive = false; // Soft delete
                    _context.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DeleteProduct Error: {ex.Message}");
                return false;
            }
        }
    }
}
