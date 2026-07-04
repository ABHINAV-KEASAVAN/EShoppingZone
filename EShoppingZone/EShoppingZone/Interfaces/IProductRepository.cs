using EShoppingZone.Models;
using EShoppingZone.DTOs;

namespace EShoppingZone.Interfaces;

/// <summary>
/// Interface for product repository operations
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Gets all products asynchronously
    /// </summary>
    /// <returns>Collection of products</returns>
    Task<IEnumerable<Product>> GetAllAsync();
    
    /// <summary>
    /// Gets paginated products asynchronously
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated result of products</returns>
    Task<PagedResult<Product>> GetPagedAsync(int page, int pageSize);
    
    /// <summary>
    /// Gets product by ID asynchronously
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product if found, null otherwise</returns>
    Task<Product?> GetByIdAsync(int id);
    
    /// <summary>
    /// Creates a new product asynchronously
    /// </summary>
    /// <param name="product">Product to create</param>
    /// <returns>Created product</returns>
    Task<Product> CreateAsync(Product product);
    
    /// <summary>
    /// Updates an existing product asynchronously
    /// </summary>
    /// <param name="product">Product to update</param>
    /// <returns>Updated product</returns>
    Task<Product> UpdateAsync(Product product);
    
    /// <summary>
    /// Deletes a product asynchronously
    /// </summary>
    /// <param name="id">Product ID to delete</param>
    /// <returns>Task representing the async operation</returns>
    Task DeleteAsync(int id);
}