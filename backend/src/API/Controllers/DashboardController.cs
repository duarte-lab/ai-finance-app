using Application.Dashboard.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly GetDashboardSummaryUseCase _getDashboardSummaryUseCase;

    public DashboardController(GetDashboardSummaryUseCase getDashboardSummaryUseCase)
    {
        _getDashboardSummaryUseCase = getDashboardSummaryUseCase;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] int? year = null, [FromQuery] int? month = null)
    {
        var now = DateTime.UtcNow;
        var resolvedYear = year ?? now.Year;
        var resolvedMonth = month ?? now.Month;

        if (resolvedMonth is < 1 or > 12)
        {
            return BadRequest("Month must be between 1 and 12.");
        }

        if (resolvedYear <= 0)
        {
            return BadRequest("Year must be greater than 0.");
        }

        var summary = await _getDashboardSummaryUseCase.ExecuteAsync(resolvedYear, resolvedMonth);
        return Ok(summary);
    }
}
