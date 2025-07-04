using ConfirmMe.Data;
using ConfirmMe.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using Polly;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ConfirmMe.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly AppDbContext _context;


        public EmailService(IOptions<EmailSettings> emailSettings, AppDbContext context)
        {
            _emailSettings = emailSettings.Value;
            _context = context;
        }





        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = htmlMessage,
                TextBody = "Email ini membutuhkan tampilan HTML. Silakan buka melalui aplikasi email yang mendukung HTML."
            };

            email.Body = builder.ToMessageBody();

            var policy = Policy
    .Handle<Exception>() // tangkap semua, bukan hanya SmtpCommand/Protocol
    .WaitAndRetryAsync(3,
        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        (exception, timeSpan, retryCount, context) =>
        {
            Console.WriteLine($"Retry {retryCount} failed: {exception.GetType().Name} - {exception.Message}");
        });


            try
            {
                await policy.ExecuteAsync(async () =>
                {
                    using var smtp = new SmtpClient();
                    smtp.LocalEndPoint = null;
                    await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, SecureSocketOptions.StartTls);
                    await smtp.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                    await smtp.SendAsync(email);
                    await smtp.DisconnectAsync(true);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ EMAIL ERROR: {ex.GetType().Name} - {ex.Message}");
                var emailLog = new EmailErrorLog
                {
                    ToEmail = toEmail,
                    Subject = subject,
                    ErrorMessage = ex.ToString(),
                    Timestamp = DateTime.UtcNow
                };
                _context.EmailErrorLogs.Add(emailLog);
                await _context.SaveChangesAsync();

                throw new Exception("Failed to send email after retries", ex);
            }
        }


        public async Task SendVerificationEmailAsync(string toEmail, string fullName, string verificationLink)
        {
            var subject = "ConfirmMe - Email Verification";
            var message = $@"
    <html>
    <body style='font-family: Arial, sans-serif; background-color: #f9f9f9; padding: 20px;'>
        <div style='max-width: 600px; margin: auto; background: #ffffff; padding: 20px; border-radius: 10px; box-shadow: 0px 0px 10px rgba(0,0,0,0.1); text-align: center;'>
            <h2 style='color: #333;'>Welcome to ConfirmMe!</h2>
            <p>Hi {fullName},</p>
            <p>Thank you for registering. Please verify your email by clicking the button below:</p>
            <p>
                <a href='{verificationLink}' 
                   style='display: inline-block; padding: 12px 24px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                   Verify Email
                </a>
            </p>
            <p style='margin-top: 20px;'>If you have any questions, feel free to reach out to us.</p>
            <p style='margin-top: 30px; font-size: 12px; color: #777;'>&copy; 2025 ConfirmMe, All Rights Reserved</p>
        </div>
    </body>
    </html>";
            await SendEmailAsync(toEmail, subject, message);
        }


        public async Task SendForgotPasswordEmailAsync(string toEmail, string fullName, string resetLink)
        {
            var subject = "ConfirmMe - Reset Password";
            var message = $@"
                <p>Hi {fullName},</p>
                <p>You requested to reset your password. Click the link below to proceed:</p>
                <p><a href='{resetLink}'>Reset Password</a></p>
                <p>If you didn't request this, you can ignore this email.</p>
                <p>Regards,<br/>ConfirmMe Team</p>";
            await SendEmailAsync(toEmail, subject, message);
        }

        public async Task SendApprovalStatusChangedAsync(string toEmail, string fullName, string approvalRequestTitle, string newStatus)
        {
            var subject = $"[ConfirmMe] Approval Request - {newStatus}";

            var statusColor = newStatus switch
            {
                "Approved" => "#2ecc71",        // green
                "Rejected" => "#e74c3c",        // red
                "Fully Approved" => "#3498db",  // blue
                "Completed" => "#3498db",
                _ => "#999999"
            };

            var message = $@"
                <html>
                <body style='font-family: Arial, sans-serif; background-color: #f5f5f5; padding: 20px;'>
                    <div style='max-width: 600px; margin: auto; background: #ffffff; padding: 30px; border-radius: 10px; box-shadow: 0 2px 8px rgba(0,0,0,0.05);'>

                        <div style='text-align: center; margin-bottom: 30px;'>
                            <img src='https://yourdomain.com/logo.png' alt='ConfirmMe Logo' style='height: 50px;' />
                        </div>

                        <h2 style='color: #333;'>Hello, {fullName}</h2>
                        <p>This is an update about your approval request:</p>
                        <p><strong>Title:</strong> {approvalRequestTitle}</p>
                        <p><strong>Status:</strong> <span style='color: {statusColor}; font-weight: bold;'>{newStatus}</span></p>

                        <p style='margin-top: 20px;'>You can view the details by logging into the ConfirmMe platform.</p>

                        <div style='text-align: center; margin-top: 30px;'>
                            <a href='https://your-app-url.com/approval-requests' 
                               style='background-color: #4CAF50; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                                View Request
                            </a>
                        </div>

                        <p style='margin-top: 40px; font-size: 12px; color: #888; text-align: center;'>
                            This is an automated message from ConfirmMe System. Please do not reply directly to this email.<br/>
                            &copy; 2025 ConfirmMe, All rights reserved.
                        </p>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, message);
        }


        public static async Task CheckSmtpConnectivity()
        {
            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync("smtp.gmail.com", 587);
                Console.WriteLine("SMTP server reachable!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SMTP connect error: {ex.Message}");
            }
        }
    }
}
