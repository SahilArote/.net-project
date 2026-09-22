using System.Reflection;
using CollegeComplaintSystem.Web.Common.Constants;
using CollegeComplaintSystem.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace CollegeComplaintSystem.Tests;

public class RoleAndAuthorizationPolicyTests
{
    [Fact]
    public void Roles_AreDistinctAndWellDefined()
    {
        Assert.NotEqual(AppRoles.Administrator, AppRoles.Student);
        Assert.Equal("Administrator", AppRoles.Administrator);
        Assert.Equal("Student", AppRoles.Student);
    }

    [Theory]
    [InlineData(typeof(AdminController), AppRoles.Administrator)]
    [InlineData(typeof(CategoryController), AppRoles.Administrator)]
    [InlineData(typeof(ReportController), AppRoles.Administrator)]
    [InlineData(typeof(StudentController), AppRoles.Student)]
    [InlineData(typeof(ComplaintController), AppRoles.Student)]
    public void Controllers_AreProtectedWithAppropriateRoles(Type controllerType, string expectedRole)
    {
        var authAttribute = controllerType.GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(authAttribute);
        Assert.Equal(expectedRole, authAttribute.Roles);
    }
}
