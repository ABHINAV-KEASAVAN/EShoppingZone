using System.ComponentModel.DataAnnotations;

namespace EShoppingZone.Models;

public class Category
{
    public int CategoryId { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public ICollection<Product> Products { get; set; } = new List<Product>();
}