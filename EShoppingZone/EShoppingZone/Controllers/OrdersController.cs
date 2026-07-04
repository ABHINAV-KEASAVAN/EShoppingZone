using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EShoppingZone.Services;
using EShoppingZone.DTOs;

namespace EShoppingZone.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirst("UserId")?.Value ?? "0");
    }

    private static OrderResponseDTO MapToDTO(Order order) => new()
    {
        OrderId = order.OrderId,
        OrderDate = order.OrderDate,
        TotalAmount = order.TotalAmount,
        Status = order.Status,
        PaymentMethod = order.PaymentMethod,
        OrderItems = order.OrderItems.Select(oi => new OrderItemDTO
        {
            ProductId = oi.ProductId,
            ProductName = oi.Product?.Name ?? string.Empty,
            Quantity = oi.Quantity,
            UnitPrice = oi.UnitPrice
        }).ToList()
    };

    [HttpPost]
    public async Task<IActionResult> CreateOrder(OrderRequestDTO request)
    {
        var userId = GetUserId();
        var order = await _orderService.CreateOrderAsync(userId, request);
        return Ok(MapToDTO(order));
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyOrders()
    {
        var userId = GetUserId();
        var orders = await _orderService.GetUserOrdersAsync(userId);
        return Ok(orders.Select(MapToDTO));
    }
}