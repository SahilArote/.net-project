using CollegeComplaintSystem.Web.Common.Constants;
using CollegeComplaintSystem.Web.Data;
using CollegeComplaintSystem.Web.Models;
using CollegeComplaintSystem.Web.Models.Enums;
using CollegeComplaintSystem.Web.Services.Interfaces;
using CollegeComplaintSystem.Web.ViewModels.Admin;
using CollegeComplaintSystem.Web.ViewModels.Complaints;
using CollegeComplaintSystem.Web.ViewModels.Reports;
using CollegeComplaintSystem.Web.ViewModels.Student;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CollegeComplaintSystem.Web.Services.Implementations;

public class StudentService : IStudentService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public StudentService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<StudentDashboardViewModel> GetStudentDashboardAsync(string studentId, CancellationToken cancellationToken = default)
    {
        var student = await _userManager.FindByIdAsync(studentId);
        var studentName = student?.FullName ?? "Student";
        var collegeId = student?.CollegeId ?? string.Empty;

        var counts = await _context.Complaints
            .Where(c => c.StudentId == studentId)
            .GroupBy(c => c.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var submittedCount = counts.FirstOrDefault(c => c.Status == ComplaintStatus.Submitted)?.Count ?? 0;
        var underReviewCount = counts.FirstOrDefault(c => c.Status == ComplaintStatus.UnderReview)?.Count ?? 0;
        var resolvedCount = counts.FirstOrDefault(c => c.Status == ComplaintStatus.Resolved)?.Count ?? 0;
        var closedCount = counts.FirstOrDefault(c => c.Status == ComplaintStatus.Closed)?.Count ?? 0;
        var totalComplaints = submittedCount + underReviewCount + resolvedCount + closedCount;

        var recentComplaints = await _context.Complaints
            .AsNoTracking()
            .Include(c => c.Category)
            .Include(c => c.Images)
            .Where(c => c.StudentId == studentId)
            .OrderByDescending(c => c.CreatedAt)
            .Take(5)
            .Select(c => new ComplaintListItemViewModel
            {
                ComplaintId = c.ComplaintId,
                ReferenceNumber = c.ReferenceNumber,
                Title = c.Title,
                CategoryName = c.Category.Name,
                Location = c.Location,
                Status = c.Status,
                CreatedAt = c.CreatedAt,
                HasImage = c.Images.Any()
            })
            .ToListAsync(cancellationToken);

        return new StudentDashboardViewModel
        {
            StudentName = studentName,
            CollegeId = collegeId,
            TotalComplaints = totalComplaints,
            SubmittedCount = submittedCount,
            UnderReviewCount = underReviewCount,
            ResolvedCount = resolvedCount,
            ClosedCount = closedCount,
            RecentComplaints = recentComplaints
        };
    }

    public async Task<List<StudentListItemViewModel>> GetStudentsListAsync(string? search, CancellationToken cancellationToken = default)
    {
        // Get all users in the Student role
        var studentUsers = await _userManager.GetUsersInRoleAsync(AppRoles.Student);
        var query = studentUsers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(u => u.FullName.ToLower().Contains(term) ||
                                     u.CollegeId.ToLower().Contains(term) ||
                                     (u.Email != null && u.Email.ToLower().Contains(term)));
        }

        var studentIds = query.Select(u => u.Id).ToList();

        // Get complaint counts per student in a single query
        var complaintCounts = await _context.Complaints
            .Where(c => studentIds.Contains(c.StudentId))
            .GroupBy(c => c.StudentId)
            .Select(g => new { StudentId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.StudentId, x => x.Count, cancellationToken);

        return query
            .OrderByDescending(u => u.CreatedAt)
            .AsEnumerable()
            .Select(u => new StudentListItemViewModel
            {
                Id = u.Id,
                FullName = u.FullName,
                CollegeId = u.CollegeId,
                Email = u.Email ?? string.Empty,
                EmailConfirmed = u.EmailConfirmed,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                TotalComplaints = complaintCounts.TryGetValue(u.Id, out var count) ? count : 0
            })
            .ToList();
    }

    public async Task<StudentProfileViewModel?> GetStudentProfileAsync(string studentId, CancellationToken cancellationToken = default)
    {
        var student = await _userManager.FindByIdAsync(studentId);
        if (student == null) return null;

        var totalComplaints = await _context.Complaints
            .CountAsync(c => c.StudentId == studentId, cancellationToken);

        return new StudentProfileViewModel
        {
            FullName = student.FullName,
            CollegeId = student.CollegeId,
            Email = student.Email ?? string.Empty,
            EmailConfirmed = student.EmailConfirmed,
            MemberSince = student.CreatedAt,
            TotalComplaintsSubmitted = totalComplaints
        };
    }
}

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReportService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<AdminDashboardViewModel> GetAdminDashboardAsync(CancellationToken cancellationToken = default)
    {
        var counts = await _context.Complaints
            .GroupBy(c => c.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var submittedCount = counts.FirstOrDefault(c => c.Status == ComplaintStatus.Submitted)?.Count ?? 0;
        var underReviewCount = counts.FirstOrDefault(c => c.Status == ComplaintStatus.UnderReview)?.Count ?? 0;
        var resolvedCount = counts.FirstOrDefault(c => c.Status == ComplaintStatus.Resolved)?.Count ?? 0;
        var closedCount = counts.FirstOrDefault(c => c.Status == ComplaintStatus.Closed)?.Count ?? 0;
        var totalComplaints = submittedCount + underReviewCount + resolvedCount + closedCount;

        var students = await _userManager.GetUsersInRoleAsync(AppRoles.Student);

        var recentComplaints = await _context.Complaints
            .AsNoTracking()
            .Include(c => c.Student)
            .Include(c => c.Category)
            .OrderByDescending(c => c.CreatedAt)
            .Take(5)
            .Select(c => new AdminComplaintListItemViewModel
            {
                ComplaintId = c.ComplaintId,
                ReferenceNumber = c.ReferenceNumber,
                StudentName = c.Student.FullName,
                CollegeId = c.Student.CollegeId,
                CategoryName = c.Category.Name,
                Title = c.Title,
                Status = c.Status,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new AdminDashboardViewModel
        {
            TotalComplaints = totalComplaints,
            SubmittedCount = submittedCount,
            UnderReviewCount = underReviewCount,
            ResolvedCount = resolvedCount,
            ClosedCount = closedCount,
            TotalStudents = students.Count,
            RecentComplaints = recentComplaints
        };
    }

    public async Task<ReportDashboardViewModel> GetReportDataAsync(CancellationToken cancellationToken = default)
    {
        var counts = await _context.Complaints
            .GroupBy(c => c.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var submittedCount = counts.FirstOrDefault(c => c.Status == ComplaintStatus.Submitted)?.Count ?? 0;
        var underReviewCount = counts.FirstOrDefault(c => c.Status == ComplaintStatus.UnderReview)?.Count ?? 0;
        var resolvedCount = counts.FirstOrDefault(c => c.Status == ComplaintStatus.Resolved)?.Count ?? 0;
        var closedCount = counts.FirstOrDefault(c => c.Status == ComplaintStatus.Closed)?.Count ?? 0;
        var totalComplaints = submittedCount + underReviewCount + resolvedCount + closedCount;

        // Breakdown by Category
        var categoryData = await _context.Complaints
            .Include(c => c.Category)
            .GroupBy(c => c.Category.Name)
            .Select(g => new
            {
                CategoryName = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        var categoryReport = categoryData.Select(c => new CategoryReportItem
        {
            CategoryName = c.CategoryName,
            Count = c.Count,
            Percentage = totalComplaints > 0 ? Math.Round(((double)c.Count / totalComplaints) * 100, 1) : 0
        }).ToList();

        // Monthly trends for last 6 months
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-5);
        var startOfRange = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var recentComplaints = await _context.Complaints
            .Where(c => c.CreatedAt >= startOfRange)
            .Select(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        var monthlyTrends = new List<MonthlyReportItem>();
        for (int i = 5; i >= 0; i--)
        {
            var targetMonth = DateTime.UtcNow.AddMonths(-i);
            var monthName = targetMonth.ToString("MMM yyyy");
            var count = recentComplaints.Count(d => d.Year == targetMonth.Year && d.Month == targetMonth.Month);
            monthlyTrends.Add(new MonthlyReportItem
            {
                MonthYear = monthName,
                Count = count
            });
        }

        return new ReportDashboardViewModel
        {
            TotalComplaints = totalComplaints,
            SubmittedCount = submittedCount,
            UnderReviewCount = underReviewCount,
            ResolvedCount = resolvedCount,
            ClosedCount = closedCount,
            ComplaintsByCategory = categoryReport,
            MonthlyTrend = monthlyTrends
        };
    }
}
