namespace Application.Dashboard.DTOs;

public record DashboardSummaryResponse(
    int Year,
    int Month,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal PendingAmount,
    int TotalCount,
    int PaidCount,
    int PendingCount,
    IReadOnlyCollection<DashboardCategoryPointResponse> Chart,
    IReadOnlyCollection<DashboardCategoryPointResponse> PaidSeries,
    IReadOnlyCollection<DashboardMonthlyTotalResponse> LastSixMonths);
