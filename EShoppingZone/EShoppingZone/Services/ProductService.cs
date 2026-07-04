using EShoppingZone.Models;
using EShoppingZone.Interfaces;
using EShoppingZone.DTOs;

namespace EShoppingZone.Services;

/// <summary>
/// Service for product-related business operations
/// </summary>
public class ProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProductRepository productRepository, ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    /// <summary>
    /// Gets all products asynchronously
    /// </summary>
    /// <returns>Collection of product DTOs</returns>
    public async Task<IEnumerable<ProductDTO>> GetAllProductsAsync()
    {
        try
        {
            _logger.LogInformation("Getting all products");
            var products = await _productRepository.GetAllAsync();
            return products.Select(MapToDTO);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all products");
            throw;
        }
    }

    /// <summary>
    /// Gets paginated products asynchronously
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated result of product DTOs</returns>
    public async Task<PagedResult<ProductDTO>> GetPagedProductsAsync(int page, int pageSize)
    {
        try
        {
            _logger.LogInformation("Getting paginated products: page {Page}, size {PageSize}", page, pageSize);
            var pagedResult = await _productRepository.GetPagedAsync(page, pageSize);
            
            return new PagedResult<ProductDTO>
            {
                Items = pagedResult.Items.Select(MapToDTO),
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize,
                TotalCount = pagedResult.TotalCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paginated products");
            throw;
        }
    }

    /// <summary>
    /// Gets product by ID asynchronously
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product DTO if found, null otherwise</returns>
    public async Task<ProductDTO?> GetProductByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Getting product by ID: {ProductId}", id);
            var product = await _productRepository.GetByIdAsync(id);
            return product != null ? MapToDTO(product) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product by ID: {ProductId}", id);
            throw;
        }
    }

    /// <summary>
    /// Creates a new product asynchronously
    /// </summary>
    /// <param name="product">Product to create</param>
    /// <returns>Created product DTO</returns>
    public async Task<ProductDTO> CreateProductAsync(Product product)
    {
        try
        {
            _logger.LogInformation("Creating product: {ProductName}", product.Name);
            var created = await _productRepository.CreateAsync(product);
            var result = await _productRepository.GetByIdAsync(created.ProductId);
            return MapToDTO(result!);
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
    /// <returns>Updated product DTO</returns>
    public async Task<ProductDTO> UpdateProductAsync(Product product)
    {
        try
        {
            _logger.LogInformation("Updating product: {ProductId}", product.ProductId);
            var updated = await _productRepository.UpdateAsync(product);
            var result = await _productRepository.GetByIdAsync(updated.ProductId);
            return MapToDTO(result!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product: {ProductId}", product.ProductId);
            throw;
        }
    }

    /// <summary>
    /// Deletes a product asynchronously
    /// </summary>
    /// <param name="id">Product ID to delete</param>
    /// <returns>Task representing the async operation</returns>
    public async Task DeleteProductAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deleting product: {ProductId}", id);
            await _productRepository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product: {ProductId}", id);
            throw;
        }
    }

    /// <summary>
    /// Maps Product entity to ProductDTO
    /// </summary>
    /// <param name="product">Product entity</param>
    /// <returns>Product DTO</returns>
    private static ProductDTO MapToDTO(Product product)
    {
        return new ProductDTO
        {
            ProductId = product.ProductId,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            CategoryId = product.CategoryId,
            CategoryName = product.Category.Name
        };
    }
}