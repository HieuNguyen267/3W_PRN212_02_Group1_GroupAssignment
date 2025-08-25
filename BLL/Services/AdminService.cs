using DAL.Entities;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL.Services
{
    public interface IAdminService
    {
        // Dashboard Statistics
        int GetTotalCustomers();
        int GetTotalProducts();
        int GetTotalOrders();
        decimal GetTotalRevenue();
        int GetTodayOrders();
        decimal GetTodayRevenue();
        List<Order> GetRecentOrders(int count = 10);
        List<Product> GetLowStockProducts(int threshold = 10);
        
        // Customer Management
        List<Customer> GetAllCustomers();
        Customer? GetCustomerById(int customerId);
        bool UpdateCustomerStatus(int customerId, bool isActive);
        
        // Product Management
        List<Product> GetAllProducts();
        bool AddProduct(Product product);
        bool UpdateProduct(Product product);
        bool DeleteProduct(string productId);
        
        // Order Management
        List<Order> GetAllOrders();
        Order? GetOrderById(int orderId);
        bool UpdateOrderStatus(int orderId, string status);
        bool AssignCarrierToOrder(int orderId, int carrierId);
        List<OrderDetail> GetOrderDetails(int orderId);
        List<OrderDetail> GetOrderDetailsByOrderId(int orderId);
        List<Carrier> GetAvailableCarriers();
        
        // Category Management
        List<Category> GetAllCategories();
        bool AddCategory(Category category);
        bool UpdateCategory(Category category);
        bool DeleteCategory(string categoryId);
        
        // Admin Management
        Admin? ValidateAdminLogin(string emailOrPhone, string password);
        List<Admin> GetAllAdmins();
    }

    public class AdminService : IAdminService
    {
        private readonly IProductRepo _productRepo;
        private readonly OrderRepo _orderRepo;
        private readonly AccountRepo _accountRepo;
        private readonly CartRepo _cartRepo;

        public AdminService()
        {
            _productRepo = new ProductRepo();
            _orderRepo = new OrderRepo();
            _accountRepo = new AccountRepo();
            _cartRepo = new CartRepo();
        }

        // Dashboard Statistics
        public int GetTotalCustomers()
        {
            using var context = new ShoppingOnlineContext();
            return context.Customers.Count();
        }

        public int GetTotalProducts()
        {
            using var context = new ShoppingOnlineContext();
            return context.Products.Where(p => p.IsActive == true).Count();
        }

        public int GetTotalOrders()
        {
            using var context = new ShoppingOnlineContext();
            return context.Orders.Count();
        }

        public decimal GetTotalRevenue()
        {
            using var context = new ShoppingOnlineContext();
            return context.Orders.Where(o => o.Status == "Completed").Sum(o => o.TotalAmount);
        }

        public int GetTodayOrders()
        {
            using var context = new ShoppingOnlineContext();
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            return context.Orders.Where(o => o.OrderDate >= today && o.OrderDate < tomorrow).Count();
        }

        public decimal GetTodayRevenue()
        {
            using var context = new ShoppingOnlineContext();
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            return context.Orders
                .Where(o => o.OrderDate >= today && o.OrderDate < tomorrow && o.Status == "Completed")
                .Sum(o => o.TotalAmount);
        }

        public List<Order> GetRecentOrders(int count = 10)
        {
            using var context = new ShoppingOnlineContext();
            return context.Orders
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .Take(count)
                .ToList();
        }

        public List<Product> GetLowStockProducts(int threshold = 10)
        {
            using var context = new ShoppingOnlineContext();
            return context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive == true && p.StockQuantity <= threshold)
                .OrderBy(p => p.StockQuantity)
                .ToList();
        }

        // Customer Management
        public List<Customer> GetAllCustomers()
        {
            using var context = new ShoppingOnlineContext();
            return context.Customers
                .Include(c => c.Account)
                .OrderByDescending(c => c.CreatedDate)
                .ToList();
        }

        public Customer? GetCustomerById(int customerId)
        {
            using var context = new ShoppingOnlineContext();
            return context.Customers
                .Include(c => c.Account)
                .FirstOrDefault(c => c.CustomerId == customerId);
        }

        public bool UpdateCustomerStatus(int customerId, bool isActive)
        {
            try
            {
                using var context = new ShoppingOnlineContext();
                var customer = context.Customers
                    .Include(c => c.Account)
                    .FirstOrDefault(c => c.CustomerId == customerId);
                
                if (customer?.Account != null)
                {
                    customer.Account.IsActive = isActive;
                    context.SaveChanges();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        // Product Management
        public List<Product> GetAllProducts()
        {
            return _productRepo.GetAllProducts();
        }

        public bool AddProduct(Product product)
        {
            try
            {
                using var context = new ShoppingOnlineContext();
                context.Products.Add(product);
                context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateProduct(Product product)
        {
            try
            {
                using var context = new ShoppingOnlineContext();
                context.Products.Update(product);
                context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteProduct(string productId)
        {
            try
            {
                using var context = new ShoppingOnlineContext();
                var product = context.Products.Find(productId);
                if (product != null)
                {
                    product.IsActive = false; // Soft delete
                    context.SaveChanges();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        // Order Management
        public List<Order> GetAllOrders()
        {
            using var context = new ShoppingOnlineContext();
            return context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        public Order? GetOrderById(int orderId)
        {
            return _orderRepo.GetOrderById(orderId);
        }

        public bool UpdateOrderStatus(int orderId, string status)
        {
            try
            {
                using var context = new ShoppingOnlineContext();
                
                // Enhanced logging and validation
                var order = context.Orders.Find(orderId);
                if (order == null)
                {
                    System.Diagnostics.Debug.WriteLine($"UpdateOrderStatus: Order {orderId} not found");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"UpdateOrderStatus: Updating order {orderId} from {order.Status} to {status}");
                
                order.Status = status;
                var rowsAffected = context.SaveChanges();
                
                System.Diagnostics.Debug.WriteLine($"UpdateOrderStatus: {rowsAffected} rows affected");
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                // Log the actual exception for debugging
                System.Diagnostics.Debug.WriteLine($"UpdateOrderStatus Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"UpdateOrderStatus Stack Trace: {ex.StackTrace}");
                
                // Also check if it's a database connection issue
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"UpdateOrderStatus Inner Exception: {ex.InnerException.Message}");
                }
                
                return false;
            }
        }

        public bool AssignCarrierToOrder(int orderId, int carrierId)
        {
            try
            {
                using var context = new ShoppingOnlineContext();
                
                var order = context.Orders.Find(orderId);
                if (order == null)
                {
                    System.Diagnostics.Debug.WriteLine($"AssignCarrierToOrder: Order {orderId} not found");
                    return false;
                }

                var carrier = context.Carriers.Find(carrierId);
                if (carrier == null)
                {
                    System.Diagnostics.Debug.WriteLine($"AssignCarrierToOrder: Carrier {carrierId} not found");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"AssignCarrierToOrder: Assigning carrier {carrierId} to order {orderId}");
                
                order.CarrierId = carrierId;
                var rowsAffected = context.SaveChanges();
                
                System.Diagnostics.Debug.WriteLine($"AssignCarrierToOrder: {rowsAffected} rows affected");
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AssignCarrierToOrder Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"AssignCarrierToOrder Inner Exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        public List<OrderDetail> GetOrderDetails(int orderId)
        {
            using var context = new ShoppingOnlineContext();
            return context.OrderDetails
                .Include(od => od.Product)
                    .ThenInclude(p => p.Category)
                .Where(od => od.OrderId == orderId)
                .ToList();
        }

        public List<OrderDetail> GetOrderDetailsByOrderId(int orderId)
        {
            using var context = new ShoppingOnlineContext();
            return context.OrderDetails
                .Include(od => od.Product)
                    .ThenInclude(p => p.Category)
                .Where(od => od.OrderId == orderId)
                .ToList();
        }

        public List<Carrier> GetAvailableCarriers()
        {
            using var context = new ShoppingOnlineContext();
            return context.Carriers
                .Include(c => c.Account)
                .Where(c => c.IsAvailable == true && c.Account!.IsActive == true)
                .ToList();
        }

        // Category Management
        public List<Category> GetAllCategories()
        {
            using var context = new ShoppingOnlineContext();
            return context.Categories.Where(c => c.IsActive == true).ToList();
        }

        public bool AddCategory(Category category)
        {
            try
            {
                using var context = new ShoppingOnlineContext();
                context.Categories.Add(category);
                context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateCategory(Category category)
        {
            try
            {
                using var context = new ShoppingOnlineContext();
                context.Categories.Update(category);
                context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteCategory(string categoryId)
        {
            try
            {
                using var context = new ShoppingOnlineContext();
                var category = context.Categories.Find(categoryId);
                if (category != null)
                {
                    category.IsActive = false; // Soft delete
                    context.SaveChanges();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        // Admin Management
        public Admin? ValidateAdminLogin(string emailOrPhone, string password)
        {
            using var context = new ShoppingOnlineContext();
            var admin = context.Admins
                .Include(a => a.Account)
                .FirstOrDefault(a => 
                    (a.Account!.Email == emailOrPhone || a.Phone == emailOrPhone) &&
                    a.Account.Password == password &&
                    a.Account.IsActive == true &&
                    a.Account.AccountType == "Admin");
            
            return admin;
        }

        public List<Admin> GetAllAdmins()
        {
            using var context = new ShoppingOnlineContext();
            return context.Admins
                .Include(a => a.Account)
                .Where(a => a.Account!.IsActive == true)
                .ToList();
        }
    }
}