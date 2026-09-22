using System.Security.Claims;
using CollegeComplaintSystem.Web.Common.Constants;
using CollegeComplaintSystem.Web.Models.Enums;
using CollegeComplaintSystem.Web.Services.Interfaces;
using CollegeComplaintSystem.Web.ViewModels.Admin;
using CollegeComplaintSystem.Web.ViewModels.Complaints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CollegeComplaintSystem.Web.Controllers;

[Authorize(Roles = AppRoles.Administrator)]
public class AdminController : Controller
{
    private readonly IComplaintService _complaintService;
    private readonly ICategoryService _categoryService;
    private readonly IStudentService _studentService;
    private readonly IReportService _reportService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IComplaintService complaintService,
        ICategoryService categoryService,
        IStudentService studentService,
        IReportService reportService,
        ILogger<AdminController> logger)
    {
        _complaintService = complaintService;
        _categoryService = categoryService;
        _studentService = studentService;
        _reportService = reportService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var dashboard = await _reportService.GetAdminDashboardAsync(cancellationToken);
        return View(dashboard);
    }

    [HttpGet]
    public async Task<IActionResult> Complaints(
        string? search,
        ComplaintStatus? status,
        int? categoryId,
        string? sortBy = "newest",
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        const int pageSize = 15;
        var model = await _complaintService.GetAdminComplaintsAsync(
            search, status, categoryId, sortBy, page, pageSize, cancellationToken);

        var categories = await _categoryService.GetAllCategoriesAsync(true, cancellationToken);
        model.Categories = categories.Select(c => new SelectListItem
        {
            Value = c.CategoryId.ToString(),
            Text = c.Name
        });

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var complaint = await _complaintService.GetComplaintForAdminAsync(id, cancellationToken);
        if (complaint == null)
        {
            return NotFound("Complaint record not found.");
        }

        var model = new AdminComplaintDetailsViewModel
        {
            ComplaintId = complaint.ComplaintId,
            ReferenceNumber = complaint.ReferenceNumber,
            StudentName = complaint.Student.FullName,
            CollegeId = complaint.Student.CollegeId,
            StudentEmail = complaint.Student.Email ?? string.Empty,
            CategoryName = complaint.Category.Name,
            Location = complaint.Location,
            Title = complaint.Title,
            Description = complaint.Description,
            ImageUrl = complaint.Images.FirstOrDefault()?.ImageUrl,
            CurrentStatus = complaint.Status,
            CreatedAt = complaint.CreatedAt,
            UpdatedAt = complaint.UpdatedAt,
            ResolvedAt = complaint.ResolvedAt,
            ClosedAt = complaint.ClosedAt,
            StatusHistories = complaint.StatusHistories
                .OrderBy(h => h.ChangedAt)
                .Select(h => new ComplaintStatusHistoryItemViewModel
                {
                    PreviousStatus = h.PreviousStatus,
                    NewStatus = h.NewStatus,
                    ChangedByName = h.ChangedByUser?.FullName ?? "System",
                    ChangedAt = h.ChangedAt,
                    Notes = h.Notes
                }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(
        int complaintId,
        ComplaintStatus newStatus,
        string? notes,
        CancellationToken cancellationToken)
    {
        var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(adminUserId)) return Challenge();

        var result = await _complaintService.UpdateStatusAsync(complaintId, newStatus, adminUserId, notes, cancellationToken);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Error;
            return RedirectToAction(nameof(Details), new { id = complaintId });
        }

        TempData["SuccessMessage"] = $"Complaint status successfully updated to '{newStatus}'.";
        return RedirectToAction(nameof(Details), new { id = complaintId });
    }

    [HttpGet]
    public async Task<IActionResult> Students(string? search, CancellationToken cancellationToken)
    {
        var students = await _studentService.GetStudentsListAsync(search, cancellationToken);
        ViewBag.Search = search;
        return View(students);
    }
}
