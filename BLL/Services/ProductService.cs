using DAL.Entities;
using DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public interface IProductService
    {
        List<Product> GetAllProducts();
        List<Product> GetActiveProducts();
        Product? GetProductById(string productId);
        List<Product> GetProductsByCategory(string categoryId);
        List<Product> GetFeaturedProducts(int count = 8);
    }

    public class ProductService : IProductService
    {
        private readonly IProductRepo _productRepo;

        public ProductService()
        {
            _productRepo = new ProductRepo();
        }

        public List<Product> GetAllProducts()
        {
            return _productRepo.GetAllProducts();
        }

        public List<Product> GetActiveProducts()
        {
            return _productRepo.GetActiveProducts();
        }

        public Product? GetProductById(string productId)
        {
            return _productRepo.GetProductById(productId);
        }

        public List<Product> GetProductsByCategory(string categoryId)
        {
            return _productRepo.GetProductsByCategory(categoryId);
        }

        public List<Product> GetFeaturedProducts(int count = 8)
        {
            var products = _productRepo.GetActiveProducts();
            // Có thể thêm logic để lấy sản phẩm nổi bật dựa trên rating, số lượng bán, etc.
            return products.Take(count).ToList();
        }
    }
}
