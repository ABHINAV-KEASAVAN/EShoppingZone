using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using EShoppingZone.Data;
using EShoppingZone.Models;
using EShoppingZone.Repositories;

namespace EShoppingZone.Tests.Repositories;

[TestFixture]
public class ProductRepositoryTests
{
    private ApplicationDbContext _context;
    private ProductRepository _repository;
    private Mock<ILogger<ProductRepository>> _mockLogger;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _mockLogger = new Mock<ILogger<ProductRepository>>();
        _repository = new ProductRepository(_context, _mockLogger.Object);

        // Seed test data
        SeedTestData();
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    private void SeedTestData()
    {
        var category = new Category { CategoryId = 1, Name = "Electronics" };
        _context.Categories.Add(category);

        var products = new List<Product>
        {
            new Product { ProductId = 1, Name = "Laptop", Price = 999.99m, Stock = 10, CategoryId = 1, Category = category },
            new Product { ProductId = 2, Name = "Mouse", Price = 29.99m, Stock = 50, CategoryId = 1, Category = category },
            new Product { ProductId = 3, Name = "Keyboard", Price = 79.99m, Stock = 25, CategoryId = 1, Category = category }
        };

        _context.Products.AddRange(products);
        _context.SaveChanges();
    }

    [Test]
    public async Task GetAllAsync_ReturnsAllProducts()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.That(result.Count(), Is.EqualTo(3));
        Assert.That(result.All(p => p.Category != null), Is.True);
    }

    [Test]
    public async Task GetPagedAsync_ReturnsCorrectPage()
    {
        // Act
        var result = await _repository.GetPagedAsync(1, 2);

        // Assert
        Assert.That(result.Items.Count(), Is.EqualTo(2));
        Assert.That(result.Page, Is.EqualTo(1));
        Assert.That(result.PageSize, Is.EqualTo(2));
        Assert.That(result.TotalCount, Is.EqualTo(3));
        Assert.That(result.TotalPages, Is.EqualTo(2));
    }

    [Test]
    public async Task GetPagedAsync_SecondPage_ReturnsRemainingItems()
    {
        // Act
        var result = await _repository.GetPagedAsync(2, 2);

        // Assert
        Assert.That(result.Items.Count(), Is.EqualTo(1));
        Assert.That(result.Page, Is.EqualTo(2));
        Assert.That(result.HasNextPage, Is.False);
        Assert.That(result.HasPreviousPage, Is.True);
    }

    [Test]
    public async Task GetByIdAsync_WithExistingId_ReturnsProduct()
    {
        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("Laptop"));
        Assert.That(result.Category, Is.Not.Null);
    }

    [Test]
    public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateAsync_AddsProductToDatabase()
    {
        // Arrange
        var newProduct = new Product
        {
            Name = "Monitor",
            Price = 299.99m,
            Stock = 15,
            CategoryId = 1
        };

        // Act
        var result = await _repository.CreateAsync(newProduct);

        // Assert
        Assert.That(result.ProductId, Is.GreaterThan(0));
        Assert.That(result.Name, Is.EqualTo("Monitor"));

        var productInDb = await _context.Products.FindAsync(result.ProductId);
        Assert.That(productInDb, Is.Not.Null);
        Assert.That(productInDb!.Name, Is.EqualTo("Monitor"));
    }

    [Test]
    public async Task UpdateAsync_ModifiesExistingProduct()
    {
        // Arrange
        var product = await _context.Products.FindAsync(1);
        product!.Name = "Updated Laptop";
        product.Price = 1199.99m;

        // Act
        var result = await _repository.UpdateAsync(product);

        // Assert
        Assert.That(result.Name, Is.EqualTo("Updated Laptop"));
        Assert.That(result.Price, Is.EqualTo(1199.99m));

        var productInDb = await _context.Products.FindAsync(1);
        Assert.That(productInDb!.Name, Is.EqualTo("Updated Laptop"));
        Assert.That(productInDb.Price, Is.EqualTo(1199.99m));
    }

    [Test]
    public async Task DeleteAsync_RemovesProductFromDatabase()
    {
        // Act
        await _repository.DeleteAsync(1);

        // Assert
        var productInDb = await _context.Products.FindAsync(1);
        Assert.That(productInDb, Is.Null);

        var remainingProducts = await _context.Products.CountAsync();
        Assert.That(remainingProducts, Is.EqualTo(2));
    }

    [Test]
    public async Task DeleteAsync_WithNonExistingId_DoesNotThrow()
    {
        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _repository.DeleteAsync(999));
    }
}