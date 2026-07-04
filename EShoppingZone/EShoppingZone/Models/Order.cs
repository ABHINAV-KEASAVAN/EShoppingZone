using System.ComponentModel.DataAnnotations;

namespace EShoppingZone.Models;

public class Order
{
    public int OrderId { get; set; }
    
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    
    [Required]
    public decimal TotalAmount { get; set; }
    
    public string Status { get; set; } = "Pending";
    
    public string PaymentMethod { get; set; } = string.Empty;
    
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}