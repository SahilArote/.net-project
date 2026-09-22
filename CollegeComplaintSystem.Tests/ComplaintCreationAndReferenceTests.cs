using CollegeComplaintSystem.Web.Common.Constants;
using CollegeComplaintSystem.Web.Configuration;
using CollegeComplaintSystem.Web.Data;
using CollegeComplaintSystem.Web.Models;
using CollegeComplaintSystem.Web.Models.Enums;
using CollegeComplaintSystem.Web.Services.Implementations;
using CollegeComplaintSystem.Web.Services.Interfaces;
using CollegeComplaintSystem.Web.ViewModels.Complaints;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Text;
using Xunit;

namespace CollegeComplaintSystem.Tests;

public class ComplaintCreationAndReferenceTests
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
    public async Task CreateComplaint_GeneratesCompliantReferenceNumber_AndStartsInSubmittedState()
    {
        var (service, context) = CreateTestFixture();
        var studentId = "student-test";

        var category = new ComplaintCategory { CategoryId = 1, Name = "Electricity", IsActive = true };
        context.ComplaintCategories.Add(category);
        await context.SaveChangesAsync();

        var model = new ComplaintCreateViewModel
        {
            Title = "Fan malfunctioning in Classroom 201",
            CategoryId = 1,
            Location = "Main Building, Room 201",
            Description = "The ceiling fan makes noise and does not rotate properly."
        };

        var result = await service.CreateComplaintAsync(model, studentId);

        Assert.True(result.Success);
        Assert.NotNull(result.Complaint);
        Assert.Equal(ComplaintStatus.Submitted, result.Complaint.Status);

        var currentYear = DateTime.UtcNow.Year;
        Assert.StartsWith($"CMP-{currentYear}-", result.Complaint.ReferenceNumber);
        Assert.Equal(15, result.Complaint.ReferenceNumber.Length); // e.g. CMP-2026-000001 = 15 chars
    }

    [Fact]
    public async Task ImageValidation_RejectsInvalidExtensions_AndLargeFiles()
    {
        var mockSettings = new Mock<IOptions<CloudinarySettings>>();
        mockSettings.Setup(s => s.Value).Returns(new CloudinarySettings());

        var mockEnv = new Mock<IWebHostEnvironment>();
        var mockLogger = new Mock<ILogger<CloudinaryService>>();

        var service = new CloudinaryService(mockSettings.Object, mockEnv.Object, mockLogger.Object);

        // 1. Invalid file extension (.exe)
        var invalidExtensionFile = CreateMockFormFile("malicious.exe", "application/octet-stream", 100);
        var res1 = await service.UploadImageAsync(invalidExtensionFile);
        Assert.False(res1.Success);
        Assert.Contains("Invalid file extension", res1.ErrorMessage);

        // 2. Exceeds 5MB size limit
        var oversizedFile = CreateMockFormFile("photo.jpg", "image/jpeg", 6 * 1024 * 1024);
        var res2 = await service.UploadImageAsync(oversizedFile);
        Assert.False(res2.Success);
        Assert.Contains("exceeds maximum allowed size", res2.ErrorMessage);
    }

    private IFormFile CreateMockFormFile(string fileName, string contentType, int size)
    {
        var stream = new MemoryStream(new byte[size]);
        return new FormFile(stream, 0, size, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}
