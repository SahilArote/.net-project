using System.ComponentModel.DataAnnotations;

namespace CollegeComplaintSystem.Web.ViewModels.Categories;

public class CategoryViewModel
{
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Category Name is required.")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    [Display(Name = "Category Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    [DataType(DataType.MultilineText)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public int ComplaintCount { get; set; }
}
