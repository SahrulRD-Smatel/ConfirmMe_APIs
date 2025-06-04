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

        //public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        //{
        //    var email = new MimeMessage();
        //    // Menggunakan FromName dan FromEmail
        //    email.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
        //    email.To.Add(MailboxAddress.Parse(toEmail));
        //    email.Subject = subject;
        //    email.Body = new TextPart(TextFormat.Html) { Text = htmlMessage };


        //    var policy = Policy
        //        .Handle<SmtpCommandException>()
        //        .Or<SmtpProtocolException>()
        //        .WaitAndRetryAsync(3,
        //            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        //            (exception, timeSpan, retryCount, context) =>
        //            {
        //                Console.WriteLine($"Retry {retryCount} failed: {exception.Message}");
        //            });

        //    try
        //    {
        //        await policy.ExecuteAsync(async () =>
        //        {
        //            using var smtp = new SmtpClient();

        //            // Jangan pake local endpoint binding, biar bebas cari jalan keluar jaringan
        //            smtp.LocalEndPoint = null;

        //            await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, SecureSocketOptions.StartTls);
        //            await smtp.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
        //            await smtp.SendAsync(email);
        //            await smtp.DisconnectAsync(true);
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log error to database if email sending fails after retries
        //        var emailLog = new EmailErrorLog
        //        {
        //            ToEmail = toEmail,
        //            Subject = subject,
        //            ErrorMessage = ex.Message,
        //            Timestamp = DateTime.UtcNow
        //        };
        //        _context.EmailErrorLogs.Add(emailLog);
        //        await _context.SaveChangesAsync();

        //        // Optionally, rethrow the exception to let the caller know it failed
        //        throw new Exception("Failed to send email after retries", ex);
        //    }
        //}



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
                .Handle<SmtpCommandException>()
                .Or<SmtpProtocolException>()
                .WaitAndRetryAsync(3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        Console.WriteLine($"Retry {retryCount} failed: {exception.Message}");
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
                var emailLog = new EmailErrorLog
                {
                    ToEmail = toEmail,
                    Subject = subject,
                    ErrorMessage = ex.Message,
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
            var subject = "Approval Status Updated";
            var message = $@"
                <p>Hi {fullName},</p>
                <p>The approval status for your request titled '{approvalRequestTitle}' has been updated to: {newStatus}</p>
                <p>Regards,<br/>ConfirmMe Team</p>";

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
