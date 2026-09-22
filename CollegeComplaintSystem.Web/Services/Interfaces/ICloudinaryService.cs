using Microsoft.AspNetCore.Http;

namespace CollegeComplaintSystem.Web.Services.Interfaces;

public interface ICloudinaryService
{
    Task<(bool Success, string? PublicId, string? Url, string? ErrorMessage)> UploadImageAsync(IFormFile file, CancellationToken cancellationToken = default);
    Task<bool> DeleteImageAsync(string publicId, CancellationToken cancellationToken = default);
}
