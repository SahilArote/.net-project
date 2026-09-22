using CollegeComplaintSystem.Web.Models.Enums;

namespace CollegeComplaintSystem.Web.Models;

public class ComplaintStatusHistory
{
    public int HistoryId { get; set; }

    public int ComplaintId { get; set; }

    public ComplaintStatus PreviousStatus { get; set; }

    public ComplaintStatus NewStatus { get; set; }

    public string? ChangedByUserId { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    public string? Notes { get; set; }

    // Navigation properties
    public virtual Complaint Complaint { get; set; } = null!;

    public virtual ApplicationUser? ChangedByUser { get; set; }
}
