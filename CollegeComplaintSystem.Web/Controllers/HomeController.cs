using System.Diagnostics;
using CollegeComplaintSystem.Web.Common.Constants;
using CollegeComplaintSystem.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace CollegeComplaintSystem.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole(AppRoles.Administrator))
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            return RedirectToAction("Dashboard", "Student");
        }

        return View();
    }

    public IActionResult Offline()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
