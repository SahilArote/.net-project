namespace CollegeComplaintSystem.Web.Models;

public class ComplaintCategory
{
    public int CategoryId { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
}
