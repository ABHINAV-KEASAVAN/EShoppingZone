using Moq;
using EShoppingZone.Services;
using EShoppingZone.Interfaces;
using EShoppingZone.Models;
using EShoppingZone.DTOs;

namespace EShoppingZone.Tests.Services;

[TestFixture]
public class CartServiceTests
{
    private Mock<ICartRepository> _mockCartRepository;
    private Mock<IProductRepository> _mockProductRepository;
    private CartService _cartService;

    [SetUp]
    public void Setup()
    {
        _mockCartRepository = new Mock<ICartRepository>();
        _mockProductRepository = new Mock<IProductRepository>();
        _cartService = new CartService(_mockCartRepository.Object, _mockProductRepository.Object);
    }

    [Test]
    public async Task AddToCartAsync_NewCart_CreatesCartAndAddsItem()
    {
        // Arrange
        var userId = 1;
        var productId = 1;
        var quantity = 2;

        _mockCartRepository.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync((Cart?)null);

        var newCart = new Cart { CartId = 1, UserId = userId };
        _mockCartRepository.Setup(r => r.CreateAsync(It.IsAny<Cart>()))
            .ReturnsAsync(newCart);

        _mockCartRepository.Setup(r => r.GetCartItemAsync(1, productId))
            .ReturnsAsync((CartItem?)null);

        // Act
        await _cartService.AddToCartAsync(userId, productId, quantity);

        // Assert
        _mockCartRepository.Verify(r => r.CreateAsync(It.IsAny<Cart>()), Times.Once);
        _mockCartRepository.Verify(r => r.AddItemAsync(It.IsAny<CartItem>()), Times.Once);
    }

    [Test]
    public async Task AddToCartAsync_ExistingCartItem_UpdatesQuantity()
    {
        // Arrange
        var userId = 1;
        var productId = 1;
        var quantity = 2;

        var cart = new Cart { CartId = 1, UserId = userId };
        var existingItem = new CartItem { CartItemId = 1, CartId = 1, ProductId = productId, Quantity = 1 };

        _mockCartRepository.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(cart);

        _mockCartRepository.Setup(r => r.GetCartItemAsync(1, productId))
            .ReturnsAsync(existingItem);

        // Act
        await _cartService.AddToCartAsync(userId, productId, quantity);

        // Assert
        Assert.That(existingItem.Quantity, Is.EqualTo(3));
        _mockCartRepository.Verify(r => r.UpdateItemAsync(existingItem), Times.Once);
    }

    [Test]
    public async Task GetCartItemsAsync_ExistingCart_ReturnsCartItemDTOs()
    {
        // Arrange
        var userId = 1;
        var cart = new Cart
        {
            CartId = 1,
            UserId = userId,
            CartItems = new List<CartItem>
            {
                new CartItem
                {
                    CartItemId = 1,
                    ProductId = 1,
                    Quantity = 2,
                    Product = new Product { ProductId = 1, Name = "Laptop", Price = 999.99m }
                }
            }
        };

        _mockCartRepository.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(cart);

        // Act
        var result = await _cartService.GetCartItemsAsync(userId);

        // Assert
        Assert.That(result.Count(), Is.EqualTo(1));
        var item = result.First();
        Assert.That(item.ProductName, Is.EqualTo("Laptop"));
        Assert.That(item.Quantity, Is.EqualTo(2));
        Assert.That(item.Total, Is.EqualTo(1999.98m));
    }

    [Test]
    public async Task GetCartItemsAsync_NoCart_ReturnsEmptyList()
    {
        // Arrange
        var userId = 1;

        _mockCartRepository.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync((Cart?)null);

        // Act
        var result = await _cartService.GetCartItemsAsync(userId);

        // Assert
        Assert.That(result.Count(), Is.EqualTo(0));
    }

    [Test]
    public async Task RemoveFromCartAsync_ExistingItem_RemovesItem()
    {
        // Arrange
        var userId = 1;
        var productId = 1;
        var cartItem = new CartItem { CartItemId = 1, ProductId = productId };
        var cart = new Cart
        {
            CartId = 1,
            UserId = userId,
            CartItems = new List<CartItem> { cartItem }
        };

        _mockCartRepository.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(cart);

        // Act
        await _cartService.RemoveFromCartAsync(userId, productId);

        // Assert
        _mockCartRepository.Verify(r => r.RemoveItemAsync(1), Times.Once);
    }
}