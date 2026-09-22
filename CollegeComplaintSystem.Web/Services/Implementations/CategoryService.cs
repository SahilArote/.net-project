using CollegeComplaintSystem.Web.Data;
using CollegeComplaintSystem.Web.Models;
using CollegeComplaintSystem.Web.Services.Interfaces;
using CollegeComplaintSystem.Web.ViewModels.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CollegeComplaintSystem.Web.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(ApplicationDbContext context, ILogger<CategoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ComplaintCategory>> GetAllCategoriesAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _context.ComplaintCategories.AsQueryable();
        if (!includeInactive)
        {
            query = query.Where(c => c.IsActive);
        }
        return await query.OrderBy(c => c.Name).ToListAsync(cancellationToken);
    }

    public async Task<ComplaintCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.ComplaintCategories
            .Include(c => c.Complaints)
            .FirstOrDefaultAsync(c => c.CategoryId == id, cancellationToken);
    }

    public async Task<(bool Success, string? Error)> CreateCategoryAsync(CategoryViewModel model, CancellationToken cancellationToken = default)
    {
        var normalizedName = model.Name.Trim();
        if (await _context.ComplaintCategories.AnyAsync(c => c.Name.ToLower() == normalizedName.ToLower(), cancellationToken))
        {
            return (false, $"A category named '{normalizedName}' already exists.");
        }

        var category = new ComplaintCategory
        {
            Name = normalizedName,
            Description = model.Description?.Trim(),
            IsActive = model.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.ComplaintCategories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Category created: {Name}", category.Name);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateCategoryAsync(CategoryViewModel model, CancellationToken cancellationToken = default)
    {
        var category = await _context.ComplaintCategories.FindAsync([model.CategoryId], cancellationToken);
        if (category == null)
        {
            return (false, "Category not found.");
        }

        var normalizedName = model.Name.Trim();
        if (await _context.ComplaintCategories.AnyAsync(c => c.CategoryId != model.CategoryId && c.Name.ToLower() == normalizedName.ToLower(), cancellationToken))
        {
            return (false, $"Another category named '{normalizedName}' already exists.");
        }

        category.Name = normalizedName;
        category.Description = model.Description?.Trim();
        category.IsActive = model.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Category updated: {CategoryId} - {Name}", category.CategoryId, category.Name);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ToggleActiveAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _context.ComplaintCategories.FindAsync([id], cancellationToken);
        if (category == null)
        {
            return (false, "Category not found.");
        }

        category.IsActive = !category.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Category {CategoryId} IsActive toggled to {IsActive}", id, category.IsActive);
        return (true, null);
    }
}
