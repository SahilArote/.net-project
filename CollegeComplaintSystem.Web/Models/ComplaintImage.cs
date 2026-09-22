namespace CollegeComplaintSystem.Web.Models;

public class ComplaintImage
{
    public int ImageId { get; set; }

    public int ComplaintId { get; set; }

    public required string CloudinaryPublicId { get; set; }

    public required string ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual Complaint Complaint { get; set; } = null!;
}
