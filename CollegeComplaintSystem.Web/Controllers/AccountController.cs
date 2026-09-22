using CollegeComplaintSystem.Web.Common.Constants;
using CollegeComplaintSystem.Web.Models;
using CollegeComplaintSystem.Web.Services.Interfaces;
using CollegeComplaintSystem.Web.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegeComplaintSystem.Web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IEmailService emailService,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole(AppRoles.Administrator))
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            return RedirectToAction("Dashboard", "Student");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email.Trim());
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid login credentials.");
            return View(model);
        }

        if (!user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Your account has been deactivated. Please contact the college administration.");
            return View(model);
        }

        if (!user.EmailConfirmed)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var devVerifyUrl = Url.Action(nameof(VerifyEmail), "Account", new { userId = user.Id, token }, Request.Scheme);
            ViewBag.DevVerifyUrl = devVerifyUrl;
            ViewBag.UnconfirmedEmail = user.Email;
            ModelState.AddModelError(string.Empty, "Please verify your college email address before logging in. Check your inbox for the verification link.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} logged in successfully.", user.Email);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(AppRoles.Administrator))
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            return RedirectToAction("Dashboard", "Student");
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User account {Email} locked out.", user.Email);
            ModelState.AddModelError(string.Empty, "Account locked due to multiple failed attempts. Please try again after 15 minutes.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Invalid login credentials.");
        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Dashboard", "Student");
        }
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var email = model.Email.Trim();
        var collegeId = model.CollegeId.Trim();

        // 1. Check duplicate email
        var existingEmail = await _userManager.FindByEmailAsync(email);
        if (existingEmail != null)
        {
            ModelState.AddModelError(nameof(model.Email), "An account with this email address already exists.");
            return View(model);
        }

        // 2. Check duplicate College ID
        var existingCollegeId = await _userManager.Users.AnyAsync(u => u.CollegeId == collegeId);
        if (existingCollegeId)
        {
            ModelState.AddModelError(nameof(model.CollegeId), "An account with this College ID already exists.");
            return View(model);
        }

        // 3. Create Student User (Never Admin via registration)
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = model.FullName.Trim(),
            CollegeId = collegeId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        // Assign Student role exclusively
        await _userManager.AddToRoleAsync(user, AppRoles.Student);
        _logger.LogInformation("Student account created: {Email}, CollegeId: {CollegeId}", email, collegeId);

        // 4. Generate verification token and send email
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var verificationUrl = Url.Action(
            nameof(VerifyEmail),
            "Account",
            new { userId = user.Id, token },
            Request.Scheme);

        try
        {
            await _emailService.SendEmailVerificationAsync(
                user.Email!,
                user.FullName,
                verificationUrl ?? string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dispatch verification email to {Email}", user.Email);
        }

        return RedirectToAction(nameof(RegisterConfirmation), new { email = user.Email, userId = user.Id });
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterConfirmation(string email, string? userId)
    {
        ViewBag.Email = email;
        if (!string.IsNullOrEmpty(userId))
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                ViewBag.DevVerifyUrl = Url.Action(nameof(VerifyEmail), "Account", new { userId = user.Id, token }, Request.Scheme);
            }
        }
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> QuickVerify(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user != null)
        {
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
            TempData["SuccessMessage"] = $"Account for '{email}' verified successfully! You can now sign in.";
            return RedirectToAction(nameof(Login));
        }
        return NotFound();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail(string userId, string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            ViewBag.ErrorMessage = "Invalid email verification request parameters.";
            return View("VerifyEmailResult", false);
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            ViewBag.ErrorMessage = "User not found.";
            return View("VerifyEmailResult", false);
        }

        if (user.EmailConfirmed)
        {
            ViewBag.Message = "Your email has already been verified. You can log in now.";
            return View("VerifyEmailResult", true);
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
        {
            _logger.LogInformation("Email verified successfully for user {Email}", user.Email);
            ViewBag.Message = "Your email has been successfully verified! You can now log in to access your student dashboard.";
            return View("VerifyEmailResult", true);
        }

        ViewBag.ErrorMessage = "Email verification link has expired or is invalid. Please request a new one.";
        return View("VerifyEmailResult", false);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email.Trim());
        if (user != null && user.EmailConfirmed)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Action(
                nameof(ResetPassword),
                "Account",
                new { token, email = user.Email },
                Request.Scheme);

            try
            {
                await _emailService.SendPasswordResetAsync(user.Email!, user.FullName, resetLink ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", user.Email);
            }
        }

        // Always show confirmation to prevent user enumeration
        return View("ForgotPasswordConfirmation");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string token, string email)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
        {
            return BadRequest("A token and email must be supplied for password reset.");
        }

        return View(new ResetPasswordViewModel { Token = token, Email = email });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email.Trim());
        if (user == null)
        {
            // Do not reveal that the user does not exist
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
        if (result.Succeeded)
        {
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
