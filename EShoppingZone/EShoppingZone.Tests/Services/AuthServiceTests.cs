using Moq;
using EShoppingZone.Services;
using EShoppingZone.Interfaces;
using EShoppingZone.Models;
using EShoppingZone.DTOs;
using EShoppingZone.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EShoppingZone.Tests.Services;

[TestFixture]
public class AuthServiceTests
{
    private Mock<IUserRepository> _mockUserRepository;
    private Mock<IWalletRepository> _mockWalletRepository;
    private Mock<JwtTokenHelper> _mockJwtHelper;
    private AuthService _authService;

    [SetUp]
    public void Setup()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockWalletRepository = new Mock<IWalletRepository>();
        
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["Jwt:Key"]).Returns("ThisIsASecretKeyForJWTTokenGenerationThatIsLongEnough");
        mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("EShoppingZone");
        mockConfig.Setup(c => c["Jwt:Audience"]).Returns("EShoppingZoneUsers");
        
        var mockEmailService = new Mock<IEmailService>();
        var mockLogger = new Mock<ILogger<AuthService>>();
        _mockJwtHelper = new Mock<JwtTokenHelper>(mockConfig.Object);
        _authService = new AuthService(_mockUserRepository.Object, _mockWalletRepository.Object, mockEmailService.Object, _mockJwtHelper.Object, mockLogger.Object);
    }

    [Test]
    public async Task RegisterAsync_NewUser_ReturnsToken()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Name = "John Doe",
            Email = "john@example.com",
            Password = "password123",
            Address = "123 Main St"
        };

        _mockUserRepository.Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);
        
        _mockUserRepository.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(new User { UserId = 1, Email = request.Email, Name = request.Name });
        
        _mockWalletRepository.Setup(r => r.CreateAsync(It.IsAny<Wallet>()))
            .ReturnsAsync(new Wallet { UserId = 1, Balance = 0 });
        
        _mockJwtHelper.Setup(j => j.GenerateToken(It.IsAny<User>()))
            .Returns("test-token");

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.That(result, Is.EqualTo("test-token"));
        _mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once);
        _mockWalletRepository.Verify(r => r.CreateAsync(It.IsAny<Wallet>()), Times.Once);
    }

    [Test]
    public async Task RegisterAsync_ExistingUser_ThrowsArgumentException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Name = "John Doe",
            Email = "john@example.com",
            Password = "password123"
        };

        _mockUserRepository.Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync(new User { Email = request.Email });

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await _authService.RegisterAsync(request));
    }

    [Test]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "john@example.com",
            Password = "password123"
        };

        var user = new User
        {
            UserId = 1,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _mockUserRepository.Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);
        
        _mockJwtHelper.Setup(j => j.GenerateToken(user))
            .Returns("test-token");

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.That(result, Is.EqualTo("test-token"));
    }

    [Test]
    public async Task LoginAsync_InvalidCredentials_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "john@example.com",
            Password = "wrongpassword"
        };

        _mockUserRepository.Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        // Act & Assert
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _authService.LoginAsync(request));
    }
}