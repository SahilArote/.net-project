namespace CollegeComplaintSystem.Web.Common.Constants;

public static class AppRoles
{
    public const string Administrator = "Administrator";
    public const string Student = "Student";
}

public static class AppPolicies
{
    public const string RequireAdmin = "RequireAdmin";
    public const string RequireStudent = "RequireStudent";
    public const string RequireVerifiedEmail = "RequireVerifiedEmail";
}

public static class ComplaintConstants
{
    public const string ReferencePrefix = "CMP";
    public const int MaxImageSizeBytes = 5 * 1024 * 1024; // 5 MB
    public static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    public static readonly string[] AllowedMimeTypes = ["image/jpeg", "image/png", "image/webp"];
}
