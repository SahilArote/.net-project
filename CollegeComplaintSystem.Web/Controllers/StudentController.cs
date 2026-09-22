using System.Security.Claims;
using CollegeComplaintSystem.Web.Common.Constants;
using CollegeComplaintSystem.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollegeComplaintSystem.Web.Controllers;

[Authorize(Roles = AppRoles.Student)]
public class StudentController : Controller
{
    private readonly IStudentService _studentService;
    private readonly ILogger<StudentController> _logger;

    public StudentController(IStudentService studentService, ILogger<StudentController> logger)
    {
        _studentService = studentService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(studentId))
        {
            return Challenge();
        }

        var dashboard = await _studentService.GetStudentDashboardAsync(studentId, cancellationToken);
        return View(dashboard);
    }

    [HttpGet]
    public async Task<IActionResult> Profile(CancellationToken cancellationToken)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(studentId))
        {
            return Challenge();
        }

        var profile = await _studentService.GetStudentProfileAsync(studentId, cancellationToken);
        if (profile == null)
        {
            return NotFound();
        }

        return View(profile);
    }
}
