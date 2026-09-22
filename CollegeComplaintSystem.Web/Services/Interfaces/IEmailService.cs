using CollegeComplaintSystem.Web.Models.Enums;

namespace CollegeComplaintSystem.Web.Services.Interfaces;

public interface IEmailService
{
    Task SendEmailVerificationAsync(string toEmail, string studentName, string verificationLink, CancellationToken cancellationToken = default);
    Task SendPasswordResetAsync(string toEmail, string studentName, string resetLink, CancellationToken cancellationToken = default);
    Task SendStatusUpdateNotificationAsync(string toEmail, string studentName, string referenceNumber, string complaintTitle, ComplaintStatus newStatus, CancellationToken cancellationToken = default);
}
