using System.Text;
using System.Text.Json;
using CollegeComplaintSystem.Web.Configuration;
using CollegeComplaintSystem.Web.Models.Enums;
using CollegeComplaintSystem.Web.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CollegeComplaintSystem.Web.Services.Implementations;

public class BrevoEmailService : IEmailService
{
    private readonly BrevoSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<BrevoEmailService> _logger;

    public BrevoEmailService(
        IOptions<BrevoSettings> settings,
        HttpClient httpClient,
        ILogger<BrevoEmailService> logger)
    {
        _settings = settings.Value;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task SendEmailVerificationAsync(
        string toEmail,
        string studentName,
        string verificationLink,
        CancellationToken cancellationToken = default)
    {
        var subject = "Verify your College Email - College Complaint Management System";
        var htmlContent = $@"
            <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 24px; border: 1px solid #E5E7EB; border-radius: 8px;"">
                <h2 style=""color: #111827; margin-bottom: 16px;"">Welcome to College Complaint Portal</h2>
                <p style=""color: #374151; font-size: 15px;"">Hello <strong>{studentName}</strong>,</p>
                <p style=""color: #374151; font-size: 15px; line-height: 1.5;"">
                    Thank you for registering. Please confirm your college email address to activate your account and start submitting complaints.
                </p>
                <div style=""margin: 28px 0;"">
                    <a href=""{verificationLink}"" 
                       style=""background-color: #111827; color: #ffffff; padding: 12px 24px; border-radius: 6px; text-decoration: none; font-weight: bold; display: inline-block;"">
                        Verify Email Address
                    </a>
                </div>
                <p style=""color: #6B7280; font-size: 13px; line-height: 1.4;"">
                    If the button above does not work, copy and paste this link into your browser:<br/>
                    <a href=""{verificationLink}"" style=""color: #2563EB; word-break: break-all;"">{verificationLink}</a>
                </p>
                <hr style=""border: 0; border-top: 1px solid #E5E7EB; margin: 24px 0;""/>
                <p style=""color: #9CA3AF; font-size: 12px;"">If you did not register for this account, you can safely ignore this email.</p>
            </div>";

        await SendEmailAsync(toEmail, studentName, subject, htmlContent, cancellationToken);
    }

    public async Task SendPasswordResetAsync(
        string toEmail,
        string studentName,
        string resetLink,
        CancellationToken cancellationToken = default)
    {
        var subject = "Reset Your Password - College Complaint Management System";
        var htmlContent = $@"
            <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 24px; border: 1px solid #E5E7EB; border-radius: 8px;"">
                <h2 style=""color: #111827; margin-bottom: 16px;"">Password Reset Request</h2>
                <p style=""color: #374151; font-size: 15px;"">Hello <strong>{studentName}</strong>,</p>
                <p style=""color: #374151; font-size: 15px; line-height: 1.5;"">
                    We received a request to reset your password. Click the button below to choose a new password.
                </p>
                <div style=""margin: 28px 0;"">
                    <a href=""{resetLink}"" 
                       style=""background-color: #111827; color: #ffffff; padding: 12px 24px; border-radius: 6px; text-decoration: none; font-weight: bold; display: inline-block;"">
                        Reset Password
                    </a>
                </div>
                <p style=""color: #6B7280; font-size: 13px;"">
                    Link: <a href=""{resetLink}"" style=""color: #2563EB; word-break: break-all;"">{resetLink}</a>
                </p>
                <hr style=""border: 0; border-top: 1px solid #E5E7EB; margin: 24px 0;""/>
                <p style=""color: #9CA3AF; font-size: 12px;"">If you didn't request a password reset, you can safely ignore this email.</p>
            </div>";

        await SendEmailAsync(toEmail, studentName, subject, htmlContent, cancellationToken);
    }

    public async Task SendStatusUpdateNotificationAsync(
        string toEmail,
        string studentName,
        string referenceNumber,
        string complaintTitle,
        ComplaintStatus newStatus,
        CancellationToken cancellationToken = default)
    {
        var statusDisplay = newStatus switch
        {
            ComplaintStatus.UnderReview => "Under Review",
            ComplaintStatus.Resolved => "Resolved",
            ComplaintStatus.Closed => "Closed",
            _ => newStatus.ToString()
        };

        var subject = $"Complaint {referenceNumber} is now {statusDisplay}";
        var htmlContent = $@"
            <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 24px; border: 1px solid #E5E7EB; border-radius: 8px;"">
                <h2 style=""color: #111827; margin-bottom: 16px;"">Complaint Status Update</h2>
                <p style=""color: #374151; font-size: 15px;"">Hello <strong>{studentName}</strong>,</p>
                <p style=""color: #374151; font-size: 15px; line-height: 1.5;"">
                    Your complaint status has been updated by the administration:
                </p>
                <div style=""background-color: #F8FAFC; border: 1px solid #E2E8F0; padding: 16px; border-radius: 6px; margin: 20px 0;"">
                    <p style=""margin: 0 0 8px 0;""><strong>Reference:</strong> {referenceNumber}</p>
                    <p style=""margin: 0 0 8px 0;""><strong>Title:</strong> {complaintTitle}</p>
                    <p style=""margin: 0;""><strong>New Status:</strong> <span style=""color: #2563EB; font-weight: bold;"">{statusDisplay}</span></p>
                </div>
                <p style=""color: #374151; font-size: 14px;"">You can log in to your student portal at any time to track updates.</p>
                <hr style=""border: 0; border-top: 1px solid #E5E7EB; margin: 24px 0;""/>
                <p style=""color: #9CA3AF; font-size: 12px;"">College Complaint Management System</p>
            </div>";

        await SendEmailAsync(toEmail, studentName, subject, htmlContent, cancellationToken);
    }

    private async Task SendEmailAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlContent,
        CancellationToken cancellationToken)
    {
        if (!_settings.IsConfigured)
        {
            _logger.LogWarning(
                "[Brevo Simulated Email] Brevo API Key not configured. Skipping external dispatch.\n" +
                "To: {ToEmail} ({ToName})\nSubject: {Subject}",
                toEmail, toName, subject);
            return;
        }

        try
        {
            var payload = new
            {
                sender = new { name = _settings.SenderName, email = _settings.SenderEmail },
                to = new[] { new { email = toEmail, name = toName } },
                subject = subject,
                htmlContent = htmlContent
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email")
            {
                Content = jsonContent
            };
            request.Headers.Add("api-key", _settings.ApiKey);
            request.Headers.Add("accept", "application/json");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Brevo API returned error {StatusCode}: {ErrorBody}", response.StatusCode, errorBody);
            }
            else
            {
                _logger.LogInformation("Brevo transactional email sent successfully to {ToEmail}", toEmail);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via Brevo to {ToEmail}", toEmail);
        }
    }
}
