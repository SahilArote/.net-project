using CollegeComplaintSystem.Web.Data;
using CollegeComplaintSystem.Web.Models.Enums;
using CollegeComplaintSystem.Web.Services.Implementations;
using CollegeComplaintSystem.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CollegeComplaintSystem.Tests;

public class ComplaintStatusTransitionTests
{
    private ComplaintService CreateComplaintService(ApplicationDbContext context)
    {
        var mockCloudinary = new Mock<ICloudinaryService>();
        var mockEmail = new Mock<IEmailService>();
        var mockLogger = new Mock<ILogger<ComplaintService>>();

        return new ComplaintService(context, mockCloudinary.Object, mockEmail.Object, mockLogger.Object);
    }

    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Theory]
    [InlineData(ComplaintStatus.Submitted, ComplaintStatus.UnderReview, true)]
    [InlineData(ComplaintStatus.UnderReview, ComplaintStatus.Resolved, true)]
    [InlineData(ComplaintStatus.Resolved, ComplaintStatus.Closed, true)]
    [InlineData(ComplaintStatus.Submitted, ComplaintStatus.Resolved, false)]
    [InlineData(ComplaintStatus.Submitted, ComplaintStatus.Closed, false)]
    [InlineData(ComplaintStatus.UnderReview, ComplaintStatus.Submitted, false)]
    [InlineData(ComplaintStatus.UnderReview, ComplaintStatus.Closed, false)]
    [InlineData(ComplaintStatus.Resolved, ComplaintStatus.Submitted, false)]
    [InlineData(ComplaintStatus.Resolved, ComplaintStatus.UnderReview, false)]
    [InlineData(ComplaintStatus.Closed, ComplaintStatus.Submitted, false)]
    [InlineData(ComplaintStatus.Closed, ComplaintStatus.UnderReview, false)]
    [InlineData(ComplaintStatus.Closed, ComplaintStatus.Resolved, false)]
    public void IsValidStatusTransition_ValidatesAllowedWorkflow(
        ComplaintStatus current,
        ComplaintStatus next,
        bool expectedValid)
    {
        using var context = CreateInMemoryDbContext();
        var service = CreateComplaintService(context);

        var isValid = service.IsValidStatusTransition(current, next);

        Assert.Equal(expectedValid, isValid);
    }
}
