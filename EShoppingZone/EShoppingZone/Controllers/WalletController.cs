using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EShoppingZone.Services;

namespace EShoppingZone.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly WalletService _walletService;

    public WalletController(WalletService walletService)
    {
        _walletService = walletService;
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirst("UserId")?.Value ?? "0");
    }

    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
        var userId = GetUserId();
        var balance = await _walletService.GetBalanceAsync(userId);
        return Ok(new { balance });
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddFunds([FromBody] AddFundsRequest request)
    {
        var userId = GetUserId();
        await _walletService.AddFundsAsync(userId, request.Amount);
        return Ok(new { message = "Funds added successfully" });
    }
}

public class AddFundsRequest
{
    public decimal Amount { get; set; }
}