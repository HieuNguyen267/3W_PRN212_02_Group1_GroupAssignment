using System;
using System.Collections.Generic;
using System.Linq;
using DAL.Entities;
using DAL.Repositories;

namespace BLL.Services
{
    public class OrderService
    {
        private readonly OrderRepo _orderRepo;

        public OrderService()
        {
            _orderRepo = new OrderRepo();
        }

        public List<Order> GetPaidOrdersByCustomer(int customerId)
        {
            return _orderRepo.GetPaidOrdersByCustomer(customerId);
        }

        public List<Order> GetAllOrdersByCustomer(int customerId)
        {
            return _orderRepo.GetAllOrdersByCustomer(customerId);
        }

        public Order GetOrderById(int orderId)
        {
            return _orderRepo.GetOrderById(orderId);
        }

        public bool CreateOrder(Order order)
        {
            return _orderRepo.CreateOrder(order);
        }

        public bool UpdateOrderStatus(int orderId, string status)
        {
            return _orderRepo.UpdateOrderStatus(orderId, status);
        }

        public bool AddOrderDetail(OrderDetail orderDetail)
        {
            return _orderRepo.AddOrderDetail(orderDetail);
        }
    }
}
