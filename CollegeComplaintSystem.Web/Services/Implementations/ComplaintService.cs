using CollegeComplaintSystem.Web.Common.Constants;
using CollegeComplaintSystem.Web.Data;
using CollegeComplaintSystem.Web.Models;
using CollegeComplaintSystem.Web.Models.Enums;
using CollegeComplaintSystem.Web.Services.Interfaces;
using CollegeComplaintSystem.Web.ViewModels.Admin;
using CollegeComplaintSystem.Web.ViewModels.Complaints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CollegeComplaintSystem.Web.Services.Implementations;

public class ComplaintService : IComplaintService
{
    private readonly ApplicationDbContext _context;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IEmailService _emailService;
    private readonly ILogger<ComplaintService> _logger;

    public ComplaintService(
        ApplicationDbContext context,
        ICloudinaryService cloudinaryService,
        IEmailService emailService,
        ILogger<ComplaintService> logger)
    {
        _context = context;
        _cloudinaryService = cloudinaryService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<(bool Success, string? Error, Complaint? Complaint)> CreateComplaintAsync(
        ComplaintCreateViewModel model,
        string studentId,
        CancellationToken cancellationToken = default)
    {
        // 1. Validate Category
        var category = await _context.ComplaintCategories.FindAsync([model.CategoryId], cancellationToken);
        if (category == null || !category.IsActive)
        {
            return (false, "Please select an active complaint category.", null);
        }

        // 2. Upload image if provided
        string? imagePublicId = null;
        string? imageUrl = null;
        if (model.ImageFile != null && model.ImageFile.Length > 0)
        {
            var uploadResult = await _cloudinaryService.UploadImageAsync(model.ImageFile, cancellationToken);
            if (!uploadResult.Success)
            {
                return (false, uploadResult.ErrorMessage ?? "Image upload failed.", null);
            }
            imagePublicId = uploadResult.PublicId;
            imageUrl = uploadResult.Url;
        }

        // 3. Generate Unique Reference Number safely
        var referenceNumber = await GenerateUniqueReferenceNumberAsync(cancellationToken);

        // 4. Create Complaint Entity
        var complaint = new Complaint
        {
            ReferenceNumber = referenceNumber,
            StudentId = studentId,
            CategoryId = model.CategoryId,
            Title = model.Title.Trim(),
            Location = model.Location.Trim(),
            Description = model.Description.Trim(),
            Status = ComplaintStatus.Submitted,
            CreatedAt = DateTime.UtcNow
        };

        if (!string.IsNullOrEmpty(imagePublicId) && !string.IsNullOrEmpty(imageUrl))
        {
            complaint.Images.Add(new ComplaintImage
            {
                CloudinaryPublicId = imagePublicId,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Initial status history
        complaint.StatusHistories.Add(new ComplaintStatusHistory
        {
            PreviousStatus = ComplaintStatus.Submitted,
            NewStatus = ComplaintStatus.Submitted,
            ChangedByUserId = studentId,
            ChangedAt = DateTime.UtcNow,
            Notes = "Complaint submitted by student."
        });

        _context.Complaints.Add(complaint);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Complaint {ReferenceNumber} created by student {StudentId}", referenceNumber, studentId);
        return (true, null, complaint);
    }

    public async Task<ComplaintListViewModel> GetStudentComplaintsAsync(
        string studentId,
        string? search,
        ComplaintStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Complaints
            .AsNoTracking()
            .Include(c => c.Category)
            .Include(c => c.Images)
            .Where(c => c.StudentId == studentId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(c => c.Title.ToLower().Contains(term) ||
                                     c.ReferenceNumber.ToLower().Contains(term) ||
                                     c.Location.ToLower().Contains(term));
        }

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        if (totalPages < 1) totalPages = 1;
        if (page < 1) page = 1;
        if (page > totalPages) page = totalPages;

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ComplaintListItemViewModel
            {
                ComplaintId = c.ComplaintId,
                ReferenceNumber = c.ReferenceNumber,
                Title = c.Title,
                CategoryName = c.Category.Name,
                Location = c.Location,
                Status = c.Status,
                CreatedAt = c.CreatedAt,
                HasImage = c.Images.Any()
            })
            .ToListAsync(cancellationToken);

        return new ComplaintListViewModel
        {
            Search = search,
            Status = status,
            PageNumber = page,
            TotalPages = totalPages,
            TotalCount = totalCount,
            Items = items
        };
    }

    public async Task<Complaint?> GetComplaintForStudentAsync(int complaintId, string studentId, CancellationToken cancellationToken = default)
    {
        return await _context.Complaints
            .Include(c => c.Category)
            .Include(c => c.Images)
            .Include(c => c.StatusHistories.OrderBy(h => h.ChangedAt))
                .ThenInclude(h => h.ChangedByUser)
            .FirstOrDefaultAsync(c => c.ComplaintId == complaintId && c.StudentId == studentId, cancellationToken);
    }

    public async Task<(bool Success, string? Error)> UpdateComplaintAsync(
        ComplaintEditViewModel model,
        string studentId,
        CancellationToken cancellationToken = default)
    {
        var complaint = await _context.Complaints
            .Include(c => c.Images)
            .FirstOrDefaultAsync(c => c.ComplaintId == model.ComplaintId && c.StudentId == studentId, cancellationToken);

        if (complaint == null)
        {
            return (false, "Complaint not found or you do not have permission to modify it.");
        }

        // Strict business rule: Cannot edit if status is anything other than Submitted
        if (complaint.Status != ComplaintStatus.Submitted)
        {
            return (false, $"Complaint cannot be edited because it is already {complaint.Status}. Only Submitted complaints can be edited.");
        }

        var category = await _context.ComplaintCategories.FindAsync([model.CategoryId], cancellationToken);
        if (category == null || !category.IsActive)
        {
            return (false, "Please select an active complaint category.");
        }

        complaint.Title = model.Title.Trim();
        complaint.CategoryId = model.CategoryId;
        complaint.Location = model.Location.Trim();
        complaint.Description = model.Description.Trim();
        complaint.UpdatedAt = DateTime.UtcNow;

        // If replacing image
        if (model.NewImageFile != null && model.NewImageFile.Length > 0)
        {
            var uploadResult = await _cloudinaryService.UploadImageAsync(model.NewImageFile, cancellationToken);
            if (!uploadResult.Success)
            {
                return (false, uploadResult.ErrorMessage ?? "Failed to upload replacement image.");
            }

            // Remove old image
            var oldImage = complaint.Images.FirstOrDefault();
            if (oldImage != null)
            {
                await _cloudinaryService.DeleteImageAsync(oldImage.CloudinaryPublicId, cancellationToken);
                _context.ComplaintImages.Remove(oldImage);
            }

            complaint.Images.Add(new ComplaintImage
            {
                CloudinaryPublicId = uploadResult.PublicId!,
                ImageUrl = uploadResult.Url!,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Complaint {ComplaintId} updated by student {StudentId}", complaint.ComplaintId, studentId);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteComplaintAsync(
        int complaintId,
        string studentId,
        CancellationToken cancellationToken = default)
    {
        var complaint = await _context.Complaints
            .Include(c => c.Images)
            .Include(c => c.StatusHistories)
            .FirstOrDefaultAsync(c => c.ComplaintId == complaintId && c.StudentId == studentId, cancellationToken);

        if (complaint == null)
        {
            return (false, "Complaint not found or you do not have permission to delete it.");
        }

        // Strict business rule: Cannot delete if status is anything other than Submitted
        if (complaint.Status != ComplaintStatus.Submitted)
        {
            return (false, $"Complaint cannot be deleted because it is already {complaint.Status}. Only Submitted complaints can be deleted.");
        }

        // Delete images from cloud
        foreach (var img in complaint.Images)
        {
            await _cloudinaryService.DeleteImageAsync(img.CloudinaryPublicId, cancellationToken);
        }

        _context.Complaints.Remove(complaint);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Complaint {ComplaintId} deleted by student {StudentId}", complaintId, studentId);
        return (true, null);
    }

    public async Task<AdminComplaintListViewModel> GetAdminComplaintsAsync(
        string? search,
        ComplaintStatus? status,
        int? categoryId,
        string? sortBy,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Complaints
            .AsNoTracking()
            .Include(c => c.Student)
            .Include(c => c.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(c => c.ReferenceNumber.ToLower().Contains(term) ||
                                     c.Title.ToLower().Contains(term) ||
                                     c.Student.FullName.ToLower().Contains(term) ||
                                     c.Student.CollegeId.ToLower().Contains(term));
        }

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(c => c.CategoryId == categoryId.Value);
        }

        query = sortBy?.ToLowerInvariant() switch
        {
            "oldest" => query.OrderBy(c => c.CreatedAt),
            "updated" => query.OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt),
            _ => query.OrderByDescending(c => c.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        if (totalPages < 1) totalPages = 1;
        if (page < 1) page = 1;
        if (page > totalPages) page = totalPages;

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new AdminComplaintListItemViewModel
            {
                ComplaintId = c.ComplaintId,
                ReferenceNumber = c.ReferenceNumber,
                StudentName = c.Student.FullName,
                CollegeId = c.Student.CollegeId,
                CategoryName = c.Category.Name,
                Title = c.Title,
                Status = c.Status,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new AdminComplaintListViewModel
        {
            Search = search,
            Status = status,
            CategoryId = categoryId,
            SortBy = sortBy ?? "newest",
            PageNumber = page,
            TotalPages = totalPages,
            TotalCount = totalCount,
            Items = items
        };
    }

    public async Task<Complaint?> GetComplaintForAdminAsync(int complaintId, CancellationToken cancellationToken = default)
    {
        return await _context.Complaints
            .Include(c => c.Student)
            .Include(c => c.Category)
            .Include(c => c.Images)
            .Include(c => c.StatusHistories.OrderBy(h => h.ChangedAt))
                .ThenInclude(h => h.ChangedByUser)
            .FirstOrDefaultAsync(c => c.ComplaintId == complaintId, cancellationToken);
    }

    public bool IsValidStatusTransition(ComplaintStatus current, ComplaintStatus next)
    {
        return (current, next) switch
        {
            (ComplaintStatus.Submitted, ComplaintStatus.UnderReview) => true,
            (ComplaintStatus.UnderReview, ComplaintStatus.Resolved) => true,
            (ComplaintStatus.Resolved, ComplaintStatus.Closed) => true,
            _ => false
        };
    }

    public async Task<(bool Success, string? Error)> UpdateStatusAsync(
        int complaintId,
        ComplaintStatus newStatus,
        string adminUserId,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        var complaint = await _context.Complaints
            .Include(c => c.Student)
            .Include(c => c.StatusHistories)
            .FirstOrDefaultAsync(c => c.ComplaintId == complaintId, cancellationToken);

        if (complaint == null)
        {
            return (false, "Complaint not found.");
        }

        if (!IsValidStatusTransition(complaint.Status, newStatus))
        {
            return (false, $"Invalid status transition from {complaint.Status} to {newStatus}. Valid workflow is Submitted -> Under Review -> Resolved -> Closed.");
        }

        var oldStatus = complaint.Status;
        complaint.Status = newStatus;
        complaint.UpdatedAt = DateTime.UtcNow;

        if (newStatus == ComplaintStatus.Resolved)
        {
            complaint.ResolvedAt = DateTime.UtcNow;
        }
        else if (newStatus == ComplaintStatus.Closed)
        {
            complaint.ClosedAt = DateTime.UtcNow;
        }

        complaint.StatusHistories.Add(new ComplaintStatusHistory
        {
            PreviousStatus = oldStatus,
            NewStatus = newStatus,
            ChangedByUserId = adminUserId,
            ChangedAt = DateTime.UtcNow,
            Notes = notes?.Trim()
        });

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Complaint {ReferenceNumber} status updated from {OldStatus} to {NewStatus} by admin {AdminId}",
            complaint.ReferenceNumber, oldStatus, newStatus, adminUserId);

        // Notify student via email asynchronously
        try
        {
            if (!string.IsNullOrEmpty(complaint.Student.Email))
            {
                await _emailService.SendStatusUpdateNotificationAsync(
                    complaint.Student.Email,
                    complaint.Student.FullName,
                    complaint.ReferenceNumber,
                    complaint.Title,
                    newStatus,
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send status update email for complaint {ReferenceNumber}", complaint.ReferenceNumber);
        }

        return (true, null);
    }

    private async Task<string> GenerateUniqueReferenceNumberAsync(CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"{ComplaintConstants.ReferencePrefix}-{year}-";

        // Find the maximum sequence for the current year
        var latestComplaint = await _context.Complaints
            .Where(c => c.ReferenceNumber.StartsWith(prefix))
            .OrderByDescending(c => c.ComplaintId)
            .Select(c => c.ReferenceNumber)
            .FirstOrDefaultAsync(cancellationToken);

        int nextSequence = 1;
        if (latestComplaint != null && latestComplaint.Length >= prefix.Length + 6)
        {
            var seqString = latestComplaint.Substring(prefix.Length);
            if (int.TryParse(seqString, out var currentSeq))
            {
                nextSequence = currentSeq + 1;
            }
        }

        // Keep incrementing if there is an unexpected collision
        while (true)
        {
            var refNumber = $"{prefix}{nextSequence:D6}";
            var exists = await _context.Complaints.AnyAsync(c => c.ReferenceNumber == refNumber, cancellationToken);
            if (!exists)
            {
                return refNumber;
            }
            nextSequence++;
        }
    }
}
