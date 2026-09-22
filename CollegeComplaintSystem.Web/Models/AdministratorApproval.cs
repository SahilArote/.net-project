using CollegeComplaintSystem.Web.Models.Enums;

namespace CollegeComplaintSystem.Web.Models;

public class AdministratorApproval
{
    public int ApprovalId { get; set; }

    public required string AdminUserId { get; set; }

    public string? ApprovedByAdminId { get; set; }

    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DecidedAt { get; set; }

    public string? Notes { get; set; }

    // Navigation properties
    public virtual ApplicationUser AdminUser { get; set; } = null!;

    public virtual ApplicationUser? ApprovedByAdmin { get; set; }
}
