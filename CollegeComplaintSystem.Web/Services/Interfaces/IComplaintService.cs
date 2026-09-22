using CollegeComplaintSystem.Web.Models;
using CollegeComplaintSystem.Web.Models.Enums;
using CollegeComplaintSystem.Web.ViewModels.Admin;
using CollegeComplaintSystem.Web.ViewModels.Complaints;

namespace CollegeComplaintSystem.Web.Services.Interfaces;

public interface IComplaintService
{
    Task<(bool Success, string? Error, Complaint? Complaint)> CreateComplaintAsync(ComplaintCreateViewModel model, string studentId, CancellationToken cancellationToken = default);
    Task<ComplaintListViewModel> GetStudentComplaintsAsync(string studentId, string? search, ComplaintStatus? status, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Complaint?> GetComplaintForStudentAsync(int complaintId, string studentId, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> UpdateComplaintAsync(ComplaintEditViewModel model, string studentId, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> DeleteComplaintAsync(int complaintId, string studentId, CancellationToken cancellationToken = default);
    Task<AdminComplaintListViewModel> GetAdminComplaintsAsync(string? search, ComplaintStatus? status, int? categoryId, string? sortBy, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Complaint?> GetComplaintForAdminAsync(int complaintId, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> UpdateStatusAsync(int complaintId, ComplaintStatus newStatus, string adminUserId, string? notes, CancellationToken cancellationToken = default);
    bool IsValidStatusTransition(ComplaintStatus current, ComplaintStatus next);
}
