using CollegeComplaintSystem.Web.Models;
using CollegeComplaintSystem.Web.ViewModels.Categories;

namespace CollegeComplaintSystem.Web.Services.Interfaces;

public interface ICategoryService
{
    Task<List<ComplaintCategory>> GetAllCategoriesAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<ComplaintCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> CreateCategoryAsync(CategoryViewModel model, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> UpdateCategoryAsync(CategoryViewModel model, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> ToggleActiveAsync(int id, CancellationToken cancellationToken = default);
}
