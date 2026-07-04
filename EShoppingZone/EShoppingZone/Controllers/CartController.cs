using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EShoppingZone.Services;
using System.Security.Claims;

namespace EShoppingZone.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly CartService _cartService;

    public CartController(CartService cartService)
    {
        _cartService = cartService;
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirst("UserId")?.Value ?? "0");
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        var userId = GetUserId();
        await _cartService.AddToCartAsync(userId, request.ProductId, request.Quantity);
        return Ok(new { message = "Item added to cart" });
    }

    [HttpDelete("remove")]
    public async Task<IActionResult> RemoveFromCart([FromBody] RemoveFromCartRequest request)
    {
        var userId = GetUserId();
        await _cartService.RemoveFromCartAsync(userId, request.ProductId);
        return Ok(new { message = "Item removed from cart" });
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var userId = GetUserId();
        var items = await _cartService.GetCartItemsAsync(userId);
        return Ok(items);
    }
}

public class AddToCartRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class RemoveFromCartRequest
{
    public int ProductId { get; set; }
}