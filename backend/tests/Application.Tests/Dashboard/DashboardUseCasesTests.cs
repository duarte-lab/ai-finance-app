using Application.Accounts.Interfaces;
using Application.Dashboard.UseCases;
using Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace Application.Tests.Dashboard;

public class DashboardUseCasesTests
{
    private static Mock<IAccountRepository> CreateRepositoryWithMonth2026_5()
    {
        var repositoryMock = new Mock<IAccountRepository>();

        // the queried month
        repositoryMock
            .Setup(x => x.GetAllAsync(2026, 5))
            .ReturnsAsync(
            [
                new Account { Id = Guid.NewGuid(), Name = "Rent", Amount = 1200m, DueDate = new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc), Paid = true },
                new Account { Id = Guid.NewGuid(), Name = "Internet", Amount = 150m, DueDate = new DateTime(2026, 5, 15, 0, 0, 0, DateTimeKind.Utc), Paid = false },
                new Account { Id = Guid.NewGuid(), Name = "Energy", Amount = 250m, DueDate = new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc), Paid = true }
            ]);

        // last 6 months (other 5)
        repositoryMock.Setup(x => x.GetAllAsync(2025, 12)).ReturnsAsync([]);
        repositoryMock.Setup(x => x.GetAllAsync(2026, 1)).ReturnsAsync([]);
        repositoryMock.Setup(x => x.GetAllAsync(2026, 2)).ReturnsAsync([]);
        repositoryMock.Setup(x => x.GetAllAsync(2026, 3)).ReturnsAsync([]);
        repositoryMock.Setup(x => x.GetAllAsync(2026, 4)).ReturnsAsync([]);

        return repositoryMock;
    }

    [Fact]
    public async Task GetDashboardSummary_ShouldAggregatePaidAndPendingValues()
    {
        var repositoryMock = CreateRepositoryWithMonth2026_5();
        var useCase = new GetDashboardSummaryUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(2026, 5);

        result.Year.Should().Be(2026);
        result.Month.Should().Be(5);
        result.TotalAmount.Should().Be(1600m);
        result.PaidAmount.Should().Be(1450m);
        result.PendingAmount.Should().Be(150m);
        result.TotalCount.Should().Be(3);
        result.PaidCount.Should().Be(2);
        result.PendingCount.Should().Be(1);
        result.Chart.Should().HaveCount(2);
        result.Chart.Should().Contain(x => x.Label == "Paid" && x.Amount == 1450m && x.Count == 2);
        result.Chart.Should().Contain(x => x.Label == "Pending" && x.Amount == 150m && x.Count == 1);
    }

    [Fact]
    public async Task GetDashboardSummary_ShouldReturnPaidSeriesGroupedByDate()
    {
        var repositoryMock = CreateRepositoryWithMonth2026_5();
        var useCase = new GetDashboardSummaryUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(2026, 5);

        result.PaidSeries.Should().HaveCount(2);
        result.PaidSeries.Should().Contain(x => x.Label == "2026-05-10" && x.Amount == 1200m);
        result.PaidSeries.Should().Contain(x => x.Label == "2026-05-20" && x.Amount == 250m);
    }

    [Fact]
    public async Task GetDashboardSummary_ShouldReturnLastSixMonthsTotals()
    {
        var repositoryMock = CreateRepositoryWithMonth2026_5();
        var useCase = new GetDashboardSummaryUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(2026, 5);

        result.LastSixMonths.Should().HaveCount(6);
        result.LastSixMonths.Should().Contain(x => x.Year == 2026 && x.Month == 5 && x.TotalAmount == 1600m);
        result.LastSixMonths.Should().Contain(x => x.Year == 2025 && x.Month == 12 && x.TotalAmount == 0m);
    }
}
