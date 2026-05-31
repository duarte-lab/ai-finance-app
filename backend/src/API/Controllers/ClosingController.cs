using Application.MonthlyClosing.DTOs;
using Application.MonthlyClosing.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("closing")]
public class ClosingController : ControllerBase
{
    private readonly GetMonthlyClosingUseCase _getMonthlyClosingUseCase;
    private readonly CreateMonthlyClosingUseCase _createMonthlyClosingUseCase;
    private readonly ReopenMonthlyClosingUseCase _reopenMonthlyClosingUseCase;

    public ClosingController(
        GetMonthlyClosingUseCase getMonthlyClosingUseCase,
        CreateMonthlyClosingUseCase createMonthlyClosingUseCase,
        ReopenMonthlyClosingUseCase reopenMonthlyClosingUseCase)
    {
        _getMonthlyClosingUseCase = getMonthlyClosingUseCase;
        _createMonthlyClosingUseCase = createMonthlyClosingUseCase;
        _reopenMonthlyClosingUseCase = reopenMonthlyClosingUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int year, [FromQuery] int month)
    {
        try
        {
            var result = await _getMonthlyClosingUseCase.ExecuteAsync(year, month);
            return result is null ? NotFound() : Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
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
