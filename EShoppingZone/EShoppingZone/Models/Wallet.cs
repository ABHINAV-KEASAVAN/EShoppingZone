using System.ComponentModel.DataAnnotations;

namespace EShoppingZone.Models;

public class Wallet
{
    public int WalletId { get; set; }
    
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    [Required]
    public decimal Balance { get; set; } = 0;
}