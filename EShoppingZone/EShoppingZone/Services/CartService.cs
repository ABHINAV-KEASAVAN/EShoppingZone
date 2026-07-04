using EShoppingZone.Models;
using EShoppingZone.Interfaces;
using EShoppingZone.DTOs;

namespace EShoppingZone.Services;

public class CartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;

    public CartService(ICartRepository cartRepository, IProductRepository productRepository)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
    }

    public async Task AddToCartAsync(int userId, int productId, int quantity)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart == null)
        {
            cart = await _cartRepository.CreateAsync(new Cart { UserId = userId });
        }

        var existingItem = await _cartRepository.GetCartItemAsync(cart.CartId, productId);
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
            await _cartRepository.UpdateItemAsync(existingItem);
        }
        else
        {
            await _cartRepository.AddItemAsync(new CartItem
            {
                CartId = cart.CartId,
                ProductId = productId,
                Quantity = quantity
            });
        }
    }

    public async Task RemoveFromCartAsync(int userId, int productId)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart == null) return;

        var item = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
        if (item != null)
        {
            await _cartRepository.RemoveItemAsync(item.CartItemId);
        }
    }

    public async Task<IEnumerable<CartItemDTO>> GetCartItemsAsync(int userId)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart == null) return new List<CartItemDTO>();

        return cart.CartItems.Select(ci => new CartItemDTO
        {
            ProductId = ci.ProductId,
            ProductName = ci.Product.Name,
            Price = ci.Product.Price,
            Quantity = ci.Quantity
        });
    }
}