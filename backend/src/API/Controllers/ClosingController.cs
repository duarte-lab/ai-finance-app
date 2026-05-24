using Application.MonthlyClosing.DTOs;
using Application.MonthlyClosing.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("closing")]
public class ClosingController : ControllerBase
{
    private readonly CreateMonthlyClosingUseCase _createMonthlyClosingUseCase;
    private readonly ReopenMonthlyClosingUseCase _reopenMonthlyClosingUseCase;

    public ClosingController(
        CreateMonthlyClosingUseCase createMonthlyClosingUseCase,
        ReopenMonthlyClosingUseCase reopenMonthlyClosingUseCase)
    {
        _createMonthlyClosingUseCase = createMonthlyClosingUseCase;
        _reopenMonthlyClosingUseCase = reopenMonthlyClosingUseCase;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMonthlyClosingRequest request)
    {
        try
        {
            var result = await _createMonthlyClosingUseCase.ExecuteAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("reopen")]
    public async Task<IActionResult> Reopen([FromBody] ReopenMonthlyClosingRequest request)
    {
        try
        {
            var result = await _reopenMonthlyClosingUseCase.ExecuteAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
