using System.ComponentModel.DataAnnotations;

namespace EShoppingZone.Models;

public class User
{
    public int UserId { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    public string Role { get; set; } = "Customer";
    
    public string Address { get; set; } = string.Empty;
    
    public Cart? Cart { get; set; }
    public Wallet? Wallet { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}