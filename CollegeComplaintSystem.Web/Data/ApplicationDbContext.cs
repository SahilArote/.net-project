using CollegeComplaintSystem.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CollegeComplaintSystem.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Complaint> Complaints => Set<Complaint>();
    public DbSet<ComplaintCategory> ComplaintCategories => Set<ComplaintCategory>();
    public DbSet<ComplaintImage> ComplaintImages => Set<ComplaintImage>();
    public DbSet<ComplaintStatusHistory> ComplaintStatusHistories => Set<ComplaintStatusHistory>();
    public DbSet<AdministratorApproval> AdministratorApprovals => Set<AdministratorApproval>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ApplicationUser configuration
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.FullName).HasMaxLength(100).IsRequired();
            entity.Property(u => u.CollegeId).HasMaxLength(50).IsRequired();
            entity.HasIndex(u => u.CollegeId).IsUnique();
        });

        // ComplaintCategory configuration
        builder.Entity<ComplaintCategory>(entity =>
        {
            entity.HasKey(c => c.CategoryId);
            entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
            entity.Property(c => c.Description).HasMaxLength(500);
            entity.HasIndex(c => c.Name).IsUnique();
        });

        // Complaint configuration
        builder.Entity<Complaint>(entity =>
        {
            entity.HasKey(c => c.ComplaintId);
            entity.Property(c => c.ReferenceNumber).HasMaxLength(50).IsRequired();
            entity.HasIndex(c => c.ReferenceNumber).IsUnique();

            entity.Property(c => c.Title).HasMaxLength(150).IsRequired();
            entity.Property(c => c.Description).IsRequired();
            entity.Property(c => c.Location).HasMaxLength(200).IsRequired();

            entity.HasOne(c => c.Student)
                  .WithMany(u => u.Complaints)
                  .HasForeignKey(c => c.StudentId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.Category)
                  .WithMany(cat => cat.Complaints)
                  .HasForeignKey(c => c.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Indexing for common search/filter patterns
            entity.HasIndex(c => c.Status);
            entity.HasIndex(c => c.CreatedAt);
            entity.HasIndex(c => c.StudentId);
            entity.HasIndex(c => c.CategoryId);
        });

        // ComplaintImage configuration
        builder.Entity<ComplaintImage>(entity =>
        {
            entity.HasKey(i => i.ImageId);
            entity.Property(i => i.CloudinaryPublicId).HasMaxLength(200).IsRequired();
            entity.Property(i => i.ImageUrl).HasMaxLength(500).IsRequired();

            entity.HasOne(i => i.Complaint)
                  .WithMany(c => c.Images)
                  .HasForeignKey(i => i.ComplaintId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ComplaintStatusHistory configuration
        builder.Entity<ComplaintStatusHistory>(entity =>
        {
            entity.HasKey(h => h.HistoryId);
            entity.Property(h => h.Notes).HasMaxLength(500);

            entity.HasOne(h => h.Complaint)
                  .WithMany(c => c.StatusHistories)
                  .HasForeignKey(h => h.ComplaintId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(h => h.ChangedByUser)
                  .WithMany()
                  .HasForeignKey(h => h.ChangedByUserId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(h => h.ComplaintId);
            entity.HasIndex(h => h.ChangedAt);
        });

        // AdministratorApproval configuration
        builder.Entity<AdministratorApproval>(entity =>
        {
            entity.HasKey(a => a.ApprovalId);
            entity.Property(a => a.Notes).HasMaxLength(500);

            entity.HasOne(a => a.AdminUser)
                  .WithMany()
                  .HasForeignKey(a => a.AdminUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.ApprovedByAdmin)
                  .WithMany()
                  .HasForeignKey(a => a.ApprovedByAdminId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
