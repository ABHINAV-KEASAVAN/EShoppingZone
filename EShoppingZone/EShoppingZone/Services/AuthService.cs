using BCrypt.Net;
using EShoppingZone.Models;
using EShoppingZone.Interfaces;
using EShoppingZone.Helpers;
using EShoppingZone.DTOs;

namespace EShoppingZone.Services;

/// <summary>
/// Service for authentication-related operations
/// </summary>
public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IEmailService _emailService;
    private readonly JwtTokenHelper _jwtHelper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository, 
        IWalletRepository walletRepository,
        IEmailService emailService,
        JwtTokenHelper jwtHelper,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _walletRepository = walletRepository;
        _emailService = emailService;
        _jwtHelper = jwtHelper;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user asynchronously
    /// </summary>
    /// <param name="request">Registration request data</param>
    /// <returns>JWT token for the registered user</returns>
    public async Task<string> RegisterAsync(RegisterRequest request)
    {
        try
        {
            _logger.LogInformation("Attempting to register user with email: {Email}", request.Email);
            
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration failed - user already exists: {Email}", request.Email);
                throw new ArgumentException("User already exists");
            }

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Address = request.Address
            };

            await _userRepository.CreateAsync(user);
            _logger.LogInformation("User registered successfully: {Email}", request.Email);
            
            // Create wallet for new user
            await _walletRepository.CreateAsync(new Wallet { UserId = user.UserId });
            _logger.LogInformation("Wallet created for user: {UserId}", user.UserId);

            // Send registration confirmation email
            try
            {
                await _emailService.SendRegistrationConfirmationEmailAsync(user.Email, user.Name);
            }
            catch (Exception emailEx)
            {
                _logger.LogWarning(emailEx, "Failed to send registration confirmation email to {Email}", user.Email);
                // Don't fail registration if email fails
            }

            return _jwtHelper.GenerateToken(user);
        }
        catch (Exception ex) when (!(ex is ArgumentException))
        {
            _logger.LogError(ex, "Error during user registration for email: {Email}", request.Email);
            throw;
        }
    }

    /// <summary>
    /// Authenticates a user and returns JWT token
    /// </summary>
    /// <param name="request">Login request data</param>
    /// <returns>JWT token for authenticated user</returns>
    public async Task<string> LoginAsync(LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", request.Email);
            
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed for email: {Email}", request.Email);
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            _logger.LogInformation("User logged in successfully: {Email}", request.Email);
            return _jwtHelper.GenerateToken(user);
        }
        catch (Exception ex) when (!(ex is UnauthorizedAccessException))
        {
            _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
            throw;
        }
    }

    /// <summary>
    /// Initiates password reset process
    /// </summary>
    /// <param name="email">User email address</param>
    /// <returns>Task representing the async operation</returns>
    public async Task RequestPasswordResetAsync(string email)
    {
        try
        {
            _logger.LogInformation("Password reset requested for email: {Email}", email);
            
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("Password reset requested for non-existent email: {Email}", email);
                // Don't reveal if user exists or not
                return;
            }

            // Generate reset token (in real app, store this in database with expiration)
            var resetToken = Guid.NewGuid().ToString("N")[..8].ToUpper();
            
            await _emailService.SendPasswordResetEmailAsync(email, resetToken);
            _logger.LogInformation("Password reset email sent to: {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset request for email: {Email}", email);
            throw;
        }
    }
}