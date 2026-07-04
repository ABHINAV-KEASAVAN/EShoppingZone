using EShoppingZone.Models;

namespace EShoppingZone.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(int userId);
    Task<Cart> CreateAsync(Cart cart);
    Task<CartItem> AddItemAsync(CartItem cartItem);
    Task RemoveItemAsync(int cartItemId);
    Task<CartItem?> GetCartItemAsync(int cartId, int productId);
    Task UpdateItemAsync(CartItem cartItem);
}