namespace Application.Dashboard.DTOs;

public record DashboardMonthlyTotalResponse(int Year, int Month, decimal TotalAmount);
