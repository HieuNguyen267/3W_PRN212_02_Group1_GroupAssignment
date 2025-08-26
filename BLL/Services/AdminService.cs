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
        decimal GetTotalOrderValue();
        int GetTodayOrders();
        decimal GetTodayRevenue();
        List<Order> GetRecentOrders(int count = 10);
        List<Product> GetLowStockProducts(int threshold = 10);
        
        // Customer Management
        List<Customer> GetAllCustomers();
        Customer? GetCustomerById(int customerId);
        bool UpdateCustomerStatus(int customerId, bool isActive);
        bool AddCustomer(Customer customer, Account account);
        bool UpdateCustomer(Customer customer);
        bool DeleteCustomer(int customerId);
        
        // Account Management
        List<Account> GetAllAccounts();
        Account? GetAccountById(int accountId);
        bool AddAccount(Account account);
        bool UpdateAccount(Account account);
        bool DeleteAccount(int accountId);
        bool UpdateAccountStatus(int accountId, bool isActive);
        
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
        Admin? GetAdminById(int adminId);
        bool AddAdmin(Admin admin, Account account);
        bool UpdateAdmin(Admin admin);
        bool DeleteAdmin(int adminId);
        
        // Carrier Management
        List<Carrier> GetAllCarriers();
        Carrier? GetCarrierById(int carrierId);
        bool AddCarrier(Carrier carrier, Account account);
        bool UpdateCarrier(Carrier carrier);
        bool DeleteCarrier(int carrierId);
        bool UpdateCarrierStatus(int carrierId, bool isAvailable);
        
        // Helper Methods
        bool IsUsernameOrEmailExists(string username, string email, int? excludeAccountId = null);
        bool CustomerHasOrders(int customerId);
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
            // Count only non-cancelled orders
            return context.Orders
                .Where(o => o.Notes == null || !o.Notes.Contains("[CANCELLED]"))
                .Count();
        }

        public decimal GetTotalRevenue()
        {
            using var context = new ShoppingOnlineContext();
            
            // Debug: Check all orders
            var allOrders = context.Orders.ToList();
            System.Diagnostics.Debug.WriteLine($"GetTotalRevenue: Total orders in DB: {allOrders.Count}");
            
            foreach (var order in allOrders)
            {
                System.Diagnostics.Debug.WriteLine($"Order #{order.OrderId}: Status='{order.Status}', Amount={order.TotalAmount}, Notes='{order.Notes}'");
            }
            
            // Calculate revenue from completed orders only, excluding cancelled orders
            // Revenue should only include orders that are actually delivered/completed and paid
            // Cancelled orders are marked with [CANCELLED] in the Notes field
            var revenueOrders = context.Orders
                .Where(o => (o.Status == "Delivered" || o.Status == "Completed") && 
                           (o.Notes == null || !o.Notes.Contains("[CANCELLED]")))
                .ToList();
                
            System.Diagnostics.Debug.WriteLine($"GetTotalRevenue: Revenue orders count: {revenueOrders.Count}");
            
            var totalRevenue = revenueOrders.Sum(o => o.TotalAmount);
            System.Diagnostics.Debug.WriteLine($"GetTotalRevenue: Total revenue: {totalRevenue}");
            
            return totalRevenue;
        }

        public decimal GetTotalOrderValue()
        {
            using var context = new ShoppingOnlineContext();
            
            // Calculate total value of all orders (excluding cancelled orders)
            // This includes Pending, Confirmed, Shipping, Delivered, Completed orders
            var totalOrderValue = context.Orders
                .Where(o => o.Notes == null || !o.Notes.Contains("[CANCELLED]"))
                .Sum(o => o.TotalAmount);
                
            System.Diagnostics.Debug.WriteLine($"GetTotalOrderValue: Total order value: {totalOrderValue}");
            
            return totalOrderValue;
        }

        public int GetTodayOrders()
        {
            using var context = new ShoppingOnlineContext();
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            return context.Orders
                .Where(o => o.OrderDate >= today && o.OrderDate < tomorrow && 
                           (o.Notes == null || !o.Notes.Contains("[CANCELLED]")))
                .Count();
        }

        public decimal GetTodayRevenue()
        {
            using var context = new ShoppingOnlineContext();
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            return context.Orders
                .Where(o => o.OrderDate >= today && o.OrderDate < tomorrow && 
                           (o.Status == "Delivered" || o.Status == "Completed") &&
                           (o.Notes == null || !o.Notes.Contains("[CANCELLED]")))
                .Sum(o => o.TotalAmount);
        }

        public List<Order> GetRecentOrders(int count = 10)
        {
            using var context = new ShoppingOnlineContext();
            return context.Orders
                .Include(o => o.Customer)
                .Where(o => o.Notes == null || !o.Notes.Contains("[CANCELLED]")) // Exclude cancelled orders from recent orders
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
            try
            {
                using var context = new ShoppingOnlineContext();
                System.Diagnostics.Debug.WriteLine("AdminService: Starting GetAllCustomers...");
                
                var customers = context.Customers
                    .Include(c => c.Account)
                    .OrderByDescending(c => c.CreatedDate)
                    .ToList();
                    
                System.Diagnostics.Debug.WriteLine($"AdminService: Retrieved {customers.Count} customers from database");
                
                // Debug: Show some details about the customers
                foreach (var customer in customers.Take(3))
                {
                    System.Diagnostics.Debug.WriteLine($"Customer: {customer.CustomerId} - {customer.FullName} - Account: {customer.Account?.Email}");
                }
                
                return customers;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AdminService: Error in GetAllCustomers: {ex.Message}");
                throw; // Re-throw to let calling code handle it
            }
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

        public bool AddCustomer(Customer customer, Account account)
        {
            try
            {
                using var context = new ShoppingOnlineContext();
                
                // Check if username or email already exists
                var existingAccount = context.Accounts
                    .FirstOrDefault(a => a.Username == account.Username || a.Email == account.Email);
                
                if (existingAccount != null)
                {
                    System.Diagnostics.Debug.WriteLine($"AddCustomer: Username '{account.Username}' or Email '{account.Email}' already exists");
                    return false; // Username or email already exists
                }
                
                using var transaction = context.Database.BeginTransaction();
                
                try
                {
                    // Add account first
                    account.AccountType = "Customer";
                    account.CreatedDate = DateTime.Now;
                    account.IsActive = true;
                    context.Accounts.Add(account);
                    context.SaveChanges();
                    
                    System.Diagnostics.Debug.WriteLine($"AddCustomer: Account created with ID {account.AccountId}");
                    
                    // Add customer with account reference
                    customer.AccountId = account.AccountId;
                    customer.CreatedDate = DateTime.Now;
                    customer.UpdatedDate = DateTime.Now;
                    context.Customers.Add(customer);
                    context.SaveChanges();
                    
                    System.Diagnostics.Debug.WriteLine($"AddCustomer: Customer created with ID {customer.CustomerId}");
                    
                    transaction.Commit();
                    System.Diagnostics.Debug.WriteLine($"AddCustomer: Successfully added customer '{customer.FullName}' with email '{account.Email}'");
                    return true;
                }
                catch (Exception innerEx)
                {
                    transaction.Rollback();
                    System.Diagnostics.Debug.WriteLine($"AddCustomer: Transaction rolled back due to error: {innerEx.Message}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddCustomer: Error - {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"AddCustomer: Inner Exception - {ex.InnerException.Message}");
                }
                return false;
            }
        }

        public bool UpdateCustomer(Customer customer)
        {
            try
            {
                using var context = new ShoppingOnlineContext();
                var existingCustomer = context.Customers
                    .Include(c => c.Account)
                    .FirstOrDefault(c => c.CustomerId == customer.CustomerId);
                
                if (existingCustomer == null)
                {
                    System.Diagnostics.Debug.WriteLine($"UpdateCustomer: Customer with ID {customer.CustomerId} not found");
                    return false;
                }

                // Check for duplicate username/email (excluding current account)
                if (customer.Account != null && existingCustomer.Account != null)
                {
                    var duplicateAccount = context.Accounts
                        .FirstOrDefault(a => a.AccountId != existingCustomer.Account.AccountId &&
                                           (a.Username == customer.Account.Username || a.Email == customer.Account.Email));
                    
                    if (duplicateAccount != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"UpdateCustomer: Username '{customer.Account.Username}' or Email '{customer.Account.Email}' already exists");
                        return false;
                    }
                }
                
                using var transaction = context.Database.BeginTransaction();
                
                try
                {
                    // Update customer info
                    existingCustomer.FullName = customer.FullName;
                    existingCustomer.Phone = customer.Phone;
                    existingCustomer.Address = customer.Address;
                    existingCustomer.UpdatedDate = DateTime.Now;
                    
                    // Update account info if provided
                    if (customer.Account != null && existingCustomer.Account != null)
                    {
                        existingCustomer.Account.Username = customer.Account.Username;
                        existingCustomer.Account.Email = customer.Account.Email;
                        existingCustomer.Account.IsActive = customer.Account.IsActive;
                        
                        // Only update password if provided
                        if (!string.IsNullOrEmpty(customer.Account.Password))
                        {
                            existingCustomer.Account.Password = customer.Account.Password;
                        }
                    }
                    
                    context.SaveChanges();
                    transaction.Commit();
                    
                    System.Diagnostics.Debug.WriteLine($"UpdateCustomer: Successfully updated customer '{existingCustomer.FullName}'");
                    return true;
                }
                catch (Exception innerEx)
                {
                    transaction.Rollback();
                    System.Diagnostics.Debug.WriteLine($"UpdateCustomer: Transaction rolled back due to error: {innerEx.Message}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateCustomer: Error - {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"UpdateCustomer: Inner Exception - {ex.InnerException.Message}");
                }
                return false;
            }
        }

        public bool DeleteCustomer(int customerId)
        {
            try
            {
                using var context = new ShoppingOnlineContext();
                var customer = context.Customers
                    .Include(c => c.Account)
                    .Include(c => c.Orders)
                    .Include(c => c.ShoppingCarts)
                    .FirstOrDefault(c => c.CustomerId == customerId);
                
                if (customer == null)
                {
                    System.Diagnostics.Debug.WriteLine($"DeleteCustomer: Customer {customerId} not found");
                    return false;
                }

                if (customer.Account == null)
                {
                    System.Diagnostics.Debug.WriteLine($"DeleteCustomer: Customer {customerId} has no associated account");
                    return false;
                }

                using var transaction = context.Database.BeginTransaction();
                
                try
                {
                    // Check if customer has orders - if yes, only soft delete
                    if (customer.Orders.Any())
                    {
                        System.Diagnostics.Debug.WriteLine($"DeleteCustomer: Customer {customerId} has {customer.Orders.Count} orders, performing soft delete");
                        // Soft delete - deactivate account instead of hard delete
                        customer.Account.IsActive = false;
                        context.SaveChanges();
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"DeleteCustomer: Customer {customerId} has no orders, performing hard delete");
                        // Hard delete - completely remove customer and account
                        
                        // Remove shopping cart items first
                        if (customer.ShoppingCarts.Any())
                        {
                            System.Diagnostics.Debug.WriteLine($"DeleteCustomer: Removing {customer.ShoppingCarts.Count} shopping cart items");
                            context.ShoppingCarts.RemoveRange(customer.ShoppingCarts);
                        }
                        
                        // Remove customer
                        context.Customers.Remove(customer);
                        System.Diagnostics.Debug.WriteLine($"DeleteCustomer: Customer {customerId} removed");
                        
                        // Remove account
                        context.Accounts.Remove(customer.Account);
                        System.Diagnostics.Debug.WriteLine($"DeleteCustomer: Account {customer.Account.AccountId} removed");
                        
                        context.SaveChanges();
                    }
                    
                    transaction.Commit();
                    System.Diagnostics.Debug.WriteLine($"DeleteCustomer: Successfully processed deletion for customer {customerId}");
                    return true;
                }
                catch (Exception innerEx)
                {
                    transaction.Rollback();
                    System.Diagnostics.Debug.WriteLine($"DeleteCustomer: Transaction rolled back due to error: {innerEx.Message}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DeleteCustomer: Error - {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"DeleteCustomer: Inner Exception - {ex.InnerException.Message}");
                }
                return false;
            }
        }

        // Account Management
        public List<Account> GetAllAccounts()
        {
            using var context = new ShoppingOnlineContext();
            return context.Accounts
                .OrderByDescending(a => a.CreatedDate)
                .ToList();
        }

        public Account? GetAccountById(int accountId)
        {
            using var context = new ShoppingOnlineContext();
            return context.Accounts.FirstOrDefault(a => a.AccountId == accountId);
        }

        public bool AddAccount(Account account)
        {
            try
            {
                using var context = new ShoppingOnlineContext();
                account.CreatedDate = DateTime.Now;
                account.IsActive = true;
                context.Accounts.Add(account);
                context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateAccount(Account account)
        {
            try
            {
                using var context = new ShoppingOnlineContext();
                var existingAccount = context.Accounts.Find(account.AccountId);
                if (existingAccount != null)
                {
                    existingAccount.Username = account.Username;
                    existingAccount.Email = account.Email;
                    existingAccount.AccountType = account.AccountType;
                    existingAccount.IsActive = account.IsActive;
                    
                    // Only update password if provided
                    if (!string.IsNullOrEmpty(account.Password))
                    {
                        existingAccount.Password = account.Password;
                    }
                    
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

        public bool DeleteAccount(int accountId)
        {
            try
            {
                using var context = new ShoppingOnlineContext();
                var account = context.Accounts.Find(accountId);
                if (account != null)
                {
                    // Soft delete
                    account.IsActive = false;
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

        public bool UpdateAccountStatus(int accountId, bool isActive)
        {
            try
            {
                using var context = new ShoppingOnlineContext();
                var account = context.Accounts.Find(accountId);
                if (account != null)
                {
                    account.IsActive = isActive;
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
                .Include(o => o.Carrier) // Include carrier information
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
            try
            {
                using var context = new ShoppingOnlineContext();
                
                // Debug: Check total carriers first
                var allCarriers = context.Carriers.Include(c => c.Account).ToList();
                System.Diagnostics.Debug.WriteLine($"GetAvailableCarriers: Total carriers in DB: {allCarriers.Count}");
                
                foreach (var carrier in allCarriers)
                {
                    System.Diagnostics.Debug.WriteLine($"Carrier #{carrier.CarrierId}: Name='{carrier.FullName}', Available={carrier.IsAvailable}, AccountActive={carrier.Account?.IsActive}");
                }
                
                // Get available carriers
                var availableCarriers = context.Carriers
                    .Include(c => c.Account)
                    .Where(c => c.IsAvailable == true && c.Account!.IsActive == true)
                    .ToList();
                    
                System.Diagnostics.Debug.WriteLine($"GetAvailableCarriers: Available carriers count: {availableCarriers.Count}");
                
                // If no available carriers, return all carriers for testing
                if (!availableCarriers.Any())
                {
                    System.Diagnostics.Debug.WriteLine("GetAvailableCarriers: No available carriers found, returning all carriers for testing");
                    return allCarriers.Where(c => c.Account?.IsActive == true).ToList();
                }
                
                return availableCarriers;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAvailableCarriers Error: {ex.Message}");
                return new List<Carrier>();
            }
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
                .OrderByDescending(a => a.CreatedDate)
                .ToList();
        }

        public Admin? GetAdminById(int adminId)
        {
            using var context = new ShoppingOnlineContext();
            return context.Admins
                .Include(a => a.Account)
                .FirstOrDefault(a => a.AdminId == adminId);
        }

        public bool AddAdmin(Admin admin, Account account)
        {
            try
            {
                using var context = new ShoppingOnlineContext();
                
                // Check if username or email already exists
                var existingAccount = context.Accounts
                    .FirstOrDefault(a => a.Username == account.Username || a.Email == account.Email);
                
                if (existingAccount != null)
                {
                    System.Diagnostics.Debug.WriteLine($"AddAdmin: Username '{account.Username}' or Email '{account.Email}' already exists");
                    return false; // Username or email already exists
                }
                
                using var transaction = context.Database.BeginTransaction();
                
                // Add account first
                account.AccountType = "Admin";
                account.CreatedDate = DateTime.Now;
                account.IsActive = true;
                context.Accounts.Add(account);
                context.SaveChanges();
                
                // Add admin with account reference
                admin.AccountId = account.AccountId;
                admin.CreatedDate = DateTime.Now;
                context.Admins.Add(admin);
                context.SaveChanges();
                
                transaction.Commit();
                System.Diagnostics.Debug.WriteLine($"AddAdmin: Successfully added admin '{admin.FullName}' with email '{account.Email}'");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddAdmin: Error - {ex.Message}");
                return false;
            }
        }

        public bool UpdateAdmin(Admin admin)
        {
            try
            {
                using var context = new ShoppingOnlineContext();
                var existingAdmin = context.Admins
                    .Include(a => a.Account)
                    .FirstOrDefault(a => a.AdminId == admin.AdminId);
                
                if (existingAdmin != null)
                {
                    existingAdmin.FullName = admin.FullName;
                    existingAdmin.Phone = admin.Phone;
                    
                    // Update account info if provided
                    if (admin.Account != null && existingAdmin.Account != null)
                    {
                        existingAdmin.Account.Username = admin.Account.Username;
                        existingAdmin.Account.Email = admin.Account.Email;
                        if (!string.IsNullOrEmpty(admin.Account.Password))
                        {
                            existingAdmin.Account.Password = admin.Account.Password;
                        }
                    }
                    
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

        public bool DeleteAdmin(int adminId)
        {
            try
            {
                using var context = new ShoppingOnlineContext();
                var admin = context.Admins
                    .Include(a => a.Account)
                    .FirstOrDefault(a => a.AdminId == adminId);
                
                if (admin?.Account != null)
                {
                    // Soft delete - deactivate account instead of hard delete
                    admin.Account.IsActive = false;
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

        // Carrier Management
        public List<Carrier> GetAllCarriers()
        {
            using var context = new ShoppingOnlineContext();
            return context.Carriers
                .Include(c => c.Account)
                .OrderByDescending(c => c.CreatedDate)
                .ToList();
        }
        
        public Carrier? GetCarrierById(int carrierId)
        {
            using var context = new ShoppingOnlineContext();
            return context.Carriers
                .Include(c => c.Account)
                .FirstOrDefault(c => c.CarrierId == carrierId);
        }

        public bool AddCarrier(Carrier carrier, Account account)
        {
            try
            {
                using var context = new ShoppingOnlineContext();
                
                // Check if username or email already exists
                var existingAccount = context.Accounts
                    .FirstOrDefault(a => a.Username == account.Username || a.Email == account.Email);
                
                if (existingAccount != null)
                {
                    System.Diagnostics.Debug.WriteLine($"AddCarrier: Username '{account.Username}' or Email '{account.Email}' already exists");
                    return false; // Username or email already exists
                }
                
                using var transaction = context.Database.BeginTransaction();
                
                // Add account first
                account.AccountType = "Carrier";
                account.CreatedDate = DateTime.Now;
                account.IsActive = true;
                context.Accounts.Add(account);
                context.SaveChanges();
                
                // Add carrier with account reference
                carrier.AccountId = account.AccountId;
                carrier.CreatedDate = DateTime.Now;
                carrier.IsAvailable = true;
                context.Carriers.Add(carrier);
                context.SaveChanges();
                
                transaction.Commit();
                System.Diagnostics.Debug.WriteLine($"AddCarrier: Successfully added carrier '{carrier.FullName}' with email '{account.Email}'");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddCarrier: Error - {ex.Message}");
                return false;
            }
        }

        public bool UpdateCarrier(Carrier carrier)
        {
            try
            {
                using var context = new ShoppingOnlineContext();
                var existingCarrier = context.Carriers
                    .Include(c => c.Account)
                    .FirstOrDefault(c => c.CarrierId == carrier.CarrierId);
                
                if (existingCarrier != null)
                {
                    existingCarrier.FullName = carrier.FullName;
                    existingCarrier.Phone = carrier.Phone;
                    existingCarrier.VehicleNumber = carrier.VehicleNumber;
                    existingCarrier.IsAvailable = carrier.IsAvailable;
                    
                    // Update account info if provided
                    if (carrier.Account != null && existingCarrier.Account != null)
                    {
                        existingCarrier.Account.Username = carrier.Account.Username;
                        existingCarrier.Account.Email = carrier.Account.Email;
                        if (!string.IsNullOrEmpty(carrier.Account.Password))
                        {
                            existingCarrier.Account.Password = carrier.Account.Password;
                        }
                    }
                    
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

        public bool DeleteCarrier(int carrierId)
        {
            try
            {
                using var context = new ShoppingOnlineContext();
                var carrier = context.Carriers
                    .Include(c => c.Account)
                    .FirstOrDefault(c => c.CarrierId == carrierId);
                
                if (carrier?.Account != null)
                {
                    // Soft delete - deactivate account instead of hard delete
                    carrier.Account.IsActive = false;
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

        public bool UpdateCarrierStatus(int carrierId, bool isAvailable)
        {
            try
            {
                using var context = new ShoppingOnlineContext();
                var carrier = context.Carriers.Find(carrierId);
                if (carrier != null)
                {
                    carrier.IsAvailable = isAvailable;
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

        // Helper Methods
        public bool IsUsernameOrEmailExists(string username, string email, int? excludeAccountId = null)
        {
            try
            {
                using var context = new ShoppingOnlineContext();
                
                var query = context.Accounts.Where(a => a.Username == username || a.Email == email);
                
                // Exclude current account when updating
                if (excludeAccountId.HasValue)
                {
                    query = query.Where(a => a.AccountId != excludeAccountId.Value);
                }
                
                return query.Any();
            }
            catch
            {
                return true; // Return true to be safe in case of error
            }
        }

        public bool CustomerHasOrders(int customerId)
        {
            try
            {
                using var context = new ShoppingOnlineContext();
                return context.Orders.Any(o => o.CustomerId == customerId);
            }
            catch
            {
                return true; // Return true to be safe - assume has orders if error
            }
        }
    }
}