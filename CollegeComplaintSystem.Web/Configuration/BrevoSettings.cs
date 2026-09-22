namespace CollegeComplaintSystem.Web.Configuration;

public class BrevoSettings
{
    public const string SectionName = "Brevo";

    public string ApiKey { get; set; } = string.Empty;

    public string SenderEmail { get; set; } = "no-reply@college.edu";

    public string SenderName { get; set; } = "College Complaint Management System";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);
}
