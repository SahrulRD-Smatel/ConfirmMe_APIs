using System.Threading.Tasks;

namespace ConfirmMe.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
        Task SendVerificationEmailAsync(string toEmail, string fullName, string verificationLink);
        Task SendForgotPasswordEmailAsync(string toEmail, string fullName, string resetLink);
        Task SendApprovalStatusChangedAsync(string toEmail, string approvalRequestName, string newStatus, string recipientRole);
    }
}
