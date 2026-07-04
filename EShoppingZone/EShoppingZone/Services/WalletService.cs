using EShoppingZone.Models;
using EShoppingZone.Interfaces;

namespace EShoppingZone.Services;

public class WalletService
{
    private readonly IWalletRepository _walletRepository;

    public WalletService(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task<decimal> GetBalanceAsync(int userId)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId);
        return wallet?.Balance ?? 0;
    }

    public async Task AddFundsAsync(int userId, decimal amount)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId);
        if (wallet == null)
        {
            wallet = await _walletRepository.CreateAsync(new Wallet { UserId = userId, Balance = amount });
        }
        else
        {
            wallet.Balance += amount;
            await _walletRepository.UpdateAsync(wallet);
        }
    }
}