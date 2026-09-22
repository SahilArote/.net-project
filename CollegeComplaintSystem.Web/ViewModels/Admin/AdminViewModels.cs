using CollegeComplaintSystem.Web.Models.Enums;
using CollegeComplaintSystem.Web.ViewModels.Complaints;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CollegeComplaintSystem.Web.ViewModels.Admin;

public class AdminDashboardViewModel
{
    public int TotalComplaints { get; set; }
    public int SubmittedCount { get; set; }
    public int UnderReviewCount { get; set; }
    public int ResolvedCount { get; set; }
    public int ClosedCount { get; set; }
    public int TotalStudents { get; set; }
    public List<AdminComplaintListItemViewModel> RecentComplaints { get; set; } = new();
}

public class AdminComplaintListItemViewModel
{
    public int ComplaintId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string CollegeId { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public ComplaintStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminComplaintListViewModel
{
    public string? Search { get; set; }
    public ComplaintStatus? Status { get; set; }
    public int? CategoryId { get; set; }
    public string? SortBy { get; set; } = "newest";
    public int PageNumber { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalCount { get; set; }
    public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
    public List<AdminComplaintListItemViewModel> Items { get; set; } = new();
}

public class AdminComplaintDetailsViewModel
{
    public int ComplaintId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string CollegeId { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public ComplaintStatus CurrentStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public List<ComplaintStatusHistoryItemViewModel> StatusHistories { get; set; } = new();

    // Allowed transition logic
    public bool CanMarkUnderReview => CurrentStatus == ComplaintStatus.Submitted;
    public bool CanMarkResolved => CurrentStatus == ComplaintStatus.UnderReview;
    public bool CanClose => CurrentStatus == ComplaintStatus.Resolved;
}

public class StudentListItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string CollegeId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalComplaints { get; set; }
}
