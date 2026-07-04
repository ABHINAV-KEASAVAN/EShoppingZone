using Microsoft.EntityFrameworkCore;
using EShoppingZone.Data;
using EShoppingZone.Models;
using EShoppingZone.Interfaces;
using EShoppingZone.DTOs;

namespace EShoppingZone.Repositories;

/// <summary>
/// Repository implementation for product operations
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProductRepository> _logger;

    public ProductRepository(ApplicationDbContext context, ILogger<ProductRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets all products asynchronously
    /// </summary>
    /// <returns>Collection of products</returns>
    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving all products");
            return await _context.Products.Include(p => p.Category).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all products");
            throw;
        }
    }

    /// <summary>
    /// Gets paginated products asynchronously
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated result of products</returns>
    public async Task<PagedResult<Product>> GetPagedAsync(int page, int pageSize)
    {
        try
        {
            _logger.LogInformation("Retrieving products page {Page} with size {PageSize}", page, pageSize);
            
            var totalCount = await _context.Products.CountAsync();
            var items = await _context.Products
                .Include(p => p.Category)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Product>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paginated products");
            throw;
        }
    }

    /// <summary>
    /// Gets product by ID asynchronously
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product if found, null otherwise</returns>
    public async Task<Product?> GetByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Retrieving product with ID {ProductId}", id);
            return await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.ProductId == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product with ID {ProductId}", id);
            throw;
        }
    }

    /// <summary>
    /// Creates a new product asynchronously
    /// </summary>
    /// <param name="product">Product to create</param>
    /// <returns>Created product</returns>
    public async Task<Product> CreateAsync(Product product)
    {
        try
        {
            _logger.LogInformation("Creating new product: {ProductName}", product.Name);
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Product created with ID {ProductId}", product.ProductId);
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product: {ProductName}", product.Name);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing product asynchronously
    /// </summary>
    /// <param name="product">Product to update</param>
    /// <returns>Updated product</returns>
    public async Task<Product> UpdateAsync(Product product)
    {
        try
        {
            _logger.LogInformation("Updating product with ID {ProductId}", product.ProductId);
            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Product updated successfully");
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product with ID {ProductId}", product.ProductId);
            throw;
        }
    }

    /// <summary>
    /// Deletes a product asynchronously
    /// </summary>
    /// <param name="id">Product ID to delete</param>
    /// <returns>Task representing the async operation</returns>
    public async Task DeleteAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deleting product with ID {ProductId}", id);
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Product deleted successfully");
            }
            else
            {
                _logger.LogWarning("Product with ID {ProductId} not found for deletion", id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product with ID {ProductId}", id);
            throw;
        }
    }
}