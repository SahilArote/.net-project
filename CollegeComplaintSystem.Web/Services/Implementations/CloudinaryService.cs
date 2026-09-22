using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using CollegeComplaintSystem.Web.Common.Constants;
using CollegeComplaintSystem.Web.Configuration;
using CollegeComplaintSystem.Web.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CollegeComplaintSystem.Web.Services.Implementations;

public class CloudinaryService : ICloudinaryService
{
    private readonly CloudinarySettings _settings;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<CloudinaryService> _logger;
    private readonly Cloudinary? _cloudinary;

    public CloudinaryService(
        IOptions<CloudinarySettings> settings,
        IWebHostEnvironment environment,
        ILogger<CloudinaryService> logger)
    {
        _settings = settings.Value;
        _environment = environment;
        _logger = logger;

        if (_settings.IsConfigured)
        {
            var account = new Account(_settings.CloudName, _settings.ApiKey, _settings.ApiSecret);
            _cloudinary = new Cloudinary(account)
            {
                Api = { Secure = true }
            };
        }
    }

    public async Task<(bool Success, string? PublicId, string? Url, string? ErrorMessage)> UploadImageAsync(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return (false, null, null, "No image file provided.");
        }

        // 1. Validate file size
        if (file.Length > ComplaintConstants.MaxImageSizeBytes)
        {
            return (false, null, null, $"Image exceeds maximum allowed size of 5 MB ({file.Length / 1024 / 1024} MB provided).");
        }

        // 2. Validate extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!ComplaintConstants.AllowedImageExtensions.Contains(extension))
        {
            return (false, null, null, $"Invalid file extension '{extension}'. Only JPG, PNG, and WEBP are permitted.");
        }

        // 3. Validate MIME type
        var mimeType = file.ContentType.ToLowerInvariant();
        if (!ComplaintConstants.AllowedMimeTypes.Contains(mimeType))
        {
            return (false, null, null, $"Invalid content type '{mimeType}'. Supported image types are JPEG, PNG, WEBP.");
        }

        try
        {
            // If Cloudinary credentials are provided, upload to Cloudinary
            if (_cloudinary != null)
            {
                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "college_complaints",
                    Transformation = new Transformation().Quality("auto").FetchFormat("auto")
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken);
                if (uploadResult.Error != null)
                {
                    _logger.LogError("Cloudinary upload failed: {Message}", uploadResult.Error.Message);
                    return (false, null, null, $"Cloudinary upload error: {uploadResult.Error.Message}");
                }

                _logger.LogInformation("Image uploaded to Cloudinary: {PublicId}", uploadResult.PublicId);
                return (true, uploadResult.PublicId, uploadResult.SecureUrl.ToString(), null);
            }

            // Fallback for local development when Cloudinary credentials are not yet configured
            _logger.LogWarning("Cloudinary credentials not configured. Storing image locally in wwwroot/uploads/complaints for development.");
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "complaints");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream, cancellationToken);
            }

            var localUrl = $"/uploads/complaints/{uniqueFileName}";
            var localPublicId = $"local_{uniqueFileName}";

            return (true, localPublicId, localUrl, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while uploading complaint image.");
            return (false, null, null, "An error occurred while uploading the image. Please try again.");
        }
    }

    public async Task<bool> DeleteImageAsync(string publicId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(publicId)) return false;

        try
        {
            if (publicId.StartsWith("local_"))
            {
                var fileName = publicId.Replace("local_", "");
                var filePath = Path.Combine(_environment.WebRootPath, "uploads", "complaints", fileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                return true;
            }

            if (_cloudinary != null)
            {
                var deletionParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deletionParams);
                return result.Result == "ok";
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete image: {PublicId}", publicId);
            return false;
        }
    }
}
