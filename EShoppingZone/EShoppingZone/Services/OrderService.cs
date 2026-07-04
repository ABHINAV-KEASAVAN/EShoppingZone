using EShoppingZone.Models;
using EShoppingZone.Interfaces;
using EShoppingZone.DTOs;

namespace EShoppingZone.Services;

/// <summary>
/// Service for order-related business operations
/// </summary>
public class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository, 
        ICartRepository cartRepository, 
        IWalletRepository walletRepository, 
        IProductRepository productRepository,
        IUserRepository userRepository,
        IEmailService emailService,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _walletRepository = walletRepository;
        _productRepository = productRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new order from user's cart
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Order request data</param>
    /// <returns>Created order</returns>
    public async Task<Order> CreateOrderAsync(int userId, OrderRequestDTO request)
    {
        try
        {
            _logger.LogInformation("Creating order for user {UserId} with payment method {PaymentMethod}", 
                userId, request.PaymentMethod);
            
            var cart = await _cartRepository.GetByUserIdAsync(userId);
            if (cart == null || !cart.CartItems.Any())
            {
                _logger.LogWarning("Order creation failed - empty cart for user {UserId}", userId);
                throw new ArgumentException("Cart is empty");
            }

            var totalAmount = cart.CartItems.Sum(ci => ci.Product.Price * ci.Quantity);
            _logger.LogInformation("Order total amount: {TotalAmount}", totalAmount);

            if (request.PaymentMethod == "Wallet")
            {
                var wallet = await _walletRepository.GetByUserIdAsync(userId);
                if (wallet == null || wallet.Balance < totalAmount)
                {
                    _logger.LogWarning("Insufficient wallet balance for user {UserId}. Required: {Required}, Available: {Available}", 
                        userId, totalAmount, wallet?.Balance ?? 0);
                    throw new ArgumentException("Insufficient wallet balance");
                }

                wallet.Balance -= totalAmount;
                await _walletRepository.UpdateAsync(wallet);
                _logger.LogInformation("Wallet balance updated for user {UserId}", userId);
            }

            // Snapshot to avoid collection-modified exception during removal
            var cartItemsSnapshot = cart.CartItems.ToList();

            // Validate stock before creating order
            foreach (var cartItem in cartItemsSnapshot)
            {
                if (cartItem.Product.Stock < cartItem.Quantity)
                {
                    _logger.LogWarning("Insufficient stock for product {ProductId}. Required: {Required}, Available: {Available}",
                        cartItem.ProductId, cartItem.Quantity, cartItem.Product.Stock);
                    throw new ArgumentException($"Insufficient stock for product {cartItem.Product.Name}");
                }
            }

            var order = new Order
            {
                UserId = userId,
                TotalAmount = totalAmount,
                PaymentMethod = request.PaymentMethod,
                Status = "Confirmed"
            };

            foreach (var cartItem in cartItemsSnapshot)
            {
                order.OrderItems.Add(new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.Product.Price
                });

                // Use already-tracked product — avoids EF duplicate tracking conflict
                cartItem.Product.Stock -= cartItem.Quantity;
                await _productRepository.UpdateAsync(cartItem.Product);
                _logger.LogInformation("Stock updated for product {ProductId}", cartItem.ProductId);
            }

            var createdOrder = await _orderRepository.CreateAsync(order);
            _logger.LogInformation("Order created with ID {OrderId}", createdOrder.OrderId);

            // Clear cart
            foreach (var item in cartItemsSnapshot)
            {
                await _cartRepository.RemoveItemAsync(item.CartItemId);
            }
            _logger.LogInformation("Cart cleared for user {UserId}", userId);

            // Send order confirmation email
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user != null)
                {
                    var orderDetails = $"Order ID: {createdOrder.OrderId}\nTotal Amount: ${totalAmount:F2}\nPayment Method: {request.PaymentMethod}";
                    await _emailService.SendOrderConfirmationEmailAsync(user.Email, orderDetails);
                }
            }
            catch (Exception emailEx)
            {
                _logger.LogWarning(emailEx, "Failed to send order confirmation email for order {OrderId}", createdOrder.OrderId);
                // Don't fail order creation if email fails
            }

            return createdOrder;
        }
        catch (Exception ex) when (!(ex is ArgumentException))
        {
            _logger.LogError(ex, "Error creating order for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Gets all orders for a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Collection of user orders</returns>
    public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Retrieving orders for user {UserId}", userId);
            return await _orderRepository.GetByUserIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders for user {UserId}", userId);
            throw;
        }
    }
}