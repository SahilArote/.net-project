namespace CollegeComplaintSystem.Web.ViewModels.Reports;

public class ReportDashboardViewModel
{
    public int TotalComplaints { get; set; }
    public int SubmittedCount { get; set; }
    public int UnderReviewCount { get; set; }
    public int ResolvedCount { get; set; }
    public int ClosedCount { get; set; }

    public double ResolutionRatePercentage => TotalComplaints > 0
        ? Math.Round(((double)(ResolvedCount + ClosedCount) / TotalComplaints) * 100, 1)
        : 0;

    public List<CategoryReportItem> ComplaintsByCategory { get; set; } = new();
    public List<MonthlyReportItem> MonthlyTrend { get; set; } = new();
}

public class CategoryReportItem
{
    public string CategoryName { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class MonthlyReportItem
{
    public string MonthYear { get; set; } = string.Empty;
    public int Count { get; set; }
}
