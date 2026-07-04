using System.Net;
using System.Net.Mail;
using EShoppingZone.Interfaces;

namespace EShoppingZone.Services;

/// <summary>
/// Email service implementation using SMTP
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Sends order confirmation email to customer
    /// </summary>
    /// <param name="email">Customer email address</param>
    /// <param name="orderDetails">Order details</param>
    /// <returns>Task representing the async operation</returns>
    public async Task SendOrderConfirmationEmailAsync(string email, string orderDetails)
    {
        try
        {
            var subject = "Order Confirmation - EShoppingZone";
            var body = $@"
                <html>
                <body>
                    <h2>Thank you for your order!</h2>
                    <p>Your order has been confirmed and is being processed.</p>
                    <div>
                        <h3>Order Details:</h3>
                        <p>{orderDetails}</p>
                    </div>
                    <p>Thank you for shopping with EShoppingZone!</p>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
            _logger.LogInformation("Order confirmation email sent to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send order confirmation email to {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Sends password reset email to user
    /// </summary>
    /// <param name="email">User email address</param>
    /// <param name="resetToken">Password reset token</param>
    /// <returns>Task representing the async operation</returns>
    public async Task SendPasswordResetEmailAsync(string email, string resetToken)
    {
        try
        {
            var subject = "Password Reset - EShoppingZone";
            var body = $@"
                <html>
                <body>
                    <h2>Password Reset Request</h2>
                    <p>You have requested to reset your password.</p>
                    <p>Please use the following token to reset your password:</p>
                    <p><strong>{resetToken}</strong></p>
                    <p>If you did not request this, please ignore this email.</p>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
            _logger.LogInformation("Password reset email sent to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Sends registration confirmation email to new user
    /// </summary>
    /// <param name="email">User email address</param>
    /// <param name="userName">User name</param>
    /// <returns>Task representing the async operation</returns>
    public async Task SendRegistrationConfirmationEmailAsync(string email, string userName)
    {
        try
        {
            var subject = "Welcome to EShoppingZone!";
            var body = $@"
                <html>
                <body>
                    <h2>Welcome to EShoppingZone, {userName}!</h2>
                    <p>Thank you for registering with us.</p>
                    <p>You can now start shopping and enjoy our services.</p>
                    <p>Happy Shopping!</p>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
            _logger.LogInformation("Registration confirmation email sent to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send registration confirmation email to {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Sends email using SMTP configuration
    /// </summary>
    /// <param name="toEmail">Recipient email</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body</param>
    /// <returns>Task representing the async operation</returns>
    private async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var smtpHost = _configuration["Smtp:Host"];
        var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
        var smtpUsername = _configuration["Smtp:Username"];
        var smtpPassword = _configuration["Smtp:Password"];
        var enableSsl = bool.Parse(_configuration["Smtp:EnableSSL"] ?? "true");

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUsername, smtpPassword),
            EnableSsl = enableSsl
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(smtpUsername!, "EShoppingZone"),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mailMessage.To.Add(toEmail);

        await client.SendMailAsync(mailMessage);
    }
}