using CollegeComplaintSystem.Web.ViewModels.Admin;
using CollegeComplaintSystem.Web.ViewModels.Reports;
using CollegeComplaintSystem.Web.ViewModels.Student;

namespace CollegeComplaintSystem.Web.Services.Interfaces;

public interface IStudentService
{
    Task<StudentDashboardViewModel> GetStudentDashboardAsync(string studentId, CancellationToken cancellationToken = default);
    Task<List<StudentListItemViewModel>> GetStudentsListAsync(string? search, CancellationToken cancellationToken = default);
    Task<StudentProfileViewModel?> GetStudentProfileAsync(string studentId, CancellationToken cancellationToken = default);
}

public interface IReportService
{
    Task<AdminDashboardViewModel> GetAdminDashboardAsync(CancellationToken cancellationToken = default);
    Task<ReportDashboardViewModel> GetReportDataAsync(CancellationToken cancellationToken = default);
}
