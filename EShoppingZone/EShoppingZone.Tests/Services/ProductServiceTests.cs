using Moq;
using Microsoft.Extensions.Logging;
using EShoppingZone.Services;
using EShoppingZone.Interfaces;
using EShoppingZone.Models;
using EShoppingZone.DTOs;

namespace EShoppingZone.Tests.Services;

[TestFixture]
public class ProductServiceTests
{
    private Mock<IProductRepository> _mockProductRepository;
    private ProductService _productService;

    [SetUp]
    public void Setup()
    {
        _mockProductRepository = new Mock<IProductRepository>();
        var mockLogger = new Mock<ILogger<ProductService>>();
        _productService = new ProductService(_mockProductRepository.Object, mockLogger.Object);
    }

    [Test]
    public async Task GetAllProductsAsync_ReturnsProductDTOs()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product
            {
                ProductId = 1,
                Name = "Laptop",
                Description = "Gaming Laptop",
                Price = 999.99m,
                Stock = 10,
                CategoryId = 1,
                Category = new Category { CategoryId = 1, Name = "Electronics" }
            },
            new Product
            {
                ProductId = 2,
                Name = "Book",
                Description = "Programming Book",
                Price = 49.99m,
                Stock = 20,
                CategoryId = 2,
                Category = new Category { CategoryId = 2, Name = "Books" }
            }
        };

        _mockProductRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(products);

        // Act
        var result = await _productService.GetAllProductsAsync();

        // Assert
        Assert.That(result.Count(), Is.EqualTo(2));
        var firstProduct = result.First();
        Assert.That(firstProduct.Name, Is.EqualTo("Laptop"));
        Assert.That(firstProduct.CategoryName, Is.EqualTo("Electronics"));
    }

    [Test]
    public async Task GetProductByIdAsync_ExistingProduct_ReturnsProductDTO()
    {
        // Arrange
        var product = new Product
        {
            ProductId = 1,
            Name = "Laptop",
            Description = "Gaming Laptop",
            Price = 999.99m,
            Stock = 10,
            CategoryId = 1,
            Category = new Category { CategoryId = 1, Name = "Electronics" }
        };

        _mockProductRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);

        // Act
        var result = await _productService.GetProductByIdAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Laptop"));
        Assert.That(result.CategoryName, Is.EqualTo("Electronics"));
    }

    [Test]
    public async Task GetProductByIdAsync_NonExistingProduct_ReturnsNull()
    {
        // Arrange
        _mockProductRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _productService.GetProductByIdAsync(999);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateProductAsync_ValidProduct_ReturnsProductDTO()
    {
        // Arrange
        var product = new Product
        {
            Name = "New Product",
            Description = "Test Product",
            Price = 99.99m,
            Stock = 5,
            CategoryId = 1
        };

        var createdProduct = new Product
        {
            ProductId = 1,
            Name = "New Product",
            Description = "Test Product",
            Price = 99.99m,
            Stock = 5,
            CategoryId = 1,
            Category = new Category { CategoryId = 1, Name = "Electronics" }
        };

        _mockProductRepository.Setup(r => r.CreateAsync(product))
            .ReturnsAsync(createdProduct);
        
        _mockProductRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(createdProduct);

        // Act
        var result = await _productService.CreateProductAsync(product);

        // Assert
        Assert.That(result.Name, Is.EqualTo("New Product"));
        Assert.That(result.CategoryName, Is.EqualTo("Electronics"));
        _mockProductRepository.Verify(r => r.CreateAsync(product), Times.Once);
    }

    [Test]
    public async Task DeleteProductAsync_CallsRepositoryDelete()
    {
        // Arrange
        var productId = 1;

        // Act
        await _productService.DeleteProductAsync(productId);

        // Assert
        _mockProductRepository.Verify(r => r.DeleteAsync(productId), Times.Once);
    }
}