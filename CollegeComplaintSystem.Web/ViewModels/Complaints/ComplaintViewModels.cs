using System.ComponentModel.DataAnnotations;
using CollegeComplaintSystem.Web.Models;
using CollegeComplaintSystem.Web.Models.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CollegeComplaintSystem.Web.ViewModels.Complaints;

public class ComplaintCreateViewModel
{
    [Required(ErrorMessage = "Complaint Title is required.")]
    [StringLength(150, ErrorMessage = "Title cannot exceed 150 characters.")]
    [Display(Name = "Complaint Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a category.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category.")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Location is required.")]
    [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters.")]
    [Display(Name = "Location (e.g. Main Building, Room 204)")]
    public string Location { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(4000, ErrorMessage = "Description cannot exceed 4000 characters.")]
    [DataType(DataType.MultilineText)]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Supporting Image (Optional, max 5MB, JPG/PNG/WEBP)")]
    public IFormFile? ImageFile { get; set; }

    public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
}

public class ComplaintEditViewModel
{
    public int ComplaintId { get; set; }

    public string ReferenceNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Complaint Title is required.")]
    [StringLength(150, ErrorMessage = "Title cannot exceed 150 characters.")]
    [Display(Name = "Complaint Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a category.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category.")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Location is required.")]
    [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters.")]
    [Display(Name = "Location (e.g. Main Building, Room 204)")]
    public string Location { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(4000, ErrorMessage = "Description cannot exceed 4000 characters.")]
    [DataType(DataType.MultilineText)]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    public string? ExistingImageUrl { get; set; }

    [Display(Name = "Replace Image (Optional, max 5MB, JPG/PNG/WEBP)")]
    public IFormFile? NewImageFile { get; set; }

    public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
}

public class ComplaintDetailsViewModel
{
    public int ComplaintId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ComplaintStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? ImageUrl { get; set; }
    public bool CanEditOrDelete => Status == ComplaintStatus.Submitted;
    public List<ComplaintStatusHistoryItemViewModel> StatusHistories { get; set; } = new();
}

public class ComplaintStatusHistoryItemViewModel
{
    public ComplaintStatus PreviousStatus { get; set; }
    public ComplaintStatus NewStatus { get; set; }
    public string? ChangedByName { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? Notes { get; set; }
}

public class ComplaintListViewModel
{
    public string? Search { get; set; }
    public ComplaintStatus? Status { get; set; }
    public int? CategoryId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalCount { get; set; }
    public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
    public List<ComplaintListItemViewModel> Items { get; set; } = new();
}

public class ComplaintListItemViewModel
{
    public int ComplaintId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public ComplaintStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool HasImage { get; set; }
}
