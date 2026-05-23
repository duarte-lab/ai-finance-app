using Application.Accounts.Interfaces;
using Application.Dashboard.UseCases;
using Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace Application.Tests.Dashboard;

public class DashboardUseCasesTests
{
    [Fact]
    public async Task GetDashboardSummary_ShouldAggregatePaidAndPendingValues()
    {
        var repositoryMock = new Mock<IAccountRepository>();
        repositoryMock
            .Setup(x => x.GetAllAsync(2026, 5))
            .ReturnsAsync(
            [
                new Account { Id = Guid.NewGuid(), Name = "Rent", Amount = 1200m, DueDate = new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc), Paid = true },
                new Account { Id = Guid.NewGuid(), Name = "Internet", Amount = 150m, DueDate = new DateTime(2026, 5, 15, 0, 0, 0, DateTimeKind.Utc), Paid = false },
                new Account { Id = Guid.NewGuid(), Name = "Energy", Amount = 250m, DueDate = new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc), Paid = true }
            ]);

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
}
