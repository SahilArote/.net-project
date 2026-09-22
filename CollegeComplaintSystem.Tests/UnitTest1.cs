using System.ComponentModel.DataAnnotations;
using CollegeComplaintSystem.Web.ViewModels.Account;
using Xunit;

namespace CollegeComplaintSystem.Tests;

public class AuthenticationAndRegistrationTests
{
    private IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var ctx = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, ctx, validationResults, true);
        return validationResults;
    }

    [Fact]
    public void RegisterViewModel_Fails_WhenRequiredFieldsAreMissing()
    {
        var model = new RegisterViewModel();
        var results = ValidateModel(model);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(RegisterViewModel.FullName)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(RegisterViewModel.CollegeId)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(RegisterViewModel.Email)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(RegisterViewModel.Password)));
    }

    [Fact]
    public void RegisterViewModel_Fails_WhenPasswordsDoNotMatch()
    {
        var model = new RegisterViewModel
        {
            FullName = "Test Student",
            CollegeId = "STU1001",
            Email = "student@college.edu",
            Password = "Password123!",
            ConfirmPassword = "DifferentPassword123!"
        };

        var results = ValidateModel(model);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(RegisterViewModel.ConfirmPassword)));
    }

    [Fact]
    public void RegisterViewModel_Fails_WhenEmailIsInvalid()
    {
        var model = new RegisterViewModel
        {
            FullName = "Test Student",
            CollegeId = "STU1001",
            Email = "not-an-email",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var results = ValidateModel(model);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(RegisterViewModel.Email)));
    }

    [Fact]
    public void RegisterViewModel_DoesNotExpose_RoleSelection()
    {
        // Security requirement: Students cannot specify or select a role during registration
        var properties = typeof(RegisterViewModel).GetProperties();
        var hasRoleProperty = properties.Any(p => p.Name.Equals("Role", StringComparison.OrdinalIgnoreCase) ||
                                                  p.Name.Equals("IsAdmin", StringComparison.OrdinalIgnoreCase));

        Assert.False(hasRoleProperty, "RegisterViewModel must not have any Role or IsAdmin properties that could be manipulated.");
    }
}
