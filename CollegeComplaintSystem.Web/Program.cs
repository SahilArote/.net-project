using CollegeComplaintSystem.Web.Common.Constants;
using CollegeComplaintSystem.Web.Configuration;
using CollegeComplaintSystem.Web.Data;
using CollegeComplaintSystem.Web.Models;
using CollegeComplaintSystem.Web.Services.Implementations;
using CollegeComplaintSystem.Web.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Configuration (Supports SQL Server & SQLite)
var dbProvider = builder.Configuration["DatabaseProvider"] ?? "SqlServer";
if (dbProvider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
{
    var sqliteConnection = builder.Configuration.GetConnectionString("SqliteConnection") 
        ?? "Data Source=CollegeComplaints.db";
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(sqliteConnection));
}
else
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
}

// 2. Identity Configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User & Sign-in settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. Cookie Configuration
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.Name = "CollegeComplaint.Auth";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});

// 4. Strongly-typed Options Configuration
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection(CloudinarySettings.SectionName));
builder.Services.Configure<BrevoSettings>(builder.Configuration.GetSection(BrevoSettings.SectionName));

// 5. Dependency Injection for Application Services
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddHttpClient<IEmailService, BrevoEmailService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IComplaintService, ComplaintService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IReportService, ReportService>();

// 6. Security: Antiforgery
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});

// 7. MVC Controllers with Views
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 8. Auto-migrate / EnsureCreated and Seed Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        if (dbProvider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            await context.Database.EnsureCreatedAsync();
        }
        else
        {
            if (context.Database.CanConnect())
            {
                await context.Database.MigrateAsync();
            }
        }

        await DbInitializer.InitializeAsync(context, userManager, roleManager, app.Configuration, logger);
        
        // In Development, ensure registered students are activated immediately so email service issues don't block login
        if (app.Environment.IsDevelopment())
        {
            var students = await userManager.GetUsersInRoleAsync(AppRoles.Student);
            foreach (var s in students)
            {
                if (!s.EmailConfirmed)
                {
                    s.EmailConfirmed = true;
                    await userManager.UpdateAsync(s);
                    logger.LogInformation("Auto-confirmed student email in development: {Email}", s.Email);
                }
            }
        }

        logger.LogInformation("Database initialized and seeded successfully using {DbProvider}.", dbProvider);
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Could not automatically migrate or seed database on startup ({DbProvider}).", dbProvider);
    }
}

// 9. HTTP Request Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
