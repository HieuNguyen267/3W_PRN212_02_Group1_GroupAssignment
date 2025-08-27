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
    }

    public class ProductRepo : IProductRepo
    {
        public ProductRepo() { }

        public List<Product> GetAllProducts()
        {
            using var context = new ShoppingOnlineContext();
            return context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .ToList();
        }

        public List<Product> GetActiveProducts()
        {
            using var context = new ShoppingOnlineContext();
            return context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => p.IsActive == true)
                .ToList();
        }

        public Product? GetProductById(string productId)
        {
            using var context = new ShoppingOnlineContext();
            return context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefault(p => p.ProductId == productId);
        }

        public List<Product> GetProductsByCategory(string categoryId)
        {
            using var context = new ShoppingOnlineContext();
            return context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => p.CategoryId == categoryId && p.IsActive == true)
                .ToList();
        }
    }
}
