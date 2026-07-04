namespace EShoppingZone.Interfaces;

/// <summary>
/// Interface for email service operations
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends order confirmation email to customer
    /// </summary>
    /// <param name="email">Customer email address</param>
    /// <param name="orderDetails">Order details</param>
    /// <returns>Task representing the async operation</returns>
    Task SendOrderConfirmationEmailAsync(string email, string orderDetails);

    /// <summary>
    /// Sends password reset email to user
    /// </summary>
    /// <param name="email">User email address</param>
    /// <param name="resetToken">Password reset token</param>
    /// <returns>Task representing the async operation</returns>
    Task SendPasswordResetEmailAsync(string email, string resetToken);

    /// <summary>
    /// Sends registration confirmation email to new user
    /// </summary>
    /// <param name="email">User email address</param>
    /// <param name="userName">User name</param>
    /// <returns>Task representing the async operation</returns>
    Task SendRegistrationConfirmationEmailAsync(string email, string userName);
}