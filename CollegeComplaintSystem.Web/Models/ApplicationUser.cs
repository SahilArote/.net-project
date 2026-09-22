using Microsoft.AspNetCore.Identity;

namespace CollegeComplaintSystem.Web.Models;

public class ApplicationUser : IdentityUser
{
    public required string FullName { get; set; }

    public required string CollegeId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
}
