using HolidayDessertStore.Data;
using HolidayDessertStore.Models;
using Microsoft.EntityFrameworkCore;

namespace HolidayDessertStore.Services
{
    public interface IShoppingCartService
    {
        Task<CartItem> AddToCartAsync(int dessertId, string cartId, int quantity);
        Task<List<CartItem>> GetCartItemsAsync(string cartId);
        Task<decimal> GetCartTotalAsync(string cartId);
        Task RemoveFromCartAsync(int cartItemId);
        Task ClearCartAsync(string cartId);
        Task UpdateCartItemQuantityAsync(int cartItemId, int quantity);
        Task<int> GetAvailableQuantityAsync(int dessertId);
    }

    public class ShoppingCartService : IShoppingCartService
    {
        private readonly ApplicationDbContext _context;

        public ShoppingCartService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CartItem> AddToCartAsync(int dessertId, string cartId, int quantity)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var dessert = await _context.Desserts.FindAsync(dessertId);
                if (dessert == null) throw new ArgumentException("Dessert not found");
                
                // Check if requested quantity is available
                if (quantity > dessert.Quantity)
                    throw new InvalidOperationException($"Only {dessert.Quantity} items available");

                var existingItem = await _context.CartItems
                    .FirstOrDefaultAsync(c => c.CartId == cartId && c.DessertId == dessertId);

                if (existingItem != null)
                {
                    // Check if total quantity after adding would exceed available stock
                    if ((existingItem.Quantity + quantity) > dessert.Quantity)
                        throw new InvalidOperationException($"Cannot add {quantity} more items. Only {dessert.Quantity - existingItem.Quantity} items available");

                    existingItem.Quantity += quantity;
                    // Update available quantity
                    dessert.Quantity -= quantity;
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return existingItem;
                }

                var cartItem = new CartItem
                {
                    CartId = cartId,
                    DessertId = dessertId,
                    Quantity = quantity,
                    Price = dessert.Price
                };

                // Update available quantity
                dessert.Quantity -= quantity;

                _context.CartItems.Add(cartItem);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return cartItem;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<CartItem>> GetCartItemsAsync(string cartId)
        {
            return await _context.CartItems
                .Include(c => c.Dessert)
                .Where(c => c.CartId == cartId)
                .ToListAsync();
        }

        public async Task<decimal> GetCartTotalAsync(string cartId)
        {
            return await _context.CartItems
                .Where(c => c.CartId == cartId)
                .SumAsync(c => c.Price * c.Quantity);
        }

        public async Task RemoveFromCartAsync(int cartItemId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var cartItem = await _context.CartItems
                    .Include(c => c.Dessert)
                    .FirstOrDefaultAsync(c => c.Id == cartItemId);

                if (cartItem != null)
                {
                    // Restore the quantity back to the dessert
                    cartItem.Dessert.Quantity += cartItem.Quantity;
                    
                    _context.CartItems.Remove(cartItem);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task ClearCartAsync(string cartId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var cartItems = await _context.CartItems
                    .Include(c => c.Dessert)
                    .Where(c => c.CartId == cartId)
                    .ToListAsync();

                foreach (var item in cartItems)
                {
                    // Restore quantities back to desserts
                    item.Dessert.Quantity += item.Quantity;
                }

                _context.CartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateCartItemQuantityAsync(int cartItemId, int quantity)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var cartItem = await _context.CartItems
                    .Include(c => c.Dessert)
                    .FirstOrDefaultAsync(c => c.Id == cartItemId);

                if (cartItem == null)
                    throw new ArgumentException("Cart item not found");

                // Calculate available quantity (current dessert quantity + current cart item quantity)
                var totalAvailable = cartItem.Dessert.Quantity + cartItem.Quantity;

                if (quantity > totalAvailable)
                    throw new InvalidOperationException($"Only {totalAvailable} items available");

                if (quantity <= 0)
                {
                    // If quantity is 0 or negative, remove item and restore quantity
                    cartItem.Dessert.Quantity += cartItem.Quantity;
                    _context.CartItems.Remove(cartItem);
                }
                else
                {
                    // Update dessert quantity based on the difference
                    var quantityDifference = cartItem.Quantity - quantity;
                    cartItem.Dessert.Quantity += quantityDifference;
                    cartItem.Quantity = quantity;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<int> GetAvailableQuantityAsync(int dessertId)
        {
            var dessert = await _context.Desserts.FindAsync(dessertId);
            if (dessert == null) throw new ArgumentException("Dessert not found");
            return dessert.Quantity;
        }
    }
}