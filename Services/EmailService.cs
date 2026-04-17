using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using PharmacyApi.Models.DTOs;
using System;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace PharmacyApi.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        // ─────────────────────────────────────────────
        // Generic Email Sender
        // ─────────────────────────────────────────────
        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            try
            {
                var smtpSection = _config.GetSection("Smtp");

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(
                    smtpSection["SenderName"] ?? "PharmacyApp",
                    smtpSection["SenderEmail"] ?? "noreply@pharmacy.com"));

                message.To.Add(MailboxAddress.Parse(to));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlBody
                };

                message.Body = bodyBuilder.ToMessageBody();

                int port = int.TryParse(smtpSection["Port"], out var p) ? p : 587;

                using var client = new SmtpClient();

                await client.ConnectAsync(
                    smtpSection["Host"] ?? "smtp.gmail.com",
                    port,
                    SecureSocketOptions.StartTls);

                await client.AuthenticateAsync(
                    smtpSection["Username"],
                    smtpSection["Password"]);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent to {To} with subject '{Subject}'", to, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", to);
                throw;
            }
        }

        // ─────────────────────────────────────────────
        // Order Confirmation Email
        // ─────────────────────────────────────────────
        public async Task SendOrderConfirmationAsync(OrderConfirmationEmailDto dto)
        {
            var sb = new StringBuilder();

            sb.Append("""
<!DOCTYPE html>
<html>
<head>
  <style>
    body {{font - family: Arial, sans-serif; color: #333; }}
    .container {{max - width: 600px; margin: 0 auto; padding: 20px; }}
    .header {{background: #2d7d46; color: white; padding: 20px; border-radius: 8px 8px 0 0; }}
    .content {{background: #f9f9f9; padding: 20px; }}
    table {{width: 100%; border-collapse: collapse; margin-top: 16px; }}
    th, td {{padding: 10px; border: 1px solid #ddd; text-align: left; }}
    th {{background: #eef; }}
    .total {{font - weight: bold; font-size: 1.1em; }}
    .footer {{text - align: center; font-size: 0.85em; color: #888; margin-top: 20px; }}
  </style>
</head>
<body>
  <div class="container">
    <div class="header">
      <h2>Order Confirmation – #{dto.OrderId}</h2>
    </div>
    <div class="content">
      <p>Dear {dto.CustomerName},</p>
      <p>Thank you for your order! Here is a summary:</p>

      <table>
        <thead>
          <tr>
            <th>Product</th>
            <th>Qty</th>
            <th>Unit Price</th>
            <th>Subtotal</th>
          </tr>
        </thead>
        <tbody>
""");

            foreach (var item in dto.Items)
            {
                var subtotal = item.Quantity * item.UnitPrice;

                sb.Append($"""
          <tr>
            <td>{item.ProductName}</td>
            <td>{item.Quantity}</td>
            <td>₹{item.UnitPrice:F2}</td>
            <td>₹{subtotal:F2}</td>
          </tr>
""");
            }

            sb.Append($"""
        </tbody>
      </table>

      <p class="total">Total: ₹{dto.TotalAmount:F2}</p>
      <p>Order Date: {dto.OrderDate:dd MMM yyyy, hh:mm tt}</p>
      <p>We will notify you once your order is dispatched.</p>
    </div>

    <div class="footer">
      PharmacyApp &copy; {DateTime.UtcNow.Year}
    </div>
  </div>
</body>
</html>
""");

            await SendEmailAsync(
                dto.CustomerEmail,
                $"Order Confirmed – #{dto.OrderId}",
                sb.ToString());
        }
    }
}