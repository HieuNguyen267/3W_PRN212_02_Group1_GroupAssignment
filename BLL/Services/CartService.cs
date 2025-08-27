using DAL.Entities;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services
{
    public class CartService
    {
        private readonly CartRepo _cartRepo;
        private readonly ProductRepo _productRepo;

        public CartService()
        {
            _cartRepo = new CartRepo();
            _productRepo = new ProductRepo();
        }

        public List<ShoppingCart> GetCartItems(int customerId)
        {
            return _cartRepo.GetCartItemsByCustomerId(customerId);
        }

        public void AddToCart(int customerId, string productId, int quantity)
        {
            // Check stock availability first
            var product = _productRepo.GetProductById(productId);
            if (product == null)
            {
                throw new InvalidOperationException("Sản phẩm không tồn tại!");
            }

            if (product.StockQuantity <= 0)
            {
                throw new InvalidOperationException("Sản phẩm đã hết hàng, vui lòng chọn sản phẩm khác!");
            }

            var existingItem = _cartRepo.GetCartItem(customerId, productId);

            if (existingItem != null)
            {
                // Check if adding this quantity would exceed stock
                int newTotalQuantity = existingItem.Quantity + quantity;
                if (newTotalQuantity > product.StockQuantity)
                {
                    throw new InvalidOperationException($"Số lượng vượt quá tồn kho! Hiện tại chỉ còn {product.StockQuantity} sản phẩm trong kho.");
                }

                existingItem.Quantity = newTotalQuantity;
                _cartRepo.UpdateCartItem(existingItem);
            }
            else
            {
                // Check if requested quantity exceeds stock
                if (quantity > product.StockQuantity)
                {
                    throw new InvalidOperationException($"Số lượng vượt quá tồn kho! Hiện tại chỉ còn {product.StockQuantity} sản phẩm trong kho.");
                }

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
                // Check stock availability
                var product = _productRepo.GetProductById(cartItem.ProductId);
                if (product == null)
                {
                    throw new InvalidOperationException("Sản phẩm không tồn tại!");
                }

                if (product.StockQuantity <= 0)
                {
                    throw new InvalidOperationException("Sản phẩm đã hết hàng, vui lòng chọn sản phẩm khác!");
                }

                if (quantity > product.StockQuantity)
                {
                    throw new InvalidOperationException($"Số lượng vượt quá tồn kho! Hiện tại chỉ còn {product.StockQuantity} sản phẩm trong kho.");
                }

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

        public int GetProductStock(string productId)
        {
            var product = _productRepo.GetProductById(productId);
            return product?.StockQuantity ?? 0;
        }
    }
}
