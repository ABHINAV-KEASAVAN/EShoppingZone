using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using EShoppingZone.Middleware;

namespace EShoppingZone.Tests.Middleware;

[TestFixture]
public class GlobalExceptionHandlerTests
{
    private Mock<ILogger<GlobalExceptionHandler>> _mockLogger;
    private GlobalExceptionHandler? _middleware;
    private DefaultHttpContext _httpContext;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<GlobalExceptionHandler>>();
        _httpContext = new DefaultHttpContext();
        _httpContext.Response.Body = new MemoryStream();
    }

    [Test]
    public async Task InvokeAsync_WithNoException_CallsNextDelegate()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = (HttpContext context) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        _middleware = new GlobalExceptionHandler(next, _mockLogger.Object);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.That(nextCalled, Is.True);
    }

    [Test]
    public async Task InvokeAsync_WithArgumentException_Returns400BadRequest()
    {
        // Arrange
        RequestDelegate next = (HttpContext context) =>
        {
            throw new ArgumentException("Invalid argument");
        };

        _middleware = new GlobalExceptionHandler(next, _mockLogger.Object);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(400));
        Assert.That(_httpContext.Response.ContentType, Is.EqualTo("application/json"));
    }

    [Test]
    public async Task InvokeAsync_WithUnauthorizedAccessException_Returns401Unauthorized()
    {
        // Arrange
        RequestDelegate next = (HttpContext context) =>
        {
            throw new UnauthorizedAccessException("Unauthorized");
        };

        _middleware = new GlobalExceptionHandler(next, _mockLogger.Object);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(401));
    }

    [Test]
    public async Task InvokeAsync_WithKeyNotFoundException_Returns404NotFound()
    {
        // Arrange
        RequestDelegate next = (HttpContext context) =>
        {
            throw new KeyNotFoundException("Resource not found");
        };

        _middleware = new GlobalExceptionHandler(next, _mockLogger.Object);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task InvokeAsync_WithGenericException_Returns500InternalServerError()
    {
        // Arrange
        RequestDelegate next = (HttpContext context) =>
        {
            throw new Exception("Something went wrong");
        };

        _middleware = new GlobalExceptionHandler(next, _mockLogger.Object);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task InvokeAsync_WithException_LogsError()
    {
        // Arrange
        var exception = new Exception("Test exception");
        RequestDelegate next = (HttpContext context) =>
        {
            throw exception;
        };

        _middleware = new GlobalExceptionHandler(next, _mockLogger.Object);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An unhandled exception occurred")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task InvokeAsync_WithException_ReturnsJsonResponse()
    {
        // Arrange
        RequestDelegate next = (HttpContext context) =>
        {
            throw new ArgumentException("Test error");
        };

        _middleware = new GlobalExceptionHandler(next, _mockLogger.Object);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(_httpContext.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        
        Assert.That(responseBody, Is.Not.Empty);
        
        var response = JsonSerializer.Deserialize<JsonElement>(responseBody);
        Assert.That(response.GetProperty("status").GetInt32(), Is.EqualTo(400));
        Assert.That(response.GetProperty("message").GetString(), Is.EqualTo("Test error"));
        Assert.That(response.TryGetProperty("timestamp", out _), Is.True);
        Assert.That(response.TryGetProperty("path", out _), Is.True);
    }
}