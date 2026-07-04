using System.ComponentModel.DataAnnotations;

namespace EShoppingZone.Models;

public class CartItem
{
    public int CartItemId { get; set; }
    
    public int CartId { get; set; }
    public Cart Cart { get; set; } = null!;
    
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    
    [Required]
    public int Quantity { get; set; }
}