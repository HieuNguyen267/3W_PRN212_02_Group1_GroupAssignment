using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class CartRepo
    {
        private readonly ShoppingOnlineContext _context;

        public CartRepo()
        {
            _context = new ShoppingOnlineContext();
        }

        public List<ShoppingCart> GetCartItemsByCustomerId(int customerId)
        {
            return _context.ShoppingCarts
                .Include(c => c.Product)
                .ThenInclude(p => p.ProductImages)
                .Where(c => c.CustomerId == customerId)
                .ToList();
        }

        public ShoppingCart? GetCartItem(int customerId, string productId)
        {
            return _context.ShoppingCarts
                .Include(c => c.Product)
                .ThenInclude(p => p.ProductImages)
                .FirstOrDefault(c => c.CustomerId == customerId && c.ProductId == productId);
        }

        public ShoppingCart? GetCartItemById(int cartId)
        {
            return _context.ShoppingCarts
                .Include(c => c.Product)
                .ThenInclude(p => p.ProductImages)
                .FirstOrDefault(c => c.CartId == cartId);
        }

        public void AddToCart(ShoppingCart cartItem)
        {
            _context.ShoppingCarts.Add(cartItem);
            _context.SaveChanges();
        }

        public void UpdateCartItem(ShoppingCart cartItem)
        {
            _context.ShoppingCarts.Update(cartItem);
            _context.SaveChanges();
        }

        public void RemoveFromCart(int cartId)
        {
            var cartItem = _context.ShoppingCarts.Find(cartId);
            if (cartItem != null)
            {
                _context.ShoppingCarts.Remove(cartItem);
                _context.SaveChanges();
            }
        }

        public void ClearCustomerCart(int customerId)
        {
            var cartItems = _context.ShoppingCarts.Where(c => c.CustomerId == customerId);
            _context.ShoppingCarts.RemoveRange(cartItems);
            _context.SaveChanges();
        }

        public int GetCartItemCount(int customerId)
        {
            return _context.ShoppingCarts
                .Where(c => c.CustomerId == customerId)
                .Sum(c => c.Quantity);
        }

        public decimal GetCartTotal(int customerId)
        {
            return _context.ShoppingCarts
                .Include(c => c.Product)
                .Where(c => c.CustomerId == customerId)
                .Sum(c => c.Quantity * c.Product.Price);
        }
    }
}
