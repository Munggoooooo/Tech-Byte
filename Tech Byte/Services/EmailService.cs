using System.Net;
using System.Net.Mail;
using Tech_Byte.Models;

namespace Tech_Byte.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendActivationCodeAsync(string toEmail, string code)
        {
            var host = _config["SMTP:Host"];
            var port = int.Parse(_config["SMTP:Port"]);
            var username = _config["SMTP:Username"];
            var password = _config["SMTP:Password"];
            var fromEmail = _config["SMTP:FromEmail"];
            var fromName = _config["SMTP:FromName"];

            using (var client = new SmtpClient(host, port))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(username, password);

                var mail = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName), // Friendly From Name
                    Subject = "Activate Your TechByte Account",
                    IsBodyHtml = true // Important! Use HTML formatting
                };

                mail.To.Add(toEmail);

                // HTML body
                mail.Body = $@"
                    <div style='font-family:Arial,sans-serif; max-width:600px; margin:auto; padding:20px; border:1px solid #ddd; border-radius:8px;'>
                        <h2 style='color:#2c3e50;'>Welcome to TechByte!</h2>
                        <p>Hi there,</p>
                        <p>Thank you for registering. Your account activation code is:</p>
                        <h3 style='color:#27ae60;'>{code}</h3>
                        <p>Please enter this code on the activation page to verify your account.</p>
                        <hr />
                        <p style='font-size:12px; color:#7f8c8d;'>If you did not create this account, you can safely ignore this email.</p>
                    </div>
                ";

                await client.SendMailAsync(mail);
            }
        }

        public async Task SendPasswordResetAsync(string toEmail, string resetLink)
        {
            var host = _config["SMTP:Host"];
            var port = int.Parse(_config["SMTP:Port"]);
            var username = _config["SMTP:Username"];
            var password = _config["SMTP:Password"];
            var fromEmail = _config["SMTP:FromEmail"];
            var fromName = _config["SMTP:FromName"];

            using (var client = new SmtpClient(host, port))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(username, password);

                var mail = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = "Reset Your TechByte Password",
                    IsBodyHtml = true
                };

                mail.To.Add(toEmail);

                mail.Body = $@"
            <div style='font-family:Arial; max-width:600px; margin:auto;'>
                <h2>Password Reset Request</h2>
                <p>Click the link below to reset your password:</p>
                <a href='{resetLink}'
                   style='display:inline-block;background:#007bff;color:white;padding:10px 20px;
                          text-decoration:none;border-radius:5px;'>Reset Password</a>
                <p>This link expires in 30 minutes.</p>
            </div>
        ";

                await client.SendMailAsync(mail);
            }
        }

        public async Task SendContactMessageAsync(string adminEmail, ContactMessage contact)
        {
            var host = _config["SMTP:Host"];
            var port = int.Parse(_config["SMTP:Port"]);
            var username = _config["SMTP:Username"];
            var password = _config["SMTP:Password"];
            var fromEmail = _config["SMTP:FromEmail"];
            var fromName = _config["SMTP:FromName"];

            using (var client = new SmtpClient(host, port))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(username, password);

                var mail = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = $"New Contact Message from {contact.FromUsername}",
                    IsBodyHtml = true
                };

                mail.To.Add(adminEmail);

                mail.Body = $@"
            <div style='font-family:Arial; max-width:600px; margin:auto;'>
                <h3>New Contact Message</h3>
                <p><strong>From:</strong> {contact.FromUsername} ({contact.UserRole})</p>
                <p><strong>Email:</strong> {contact.Email}</p>
                <p><strong>Subject:</strong> {contact.Subject}</p>
                <p><strong>Message:</strong><br>{contact.Message}</p>
                <p><small>Sent at {contact.Timestamp:yyyy-MM-dd HH:mm:ss} UTC</small></p>
            </div>
        ";

                await client.SendMailAsync(mail);
            }
        }

        public async Task SendReplyToUserAsync(string toEmail, string fromAdmin, string replyMessage)
        {
            var host = _config["SMTP:Host"];
            var port = int.Parse(_config["SMTP:Port"]);
            var username = _config["SMTP:Username"];
            var password = _config["SMTP:Password"];
            var fromEmail = _config["SMTP:FromEmail"];
            var fromName = _config["SMTP:FromName"];

            using (var client = new SmtpClient(host, port))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(username, password);

                var mail = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromAdmin),
                    Subject = "Reply to your Contact Message",
                    IsBodyHtml = true
                };

                mail.To.Add(toEmail);

                mail.Body = $@"
            <div style='font-family:Arial; max-width:600px; margin:auto;'>
                <h3>Your message has been replied by admin</h3>
                <p>{replyMessage}</p>
                <hr />
                <p><small>Do not reply to this email directly. This is an automated message.</small></p>
            </div>
        ";

                await client.SendMailAsync(mail);
            }
        }

        public async Task SendPurchaseReceiptAsync(string toEmail, Purchase purchase)
        {
            var host = _config["SMTP:Host"];
            var port = int.Parse(_config["SMTP:Port"]);
            var username = _config["SMTP:Username"];
            var password = _config["SMTP:Password"];
            var fromEmail = _config["SMTP:FromEmail"];
            var fromName = _config["SMTP:FromName"];

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(username, password)
            };

            var mail = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = $"Your TechByte Receipt - Order {purchase.OrderId}",
                IsBodyHtml = true
            };

            mail.To.Add(toEmail);

            var itemsHtml = string.Join("", purchase.Items.Select(i =>
                $"<tr>" +
                $"<td>{i.Name}</td>" +
                $"<td>{i.Quantity}</td>" +
                $"<td>₱{i.Price:N2}</td>" +
                $"<td>₱{i.Total:N2}</td>" +
                $"</tr>"
            ));

            mail.Body = $@"
        <div style='font-family:Arial; max-width:600px; margin:auto; padding:20px;'>
            <h2>Thank you for your purchase, {purchase.CustomerUserName}!</h2>
            <p>Order ID: <strong>{purchase.OrderId}</strong></p>
            <p>Date: {purchase.Date.ToLocalTime():MMMM dd, yyyy hh:mm tt}</p>
            <table style='width:100%; border-collapse:collapse;'>
                <thead>
                    <tr>
                        <th>Product</th><th>Qty</th><th>Price</th><th>Subtotal</th>
                    </tr>
                </thead>
                <tbody>
                    {itemsHtml}
                </tbody>
                <tfoot>
                    <tr>
                        <td colspan='3' style='text-align:right; font-weight:bold;'>Total:</td>
                        <td style='font-weight:bold;'>₱{purchase.Total:N2}</td>
                    </tr>
                </tfoot>
            </table>
            <p>We hope you enjoy your purchase!</p>
        </div>";

            await client.SendMailAsync(mail);
        }

    }
}
