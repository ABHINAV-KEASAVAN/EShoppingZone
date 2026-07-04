using Moq;
using Microsoft.Extensions.Logging;
using EShoppingZone.Services;
using EShoppingZone.Interfaces;
using EShoppingZone.Models;
using EShoppingZone.DTOs;

namespace EShoppingZone.Tests.Services;

[TestFixture]
public class OrderServiceTests
{
    private Mock<IOrderRepository> _mockOrderRepository;
    private Mock<ICartRepository> _mockCartRepository;
    private Mock<IWalletRepository> _mockWalletRepository;
    private Mock<IProductRepository> _mockProductRepository;
    private OrderService _orderService;

    [SetUp]
    public void Setup()
    {
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockCartRepository = new Mock<ICartRepository>();
        _mockWalletRepository = new Mock<IWalletRepository>();
        _mockProductRepository = new Mock<IProductRepository>();
        
        var mockUserRepository = new Mock<IUserRepository>();
        var mockEmailService = new Mock<IEmailService>();
        var mockLogger = new Mock<ILogger<OrderService>>();

        _orderService = new OrderService(
            _mockOrderRepository.Object,
            _mockCartRepository.Object,
            _mockWalletRepository.Object,
            _mockProductRepository.Object,
            mockUserRepository.Object,
            mockEmailService.Object,
            mockLogger.Object
        );
    }

    [Test]
    public async Task CreateOrderAsync_EmptyCart_ThrowsArgumentException()
    {
        // Arrange
        var userId = 1;
        var request = new OrderRequestDTO { PaymentMethod = "Cash" };

        _mockCartRepository.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync((Cart?)null);

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await _orderService.CreateOrderAsync(userId, request));
    }

    [Test]
    public async Task CreateOrderAsync_WalletPaymentInsufficientBalance_ThrowsArgumentException()
    {
        // Arrange
        var userId = 1;
        var request = new OrderRequestDTO { PaymentMethod = "Wallet" };
        
        var cart = new Cart
        {
            CartId = 1,
            UserId = userId,
            CartItems = new List<CartItem>
            {
                new CartItem
                {
                    ProductId = 1,
                    Quantity = 1,
                    Product = new Product { ProductId = 1, Price = 100m }
                }
            }
        };

        var wallet = new Wallet { UserId = userId, Balance = 50m };

        _mockCartRepository.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(cart);
        
        _mockWalletRepository.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(wallet);

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await _orderService.CreateOrderAsync(userId, request));
    }

    [Test]
    public async Task CreateOrderAsync_ValidOrder_CreatesOrderAndClearsCart()
    {
        // Arrange
        var userId = 1;
        var request = new OrderRequestDTO { PaymentMethod = "Cash" };
        
        var product = new Product { ProductId = 1, Price = 100m, Stock = 10 };
        var cartItem = new CartItem
        {
            CartItemId = 1,
            ProductId = 1,
            Quantity = 2,
            Product = product
        };
        
        var cart = new Cart
        {
            CartId = 1,
            UserId = userId,
            CartItems = new List<CartItem> { cartItem }
        };

        _mockCartRepository.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(cart);
        
        _mockProductRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);
        
        _mockOrderRepository.Setup(r => r.CreateAsync(It.IsAny<Order>()))
            .ReturnsAsync((Order o) => new Order { OrderId = 1, UserId = o.UserId, TotalAmount = o.TotalAmount, PaymentMethod = o.PaymentMethod, Status = o.Status });

        // Act
        var result = await _orderService.CreateOrderAsync(userId, request);

        // Assert
        Assert.That(result.TotalAmount, Is.EqualTo(200m));
        Assert.That(result.PaymentMethod, Is.EqualTo("Cash"));
        Assert.That(result.Status, Is.EqualTo("Confirmed"));
        
        _mockOrderRepository.Verify(r => r.CreateAsync(It.IsAny<Order>()), Times.Once);
        _mockCartRepository.Verify(r => r.RemoveItemAsync(1), Times.Once);
        _mockProductRepository.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Once);
        
        // Verify stock was reduced
        Assert.That(product.Stock, Is.EqualTo(8));
    }

    [Test]
    public async Task CreateOrderAsync_WalletPaymentSufficientBalance_DeductsFromWallet()
    {
        // Arrange
        var userId = 1;
        var request = new OrderRequestDTO { PaymentMethod = "Wallet" };
        
        var product = new Product { ProductId = 1, Price = 100m, Stock = 10 };
        var cartItem = new CartItem
        {
            CartItemId = 1,
            ProductId = 1,
            Quantity = 1,
            Product = product
        };
        
        var cart = new Cart
        {
            CartId = 1,
            UserId = userId,
            CartItems = new List<CartItem> { cartItem }
        };

        var wallet = new Wallet { UserId = userId, Balance = 200m };

        _mockCartRepository.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(cart);
        
        _mockWalletRepository.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(wallet);
        
        _mockProductRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);
        
        _mockOrderRepository.Setup(r => r.CreateAsync(It.IsAny<Order>()))
            .ReturnsAsync(new Order { OrderId = 1, UserId = userId, TotalAmount = 100m });

        // Act
        var result = await _orderService.CreateOrderAsync(userId, request);

        // Assert
        Assert.That(wallet.Balance, Is.EqualTo(100m));
        _mockWalletRepository.Verify(r => r.UpdateAsync(wallet), Times.Once);
    }

    [Test]
    public async Task GetUserOrdersAsync_CallsRepository()
    {
        // Arrange
        var userId = 1;
        var orders = new List<Order>
        {
            new Order { OrderId = 1, UserId = userId, TotalAmount = 100m }
        };

        _mockOrderRepository.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(orders);

        // Act
        var result = await _orderService.GetUserOrdersAsync(userId);

        // Assert
        Assert.That(result.Count(), Is.EqualTo(1));
        _mockOrderRepository.Verify(r => r.GetByUserIdAsync(userId), Times.Once);
    }
}