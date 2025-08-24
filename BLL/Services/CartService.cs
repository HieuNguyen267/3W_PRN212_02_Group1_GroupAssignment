using DAL.Entities;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services
{
    public class CartService
    {
        private readonly CartRepo _cartRepo;

        public CartService()
        {
            _cartRepo = new CartRepo();
        }

        public List<ShoppingCart> GetCartItems(int customerId)
        {
            return _cartRepo.GetCartItemsByCustomerId(customerId);
        }

        public void AddToCart(int customerId, string productId, int quantity)
        {
            var existingItem = _cartRepo.GetCartItem(customerId, productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                _cartRepo.UpdateCartItem(existingItem);
            }
            else
            {
                var cartItem = new ShoppingCart
                {
                    CustomerId = customerId,
                    ProductId = productId,
                    Quantity = quantity,
                    AddedDate = DateTime.Now
                };
                _cartRepo.AddToCart(cartItem);
            }
        }

        public void UpdateQuantity(int cartId, int quantity)
        {
            var cartItem = _cartRepo.GetCartItemById(cartId);
            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
                _cartRepo.UpdateCartItem(cartItem);
            }
        }

        public void RemoveFromCart(int cartId)
        {
            _cartRepo.RemoveFromCart(cartId);
        }

        public void ClearCart(int customerId)
        {
            _cartRepo.ClearCustomerCart(customerId);
        }

        public decimal CalculateSubTotal(int customerId)
        {
            return _cartRepo.GetCartTotal(customerId);
        }

        public decimal CalculateShippingFee(decimal subTotal)
        {
            return subTotal > 300000 ? 0 : 30000; // Free shipping for orders > 300k
        }

        public decimal CalculateDiscount(decimal subTotal)
        {
            return subTotal > 500000 ? subTotal * 0.05m : 0; // 5% discount for orders > 500k
        }

        public decimal CalculateTotal(int customerId)
        {
            var subTotal = CalculateSubTotal(customerId);
            var shippingFee = CalculateShippingFee(subTotal);
            var discount = CalculateDiscount(subTotal);
            return subTotal + shippingFee - discount;
        }
    }
}
