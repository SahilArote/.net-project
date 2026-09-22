using CollegeComplaintSystem.Web.Common.Constants;
using CollegeComplaintSystem.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollegeComplaintSystem.Web.Controllers;

[Authorize(Roles = AppRoles.Administrator)]
public class ReportController : Controller
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await _reportService.GetReportDataAsync(cancellationToken);
        return View(model);
    }
}
