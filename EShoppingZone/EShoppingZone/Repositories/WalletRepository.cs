using Microsoft.EntityFrameworkCore;
using EShoppingZone.Data;
using EShoppingZone.Models;
using EShoppingZone.Interfaces;

namespace EShoppingZone.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly ApplicationDbContext _context;

    public WalletRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Wallet?> GetByUserIdAsync(int userId)
    {
        return await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
    }

    public async Task<Wallet> CreateAsync(Wallet wallet)
    {
        _context.Wallets.Add(wallet);
        await _context.SaveChangesAsync();
        return wallet;
    }

    public async Task<Wallet> UpdateAsync(Wallet wallet)
    {
        _context.Entry(wallet).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return wallet;
    }
}