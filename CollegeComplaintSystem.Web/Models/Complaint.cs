using CollegeComplaintSystem.Web.Models.Enums;

namespace CollegeComplaintSystem.Web.Models;

public class Complaint
{
    public int ComplaintId { get; set; }

    public required string ReferenceNumber { get; set; }

    public required string StudentId { get; set; }

    public int CategoryId { get; set; }

    public required string Title { get; set; }

    public required string Description { get; set; }

    public required string Location { get; set; }

    public ComplaintStatus Status { get; set; } = ComplaintStatus.Submitted;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    // Navigation properties
    public virtual ApplicationUser Student { get; set; } = null!;

    public virtual ComplaintCategory Category { get; set; } = null!;

    public virtual ICollection<ComplaintImage> Images { get; set; } = new List<ComplaintImage>();

    public virtual ICollection<ComplaintStatusHistory> StatusHistories { get; set; } = new List<ComplaintStatusHistory>();
}
