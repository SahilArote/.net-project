using CollegeComplaintSystem.Web.Common.Constants;
using CollegeComplaintSystem.Web.Services.Interfaces;
using CollegeComplaintSystem.Web.ViewModels.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollegeComplaintSystem.Web.Controllers;

[Authorize(Roles = AppRoles.Administrator)]
public class CategoryController : Controller
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoryController> _logger;

    public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetAllCategoriesAsync(includeInactive: true, cancellationToken);
        var viewModels = categories.Select(c => new CategoryViewModel
        {
            CategoryId = c.CategoryId,
            Name = c.Name,
            Description = c.Description,
            IsActive = c.IsActive,
            ComplaintCount = c.Complaints.Count
        }).ToList();

        return View(viewModels);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CategoryViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _categoryService.CreateCategoryAsync(model, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError(nameof(model.Name), result.Error ?? "Failed to create category.");
            return View(model);
        }

        TempData["SuccessMessage"] = $"Category '{model.Name}' created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var category = await _categoryService.GetByIdAsync(id, cancellationToken);
        if (category == null)
        {
            return NotFound("Category not found.");
        }

        var model = new CategoryViewModel
        {
            CategoryId = category.CategoryId,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            ComplaintCount = category.Complaints.Count
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CategoryViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _categoryService.UpdateCategoryAsync(model, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Failed to update category.");
            return View(model);
        }

        TempData["SuccessMessage"] = $"Category '{model.Name}' updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.ToggleActiveAsync(id, cancellationToken);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Error ?? "Failed to toggle category status.";
        }
        else
        {
            TempData["SuccessMessage"] = "Category status toggled successfully.";
        }

        return RedirectToAction(nameof(Index));
    }
}
