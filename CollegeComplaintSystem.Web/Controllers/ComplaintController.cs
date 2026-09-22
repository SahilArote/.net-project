using System.Security.Claims;
using CollegeComplaintSystem.Web.Common.Constants;
using CollegeComplaintSystem.Web.Models.Enums;
using CollegeComplaintSystem.Web.Services.Interfaces;
using CollegeComplaintSystem.Web.ViewModels.Complaints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CollegeComplaintSystem.Web.Controllers;

[Authorize(Roles = $"{AppRoles.Student},{AppRoles.Administrator}")]
public class ComplaintController : Controller
{
    private readonly IComplaintService _complaintService;
    private readonly ICategoryService _categoryService;
    private readonly ILogger<ComplaintController> _logger;

    public ComplaintController(
        IComplaintService complaintService,
        ICategoryService categoryService,
        ILogger<ComplaintController> logger)
    {
        _complaintService = complaintService;
        _categoryService = categoryService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? search,
        ComplaintStatus? status,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(studentId)) return Challenge();

        const int pageSize = 10;
        var model = await _complaintService.GetStudentComplaintsAsync(studentId, search, status, page, pageSize, cancellationToken);

        var categories = await _categoryService.GetAllCategoriesAsync(false, cancellationToken);
        model.Categories = categories.Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.Name });

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new ComplaintCreateViewModel
        {
            Categories = await GetCategorySelectListAsync(cancellationToken)
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ComplaintCreateViewModel model, CancellationToken cancellationToken)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(studentId)) return Challenge();

        if (!ModelState.IsValid)
        {
            model.Categories = await GetCategorySelectListAsync(cancellationToken);
            return View(model);
        }

        var result = await _complaintService.CreateComplaintAsync(model, studentId, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Failed to create complaint.");
            model.Categories = await GetCategorySelectListAsync(cancellationToken);
            return View(model);
        }

        TempData["SuccessMessage"] = $"Complaint submitted successfully! Your reference number is {result.Complaint!.ReferenceNumber}.";
        return RedirectToAction(nameof(Details), new { id = result.Complaint.ComplaintId });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(studentId)) return Challenge();

        var complaint = await _complaintService.GetComplaintForStudentAsync(id, studentId, cancellationToken);
        if (complaint == null)
        {
            return NotFound("Complaint not found or you do not have permission to view it.");
        }

        var model = new ComplaintDetailsViewModel
        {
            ComplaintId = complaint.ComplaintId,
            ReferenceNumber = complaint.ReferenceNumber,
            Title = complaint.Title,
            CategoryName = complaint.Category.Name,
            Location = complaint.Location,
            Description = complaint.Description,
            Status = complaint.Status,
            CreatedAt = complaint.CreatedAt,
            UpdatedAt = complaint.UpdatedAt,
            ResolvedAt = complaint.ResolvedAt,
            ClosedAt = complaint.ClosedAt,
            ImageUrl = complaint.Images.FirstOrDefault()?.ImageUrl,
            StatusHistories = complaint.StatusHistories
                .OrderBy(h => h.ChangedAt)
                .Select(h => new ComplaintStatusHistoryItemViewModel
                {
                    PreviousStatus = h.PreviousStatus,
                    NewStatus = h.NewStatus,
                    ChangedByName = h.ChangedByUser?.FullName ?? "Student",
                    ChangedAt = h.ChangedAt,
                    Notes = h.Notes
                }).ToList()
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(studentId)) return Challenge();

        var complaint = await _complaintService.GetComplaintForStudentAsync(id, studentId, cancellationToken);
        if (complaint == null)
        {
            return NotFound("Complaint not found.");
        }

        // Enforce Submitted-only rule
        if (complaint.Status != ComplaintStatus.Submitted)
        {
            TempData["ErrorMessage"] = $"Complaint cannot be edited because it is currently '{complaint.Status}'. Only 'Submitted' complaints can be modified.";
            return RedirectToAction(nameof(Details), new { id = complaint.ComplaintId });
        }

        var model = new ComplaintEditViewModel
        {
            ComplaintId = complaint.ComplaintId,
            ReferenceNumber = complaint.ReferenceNumber,
            Title = complaint.Title,
            CategoryId = complaint.CategoryId,
            Location = complaint.Location,
            Description = complaint.Description,
            ExistingImageUrl = complaint.Images.FirstOrDefault()?.ImageUrl,
            Categories = await GetCategorySelectListAsync(cancellationToken)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ComplaintEditViewModel model, CancellationToken cancellationToken)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(studentId)) return Challenge();

        if (!ModelState.IsValid)
        {
            model.Categories = await GetCategorySelectListAsync(cancellationToken);
            return View(model);
        }

        var result = await _complaintService.UpdateComplaintAsync(model, studentId, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Failed to update complaint.");
            model.Categories = await GetCategorySelectListAsync(cancellationToken);
            return View(model);
        }

        TempData["SuccessMessage"] = "Complaint updated successfully.";
        return RedirectToAction(nameof(Details), new { id = model.ComplaintId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(studentId)) return Challenge();

        var result = await _complaintService.DeleteComplaintAsync(id, studentId, cancellationToken);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Error ?? "Failed to delete complaint.";
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["SuccessMessage"] = "Complaint deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<IEnumerable<SelectListItem>> GetCategorySelectListAsync(CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetAllCategoriesAsync(false, cancellationToken);
        return categories.Select(c => new SelectListItem
        {
            Value = c.CategoryId.ToString(),
            Text = c.Name
        });
    }
}
