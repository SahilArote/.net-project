using CollegeComplaintSystem.Web.Data;
using CollegeComplaintSystem.Web.Models;
using CollegeComplaintSystem.Web.Models.Enums;
using CollegeComplaintSystem.Web.Services.Implementations;
using CollegeComplaintSystem.Web.Services.Interfaces;
using CollegeComplaintSystem.Web.ViewModels.Complaints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CollegeComplaintSystem.Tests;

public class ComplaintPermissionAndOwnershipTests
{
    private (ComplaintService Service, ApplicationDbContext Context) CreateTestFixture()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);

        var mockCloudinary = new Mock<ICloudinaryService>();
        var mockEmail = new Mock<IEmailService>();
        var mockLogger = new Mock<ILogger<ComplaintService>>();

        var service = new ComplaintService(context, mockCloudinary.Object, mockEmail.Object, mockLogger.Object);
        return (service, context);
    }

    [Fact]
    public async Task Student_CanEdit_WhenStatusIsSubmitted()
    {
        var (service, context) = CreateTestFixture();
        var studentId = "student-123";

        var category = new ComplaintCategory { CategoryId = 1, Name = "Infrastructure", IsActive = true };
        context.ComplaintCategories.Add(category);

        var complaint = new Complaint
        {
            ComplaintId = 1,
            ReferenceNumber = "CMP-2026-000001",
            StudentId = studentId,
            CategoryId = 1,
            Title = "Original Title",
            Location = "Room 101",
            Description = "Original Description",
            Status = ComplaintStatus.Submitted
        };
        context.Complaints.Add(complaint);
        await context.SaveChangesAsync();

        var editModel = new ComplaintEditViewModel
        {
            ComplaintId = 1,
            Title = "Updated Title",
            CategoryId = 1,
            Location = "Room 102",
            Description = "Updated Description"
        };

        var result = await service.UpdateComplaintAsync(editModel, studentId);

        Assert.True(result.Success);
        var updated = await context.Complaints.FindAsync(1);
        Assert.Equal("Updated Title", updated!.Title);
    }

    [Theory]
    [InlineData(ComplaintStatus.UnderReview)]
    [InlineData(ComplaintStatus.Resolved)]
    [InlineData(ComplaintStatus.Closed)]
    public async Task Student_CannotEdit_WhenStatusIsNotSubmitted(ComplaintStatus nonSubmittedStatus)
    {
        var (service, context) = CreateTestFixture();
        var studentId = "student-123";

        var category = new ComplaintCategory { CategoryId = 1, Name = "Infrastructure", IsActive = true };
        context.ComplaintCategories.Add(category);

        var complaint = new Complaint
        {
            ComplaintId = 2,
            ReferenceNumber = "CMP-2026-000002",
            StudentId = studentId,
            CategoryId = 1,
            Title = "Original Title",
            Location = "Room 101",
            Description = "Original Description",
            Status = nonSubmittedStatus
        };
        context.Complaints.Add(complaint);
        await context.SaveChangesAsync();

        var editModel = new ComplaintEditViewModel
        {
            ComplaintId = 2,
            Title = "Hacked Title",
            CategoryId = 1,
            Location = "Room 102",
            Description = "Hacked Description"
        };

        var result = await service.UpdateComplaintAsync(editModel, studentId);

        Assert.False(result.Success);
        Assert.Contains("Only Submitted complaints can be edited", result.Error);
    }

    [Fact]
    public async Task Student_CanDelete_WhenStatusIsSubmitted()
    {
        var (service, context) = CreateTestFixture();
        var studentId = "student-123";

        var complaint = new Complaint
        {
            ComplaintId = 3,
            ReferenceNumber = "CMP-2026-000003",
            StudentId = studentId,
            CategoryId = 1,
            Title = "To Be Deleted",
            Location = "Room 101",
            Description = "Description",
            Status = ComplaintStatus.Submitted
        };
        context.Complaints.Add(complaint);
        await context.SaveChangesAsync();

        var result = await service.DeleteComplaintAsync(3, studentId);

        Assert.True(result.Success);
        var deleted = await context.Complaints.FindAsync(3);
        Assert.Null(deleted);
    }

    [Theory]
    [InlineData(ComplaintStatus.UnderReview)]
    [InlineData(ComplaintStatus.Resolved)]
    [InlineData(ComplaintStatus.Closed)]
    public async Task Student_CannotDelete_WhenStatusIsNotSubmitted(ComplaintStatus nonSubmittedStatus)
    {
        var (service, context) = CreateTestFixture();
        var studentId = "student-123";

        var complaint = new Complaint
        {
            ComplaintId = 4,
            ReferenceNumber = "CMP-2026-000004",
            StudentId = studentId,
            CategoryId = 1,
            Title = "Under Review Complaint",
            Location = "Room 101",
            Description = "Description",
            Status = nonSubmittedStatus
        };
        context.Complaints.Add(complaint);
        await context.SaveChangesAsync();

        var result = await service.DeleteComplaintAsync(4, studentId);

        Assert.False(result.Success);
        Assert.Contains("Only Submitted complaints can be deleted", result.Error);
    }

    [Fact]
    public async Task OwnershipProtection_StudentACannotAccessOrModify_StudentBComplaint()
    {
        var (service, context) = CreateTestFixture();
        var studentA = "student-A";
        var studentB = "student-B";

        var category = new ComplaintCategory { CategoryId = 1, Name = "Infrastructure", IsActive = true };
        context.ComplaintCategories.Add(category);

        var complaintOfStudentB = new Complaint
        {
            ComplaintId = 5,
            ReferenceNumber = "CMP-2026-000005",
            StudentId = studentB,
            CategoryId = 1,
            Title = "Student B's Complaint",
            Location = "Room 101",
            Description = "Description",
            Status = ComplaintStatus.Submitted
        };
        context.Complaints.Add(complaintOfStudentB);
        await context.SaveChangesAsync();

        // 1. Student A tries to view Student B's complaint
        var fetched = await service.GetComplaintForStudentAsync(5, studentA);
        Assert.Null(fetched);

        // 2. Student A tries to edit Student B's complaint
        var editModel = new ComplaintEditViewModel
        {
            ComplaintId = 5,
            Title = "Tampered Title",
            CategoryId = 1,
            Location = "Room 101",
            Description = "Description"
        };
        var editResult = await service.UpdateComplaintAsync(editModel, studentA);
        Assert.False(editResult.Success);

        // 3. Student A tries to delete Student B's complaint
        var deleteResult = await service.DeleteComplaintAsync(5, studentA);
        Assert.False(deleteResult.Success);
    }
}
