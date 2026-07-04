using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using EShoppingZone.Controllers;
using EShoppingZone.Services;
using EShoppingZone.Interfaces;
using EShoppingZone.Models;
using EShoppingZone.DTOs;

namespace EShoppingZone.Tests.Controllers;

[TestFixture]
public class ProductsControllerTests
{
    private Mock<IProductRepository> _mockProductRepository;
    private Mock<ILogger<ProductsController>> _mockLogger;
    private ProductsController _controller;
    private ProductService _productService;

    [SetUp]
    public void Setup()
    {
        _mockProductRepository = new Mock<IProductRepository>();
        _mockLogger = new Mock<ILogger<ProductsController>>();
        var mockServiceLogger = new Mock<ILogger<ProductService>>();
        _productService = new ProductService(_mockProductRepository.Object, mockServiceLogger.Object);
        _controller = new ProductsController(_productService, _mockLogger.Object);
    }

    [Test]
    public async Task GetProducts_ReturnsOkResult_WithProductList()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { ProductId = 1, Name = "Laptop", Price = 999.99m, CategoryId = 1, Category = new Category { Name = "Electronics" } },
            new Product { ProductId = 2, Name = "Mouse", Price = 29.99m, CategoryId = 1, Category = new Category { Name = "Electronics" } }
        };

        _mockProductRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(products);

        // Act
        var result = await _controller.GetProducts();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        var returnedProducts = okResult!.Value as IEnumerable<ProductDTO>;
        Assert.That(returnedProducts!.Count(), Is.EqualTo(2));
        Assert.That(returnedProducts.First().Name, Is.EqualTo("Laptop"));
    }

    [Test]
    public async Task GetPagedProducts_WithValidParameters_ReturnsOkResult()
    {
        // Arrange
        var pagedResult = new PagedResult<Product>
        {
            Items = new List<Product>
            {
                new Product { ProductId = 1, Name = "Laptop", Price = 999.99m, CategoryId = 1, Category = new Category { Name = "Electronics" } }
            },
            Page = 1,
            PageSize = 10,
            TotalCount = 1
        };

        _mockProductRepository.Setup(r => r.GetPagedAsync(1, 10)).ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetPagedProducts(1, 10);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        var returnedPaged = okResult!.Value as PagedResult<ProductDTO>;
        Assert.That(returnedPaged!.Page, Is.EqualTo(1));
        Assert.That(returnedPaged.TotalCount, Is.EqualTo(1));
        Assert.That(returnedPaged.Items.First().Name, Is.EqualTo("Laptop"));
    }

    [Test]
    public async Task GetProduct_WithExistingId_ReturnsOkResult()
    {
        // Arrange
        var product = new Product { ProductId = 1, Name = "Laptop", Price = 999.99m, CategoryId = 1, Category = new Category { Name = "Electronics" } };
        _mockProductRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

        // Act
        var result = await _controller.GetProduct(1);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        var returnedProduct = okResult!.Value as ProductDTO;
        Assert.That(returnedProduct!.Name, Is.EqualTo("Laptop"));
        Assert.That(returnedProduct.Price, Is.EqualTo(999.99m));
    }

    [Test]
    public async Task GetProduct_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        _mockProductRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        // Act
        var result = await _controller.GetProduct(999);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task CreateProduct_WithValidProduct_ReturnsCreatedResult()
    {
        // Arrange
        var product = new Product { Name = "New Product", Price = 99.99m, Stock = 10, CategoryId = 1 };
        var createdProduct = new Product { ProductId = 1, Name = "New Product", Price = 99.99m, CategoryId = 1, Category = new Category { Name = "Electronics" } };

        _mockProductRepository.Setup(r => r.CreateAsync(It.IsAny<Product>())).ReturnsAsync(createdProduct);
        _mockProductRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(createdProduct);

        // Act
        var result = await _controller.CreateProduct(product);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
        var createdResult = result.Result as CreatedAtActionResult;
        var returnedProduct = createdResult!.Value as ProductDTO;
        Assert.That(returnedProduct!.Name, Is.EqualTo("New Product"));
    }

    [Test]
    public async Task UpdateProduct_WithMismatchedId_ReturnsBadRequest()
    {
        // Arrange
        var product = new Product { ProductId = 2, Name = "Updated Product" };

        // Act
        var result = await _controller.UpdateProduct(1, product);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task DeleteProduct_WithExistingId_ReturnsNoContent()
    {
        // Arrange
        _mockProductRepository.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteProduct(1);

        // Assert
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task DeleteProduct_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        _mockProductRepository.Setup(r => r.DeleteAsync(999)).ThrowsAsync(new KeyNotFoundException());

        // Act
        var result = await _controller.DeleteProduct(999);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }
}