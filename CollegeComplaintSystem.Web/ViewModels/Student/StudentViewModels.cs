using CollegeComplaintSystem.Web.ViewModels.Complaints;

namespace CollegeComplaintSystem.Web.ViewModels.Student;

public class StudentDashboardViewModel
{
    public string StudentName { get; set; } = string.Empty;
    public string CollegeId { get; set; } = string.Empty;
    public int TotalComplaints { get; set; }
    public int SubmittedCount { get; set; }
    public int UnderReviewCount { get; set; }
    public int ResolvedCount { get; set; }
    public int ClosedCount { get; set; }
    public List<ComplaintListItemViewModel> RecentComplaints { get; set; } = new();
}

public class StudentProfileViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string CollegeId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public DateTime MemberSince { get; set; }
    public int TotalComplaintsSubmitted { get; set; }
}
