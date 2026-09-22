using CollegeComplaintSystem.Web.Common.Constants;
using CollegeComplaintSystem.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CollegeComplaintSystem.Web.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration,
        ILogger logger)
    {
        // 1. Seed Roles
        string[] roles = [AppRoles.Administrator, AppRoles.Student];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Seeded role: {Role}", role);
            }
        }

        // 2. Seed Default Complaint Categories
        var defaultCategories = new[]
        {
            new ComplaintCategory { Name = "Infrastructure", Description = "Issues with classrooms, furniture, buildings, and campus structures." },
            new ComplaintCategory { Name = "Laboratory", Description = "Equipment, chemicals, computers, or safety issues in labs." },
            new ComplaintCategory { Name = "Library", Description = "Book availability, study space, digital resources, or library systems." },
            new ComplaintCategory { Name = "Washroom & Sanitation", Description = "Cleanliness, hygiene, plumbing, and sanitation facilities." },
            new ComplaintCategory { Name = "Electricity", Description = "Power cuts, lighting, fans, switches, and electrical equipment." },
            new ComplaintCategory { Name = "Water Supply", Description = "Drinking water coolers, restrooms water availability, and filtration." },
            new ComplaintCategory { Name = "Internet & Wi-Fi", Description = "Campus Wi-Fi connectivity, speed, portals, and network access." },
            new ComplaintCategory { Name = "Administration", Description = "Admissions, exams, student documentation, fee counters, and records." },
            new ComplaintCategory { Name = "Other", Description = "General college issues not covered by other categories." }
        };

        foreach (var cat in defaultCategories)
        {
            if (!await context.ComplaintCategories.AnyAsync(c => c.Name == cat.Name))
            {
                context.ComplaintCategories.Add(cat);
            }
        }
        await context.SaveChangesAsync();

        // 3. Seed Initial Administrator Account
        var adminEmail = configuration["AdminSetup:Email"] ?? "admin@college.edu";
        var adminPassword = configuration["AdminSetup:Password"] ?? "Admin@College2026!";
        var adminCollegeId = configuration["AdminSetup:CollegeId"] ?? "ADMIN001";
        var adminFullName = configuration["AdminSetup:FullName"] ?? "System Administrator";

        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = adminFullName,
                CollegeId = adminCollegeId,
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, AppRoles.Administrator);
                logger.LogInformation("Seeded default administrator: {AdminEmail}", adminEmail);
            }
            else
            {
                logger.LogError("Failed to seed administrator: {Errors}",
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }
        }
    }
}
