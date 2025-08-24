using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DAL.Entities;

namespace DAL.Repositories
{
    public class OrderRepo
    {
        private readonly ShoppingOnlineContext _context;

        public OrderRepo()
        {
            _context = new ShoppingOnlineContext();
        }

        public List<Order> GetPaidOrdersByCustomer(int customerId)
        {
            return _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.Category)
                .Where(o => o.CustomerId == customerId && o.Status == "Pending")
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        public List<Order> GetAllOrdersByCustomer(int customerId)
        {
            return _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.Category)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        public Order GetOrderById(int orderId)
        {
            return _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.Category)
                .FirstOrDefault(o => o.OrderId == orderId);
        }

        public bool CreateOrder(Order order)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Adding order to context: CustomerId={order.CustomerId}, TotalAmount={order.TotalAmount}");
                _context.Orders.Add(order);
                System.Diagnostics.Debug.WriteLine("Saving changes...");
                _context.SaveChanges();
                System.Diagnostics.Debug.WriteLine($"Order saved successfully with ID: {order.OrderId}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating order: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public bool UpdateOrderStatus(int orderId, string status)
        {
            try
            {
                var order = _context.Orders.Find(orderId);
                if (order != null)
                {
                    order.Status = status;
                    _context.SaveChanges();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool AddOrderDetail(OrderDetail orderDetail)
        {
            try
            {
                _context.OrderDetails.Add(orderDetail);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding order detail: {ex.Message}");
                return false;
            }
        }
    }
}
